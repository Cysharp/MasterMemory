#nullable disable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MasterMemory.GeneratorCore
{
    internal static class CodeGenerator
    {
        // return GenerationContext?
        public static GenerationContext CreateGenerationContext(TypeDeclarationSyntax classDecl, AttributeData memoryTableAttribute, DiagnosticReporter reporter)
        {
            var context = new GenerationContext();

            context.ClassName = classDecl.Identifier.ToFullString().Trim();
            context.MemoryTableName = memoryTableAttribute.ConstructorArguments[0].Value as string ?? context.ClassName;

            var hasError = false;
            var members = classDecl.Members.OfType<PropertyDeclarationSyntax>()
                .Select(x =>
                {
                    var prop = ExtractPropertyAttribute(x, reporter);
                    if (prop == null)
                    {
                        hasError = true;
                        return default!;
                    }
                    return prop.Value;
                })
                .ToArray();
            if (hasError) return null;

            var primaryKey = AggregatePrimaryKey(members.Where(x => x.Item1 != null).Select(x => x.Item1));
            if (primaryKey.Properties.Length == 0)
            {
                reporter.ReportDiagnostic(DiagnosticDescriptors.RequirePrimaryKey, classDecl.Identifier.GetLocation(), context.ClassName);
                return null;
            }

            var secondaryKeys = members.SelectMany(x => x.Item2).GroupBy(x => x.IndexNo).Select(x => AggregateSecondaryKey(x)).ToArray();
            var properties = members.Where(x => x.Item3 != null).Select(x => new Property
            {
                Type = x.Item3.Type.ToFullStringTrim(),
                Name = x.Item3.Identifier.Text,
            }).ToArray();

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

            context.PrimaryKey = primaryKey;
            context.SecondaryKeys = secondaryKeys;
            context.Properties = properties;
            context.UsingStrings = usingStrings;
            context.OriginalClassDeclaration = classDecl;
            return context;
        }


        static (PrimaryKey, List<SecondaryKey>, PropertyDeclarationSyntax)? ExtractPropertyAttribute(PropertyDeclarationSyntax property, DiagnosticReporter reporter)
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
                    if (attrName == "PrimaryKey" || attrName == "MasterMemory.PrimaryKey")
                    {
                        if (resultPrimaryKey != null)
                        {
                            // PrimaryKey is AllowMultiple:false so this code is dead
                            reporter.ReportDiagnostic(DiagnosticDescriptors.DuplicatePrimaryKey, property.Identifier.GetLocation(), property.Type.ToFullString(), property.Identifier.ToFullString());
                            return null;
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
                    else if (attrName == "SecondaryKey" || attrName == "MasterMemory.SecondaryKey")
                    {
                        if (secondaryKey != null)
                        {
                            reporter.ReportDiagnostic(DiagnosticDescriptors.DuplicateSecondaryKey, property.Identifier.GetLocation(), property.Type.ToFullString(), property.Identifier.ToFullString());
                            return null;
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
                    else if (attrName == "NonUnique" || attrName == "MasterMemory.NonUnique")
                    {
                        hasNonUnique = true;
                    }
                    else if (attrName == "StringComparisonOption" || attrName == "MasterMemory.StringComparisonOption")
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
