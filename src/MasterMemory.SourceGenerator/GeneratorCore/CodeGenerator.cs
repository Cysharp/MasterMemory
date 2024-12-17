#nullable disable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MasterMemory.GeneratorCore
{
    public static class CodeGenerator
    {
        public static GenerationContext CreateGenerationContext(ClassDeclarationSyntax classDecl)
        {
            var root = classDecl.SyntaxTree.GetRoot();

            var ns = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
                .Select(x => "using " + x.Name.ToFullStringTrim() + ";")
                .ToArray();

            var usingStrings = root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(x => x.ToFullString().Trim())
                .Concat(new[] { "using MasterMemory", "using MasterMemory.Validation", "using System", "using System.Collections.Generic" })
                .Concat(ns)
                .Select(x => x.Trim(';') + ";")
                .Distinct()
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();

            var context = new GenerationContext();

            foreach (var attr in classDecl.AttributeLists.SelectMany(x => x.Attributes))
            {
                var attrName = attr.Name.ToFullString().Trim();
                if (attrName == "MemoryTable" || attrName == "MasterMemory.Annotations.MemoryTable")
                {
                    context.ClassName = classDecl.Identifier.ToFullString().Trim();
                    context.MemoryTableName = AttributeExpressionToString(attr.ArgumentList.Arguments[0].Expression) ?? context.ClassName;

                    var members = classDecl.Members.OfType<PropertyDeclarationSyntax>()
                        .Select(x => ExtractPropertyAttribute(x))
                        .ToArray();

                    var primaryKey = AggregatePrimaryKey(members.Where(x => x.Item1 != null).Select(x => x.Item1));
                    if (primaryKey.Properties.Length == 0)
                    {
                        throw new InvalidOperationException("MemoryTable does not found PrimaryKey property, Type:" + context.ClassName);
                    }

                    var secondaryKeys = members.SelectMany(x => x.Item2).GroupBy(x => x.IndexNo).Select(x => AggregateSecondaryKey(x)).ToArray();
                    var properties = members.Where(x => x.Item3 != null).Select(x => new Property
                    {
                        Type = x.Item3.Type.ToFullStringTrim(),
                        Name = x.Item3.Identifier.Text,
                    }).ToArray();

                    context.PrimaryKey = primaryKey;
                    context.SecondaryKeys = secondaryKeys;
                    context.Properties = properties;
                }
            }

            if (context.PrimaryKey != null)
            {
                context.UsingStrings = usingStrings;
                context.OriginalClassDeclaration = classDecl;
                // context.InputFilePath = filePath;
                return context;
            }

            return null!; // if null????
        }


        static (PrimaryKey, List<SecondaryKey>, PropertyDeclarationSyntax) ExtractPropertyAttribute(PropertyDeclarationSyntax property)
        {
            // Attribute Parterns:
            // Primarykey(keyOrder = 0)
            // SecondaryKey(indexNo, keyOrder = 0)
            // NonUnique
            // StringComparisonOption

            PrimaryKey resultPrimaryKey = default;
            List<SecondaryKey> secondaryKeys = new List<SecondaryKey>();
            bool isSerializableProperty = true;

            foreach (var attrList in property.AttributeLists)
            {
                var hasNonUnique = false;
                PrimaryKey primaryKey = default;
                SecondaryKey secondaryKey = default;

                foreach (var attr in attrList.Attributes)
                {
                    var attrName = attr.Name.ToFullString().Trim();
                    if (attrName == "PrimaryKey" || attrName == "MasterMemory.Annotations.PrimaryKey")
                    {
                        if (resultPrimaryKey != null)
                        {
                            throw new InvalidOperationException("Duplicate PrimaryKey:" + property.Type.ToFullString() + "." + property.Identifier.ToFullString());
                        }

                        primaryKey = new PrimaryKey();
                        var keyProperty = new KeyProperty()
                        {
                            Name = property.Identifier.ToFullStringTrim(),
                            TypeName = property.Type.ToFullStringTrim()
                        };

                        foreach (var arg in attr.ArgumentList?.Arguments ?? default)
                        {
                            keyProperty.KeyOrder = (int)((arg.Expression as LiteralExpressionSyntax).Token.Value);
                        }

                        primaryKey.Properties = new[] { keyProperty };
                    }
                    else if (attrName == "SecondaryKey" || attrName == "MasterMemory.Annotations.SecondaryKey")
                    {
                        if (secondaryKey != null)
                        {
                            throw new InvalidOperationException("Duplicate SecondaryKey, doesn't allow to add multiple attribute in same attribute list:" + property.Type.ToFullString() + "." + property.Identifier.ToFullString());
                        }

                        secondaryKey = new SecondaryKey();
                        var keyProperty = new KeyProperty()
                        {
                            Name = property.Identifier.ToFullStringTrim(),
                            TypeName = property.Type.ToFullStringTrim()
                        };

                        var args = attr.ArgumentList.Arguments;
                        secondaryKey.IndexNo = (int)((args[0].Expression as LiteralExpressionSyntax).Token.Value);
                        if (args.Count == 2)
                        {
                            keyProperty.KeyOrder = (int)((args[1].Expression as LiteralExpressionSyntax).Token.Value);
                        }
                        secondaryKey.Properties = new[] { keyProperty };
                    }
                    else if (attrName == "NonUnique" || attrName == "MasterMemory.Annotations.NonUnique")
                    {
                        hasNonUnique = true;
                    }
                    else if (attrName == "StringComparisonOption" || attrName == "MasterMemory.Annotations.StringComparisonOption")
                    {
                        var option = (attr.ArgumentList.Arguments[0].Expression as MemberAccessExpressionSyntax).ToFullStringTrim();
                        if (primaryKey != null)
                        {
                            primaryKey.StringComparisonOption = option;
                        }
                        if (secondaryKey != null)
                        {
                            secondaryKey.StringComparisonOption = option;
                        }
                    }
                    else if (!property.Modifiers.Any(SyntaxKind.PublicKeyword)
                        || attrName == "IgnoreMember" || attrName == "MessagePack.IgnoreMember"
                        || attrName == "IgnoreDataMember" || attrName == "System.Runtime.Serialization.IgnoreDataMember")
                    {
                        isSerializableProperty = false;
                    }
                }

                if (hasNonUnique)
                {
                    if (primaryKey != null)
                    {
                        primaryKey.IsNonUnique = true;
                    }
                    if (secondaryKey != null)
                    {
                        secondaryKey.IsNonUnique = true;
                    }
                }

                if (primaryKey != null)
                {
                    resultPrimaryKey = primaryKey;
                }
                if (secondaryKey != null)
                {
                    secondaryKeys.Add(secondaryKey);
                }
            }

            return (resultPrimaryKey, secondaryKeys, isSerializableProperty ? property : null);
        }

        static PrimaryKey AggregatePrimaryKey(IEnumerable<PrimaryKey> primaryKeys)
        {
            var primarykey = new PrimaryKey();
            var list = new List<KeyProperty>();

            foreach (var item in primaryKeys)
            {
                if (item.IsNonUnique) primarykey.IsNonUnique = true;
                if (item.StringComparisonOption != null) primarykey.StringComparisonOption = item.StringComparisonOption;

                list.AddRange(item.Properties);
            }

            primarykey.Properties = list.OrderBy(x => x.KeyOrder).ToArray();

            return primarykey;
        }

        // grouped by IndexNo.
        static SecondaryKey AggregateSecondaryKey(IGrouping<int, SecondaryKey> secondaryKeys)
        {
            var secondaryKey = new SecondaryKey();
            secondaryKey.IndexNo = secondaryKeys.Key;

            var list = new List<KeyProperty>();

            foreach (var item in secondaryKeys)
            {
                if (item.IsNonUnique) secondaryKey.IsNonUnique = true;
                if (item.StringComparisonOption != null) secondaryKey.StringComparisonOption = item.StringComparisonOption;

                list.AddRange(item.Properties);
            }

            secondaryKey.Properties = list.OrderBy(x => x.KeyOrder).ToArray();
            return secondaryKey;
        }

        static string AttributeExpressionToString(ExpressionSyntax expression)
        {
            if (expression is InvocationExpressionSyntax ie)
            {
                var expr = ie.ArgumentList.Arguments.Last().Expression;
                if (expr is MemberAccessExpressionSyntax mae)
                {
                    return mae.Name?.ToString();
                }
                else if (expr is IdentifierNameSyntax inx)
                {
                    return inx.Identifier.ValueText;
                }
                return null;
            }
            else if (expression is LiteralExpressionSyntax le)
            {
                return le.Token.ValueText;
            }
            return null;
        }
    }

    internal static class Extensions
    {
        public static string ToFullStringTrim(this SyntaxNode node)
        {
            return node.ToFullString().Trim();
        }

        public static string ToFullStringTrim(this SyntaxToken token)
        {
            return token.ToFullString().Trim();
        }
    }
}
