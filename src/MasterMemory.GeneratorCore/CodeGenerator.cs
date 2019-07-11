using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MasterMemory.GeneratorCore
{
    public class CodeGenerator
    {
        public void GenerateFile(string usingNamespace, string inputDirectory, string outputDirectory, Action<string> logger)
        {
            var list = new List<GenerationContext>();

            // Collect
            foreach (var item in Directory.GetFiles(inputDirectory, "*.cs", SearchOption.AllDirectories))
            {
                list.AddRange(CreateGenerationContext(item));
            }

            if (list.Count == 0)
            {
                throw new InvalidOperationException("Not found MemoryTable files, inputDir:" + inputDirectory);
            }

            // Output
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                var usingStrings = string.Join(Environment.NewLine, list.SelectMany(x => x.UsingStrings).Distinct().OrderBy(x => x));

                var builderTemplate = new DatabaseBuilderTemplate();
                var databaseTemplate = new MemoryDatabaseTemplate();
                var immutableBuilderTemplate = new ImmutableBuilderTemplate();
                var resolverTemplate = new MessagePackResolverTemplate();
                builderTemplate.Namespace = databaseTemplate.Namespace = immutableBuilderTemplate.Namespace = resolverTemplate.Namespace = usingNamespace;
                builderTemplate.Using = databaseTemplate.Using = immutableBuilderTemplate.Using = resolverTemplate.Using = usingStrings + Environment.NewLine + ("using " + usingNamespace + ".Tables;");
                builderTemplate.GenerationContexts = databaseTemplate.GenerationContexts = immutableBuilderTemplate.GenerationContexts = resolverTemplate.GenerationContexts = list.ToArray();

                logger(WriteToFile(outputDirectory, "DatabaseBuilder", builderTemplate.TransformText()));
                logger(WriteToFile(outputDirectory, "ImmutableBuilder", immutableBuilderTemplate.TransformText()));
                logger(WriteToFile(outputDirectory, "MemoryDatabase", databaseTemplate.TransformText()));
                logger(WriteToFile(outputDirectory, "MasterMemoryResolver", resolverTemplate.TransformText()));
            }
            {
                var tableDir = Path.Combine(outputDirectory, "Tables");
                if (!Directory.Exists(tableDir))
                {
                    Directory.CreateDirectory(tableDir);
                }

                foreach (var context in list)
                {
                    var template = new TableTemplate()
                    {
                        Namespace = usingNamespace,
                        GenerationContext = context,
                        Using = string.Join(Environment.NewLine, context.UsingStrings)
                    };

                    logger(WriteToFile(tableDir, context.ClassName + "Table", template.TransformText()));
                }
            }
        }

        static string WriteToFile(string directory, string fileName, string content)
        {
            var path = Path.Combine(directory, fileName + ".cs");
            File.WriteAllText(path, content, Encoding.UTF8);
            return $"Generate {fileName} to: {path}";
        }

        IEnumerable<GenerationContext> CreateGenerationContext(string filePath)
        {
            var syntax = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
            var root = syntax.GetRoot();

            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();
            if (classDeclarations.Length == 0) yield break;

            var ns = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
                .Select(x => "using " + x.Name.ToFullStringTrim() + ";")
                .ToArray();

            var usingStrings = root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(x => x.ToFullString().Trim())
                .Concat(new[] { "using MasterMemory;", "using System;", "using System.Collections.Generic;" })
                .Concat(ns)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            foreach (var classDecl in classDeclarations)
            {
                var context = new GenerationContext();

                foreach (var attr in classDecl.AttributeLists.SelectMany(x => x.Attributes))
                {
                    var attrName = attr.Name.ToFullString().Trim();
                    if (attrName == "MemoryTable" || attrName == "MasterMemory.Annotations.MemoryTable")
                    {
                        context.ClassName = classDecl.Identifier.ToFullString().Trim();

                        var members = classDecl.Members.OfType<PropertyDeclarationSyntax>()
                            .Select(x => ExtractPropertyAttribute(x))
                            .ToArray();

                        var primaryKey = AggregatePrimaryKey(members.Where(x => x.Item1 != null).Select(x => x.Item1));
                        if (primaryKey.Properties.Length == 0)
                        {
                            throw new InvalidOperationException("MemoryTable does not found PrimaryKey property, Type:" + context.ClassName);
                        }

                        var secondaryKeys = members.SelectMany(x => x.Item2).GroupBy(x => x.IndexNo).Select(x => AggregateSecondaryKey(x)).ToArray();

                        context.PrimaryKey = primaryKey;
                        context.SecondaryKeys = secondaryKeys;
                    }
                }

                if (context.PrimaryKey != null)
                {
                    context.UsingStrings = usingStrings;
                    yield return context;
                }
            }
        }

        (PrimaryKey, List<SecondaryKey>) ExtractPropertyAttribute(PropertyDeclarationSyntax property)
        {
            // Attribute Parterns:
            // Primarykey(keyOrder = 0)
            // SecondaryKey(indexNo, keyOrder = 0)
            // NonUnique
            // StringComparisonOption

            PrimaryKey resultPrimaryKey = default;
            List<SecondaryKey> secondaryKeys = new List<SecondaryKey>();

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

            return (resultPrimaryKey, secondaryKeys);
        }

        PrimaryKey AggregatePrimaryKey(IEnumerable<PrimaryKey> primaryKeys)
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
        SecondaryKey AggregateSecondaryKey(IGrouping<int, SecondaryKey> secondaryKeys)
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
