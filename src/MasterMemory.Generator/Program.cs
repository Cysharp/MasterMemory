using MasterMemory.GeneratorCore;
using System.Linq;
using MicroBatchFramework;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MasterMemory.Generator
{
    public class Program : BatchBase
    {
        static async Task Main(string[] args)
        {
            await BatchHost.CreateDefaultBuilder().RunBatchEngineAsync<Program>(args);
        }

        public void Execute()
        {
            new CodeAnalyzer(@"C:\GitHubRepositories\MasterMemory\src\MasterMemory2\", "Foo");
        }
    }

    public static class Extensions
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

    public class CodeAnalyzer
    {
        public CodeAnalyzer(string directoryPath, string outputNamespace)
        {
            var list = new List<GenerationContext>();

            // Collect
            foreach (var item in Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories))
            {
                var syntax = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(File.ReadAllText(item));
                var root = syntax.GetRoot();

                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();
                if (classDeclarations.Length == 0) continue;

                var ns = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
                    .Select(x => "using " + x.Name.ToFullStringTrim() + ";")
                    .ToArray();

                var usingStrings = root.DescendantNodes()
                    .OfType<UsingDirectiveSyntax>()
                    .Select(x => x.ToFullString().Trim())
                    .Concat(new[] { "using MasterMemory;", "using System;" })
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
                        list.Add(context);
                    }
                }
            }

            // Output
            {
                var usingStrings = string.Join(Environment.NewLine, list.SelectMany(x => x.UsingStrings).Distinct().OrderBy(x => x));

                var builderTemplate = new DatabaseBuilderTemplate();
                var databaseTemplate = new MemoryDatabaseTemplate();
                var immutableBuilderTemplate = new ImmutableBuilderTemplate();
                builderTemplate.Namespace = databaseTemplate.Namespace = immutableBuilderTemplate.Namespace = outputNamespace;
                builderTemplate.Using = databaseTemplate.Using = immutableBuilderTemplate.Using = usingStrings + Environment.NewLine + ("using " + outputNamespace + ".Tables;");
                builderTemplate.GenerationContexts = databaseTemplate.GenerationContexts = immutableBuilderTemplate.GenerationContexts = list.ToArray();

                var builder = builderTemplate.TransformText();
                var database = databaseTemplate.TransformText();
                var immutableBuilder = immutableBuilderTemplate.TransformText();
            }
            {
                foreach (var context in list)
                {
                    var template = new TableTemplate()
                    {
                        Namespace = outputNamespace,
                        GenerationContext = context,
                        Using = string.Join(Environment.NewLine, context.UsingStrings)
                    };

                    var table = template.TransformText();
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


    public class GenerationContext
    {
        public string ClassName { get; set; }
        public string[] UsingStrings { get; set; }
        public PrimaryKey PrimaryKey { get; set; }
        public SecondaryKey[] SecondaryKeys { get; set; }

    }

    public abstract class KeyBase
    {
        public bool IsNonUnique { get; set; }
        public string StringComparisonOption { get; set; }
        public KeyProperty[] Properties { get; set; }
        public abstract string SelectorName { get; }

        public string BuildKeyAccessor(string lambdaArgument)
        {
            if (Properties.Length == 1)
            {
                return lambdaArgument + "." + Properties[0].Name;
            }
            else
            {
                return "(" + string.Join(", ", Properties.Select(x => lambdaArgument + "." + x.Name)) + ")";
            }
        }

        public string BuildTypeName()
        {
            if (Properties.Length == 1)
            {
                return Properties[0].TypeName;
            }
            else
            {
                return "(" + string.Join(", ", Properties.Select(x => x.TypeName + " " + x.Name)) + ")";
            }
        }

        public string BuildMethodName()
        {
            if (Properties.Length == 1)
            {
                return Properties[0].Name;
            }
            else
            {
                return string.Join("And", Properties.Select(x => x.Name));
            }
        }

        public string BuildFindPrefix()
        {
            return IsNonUnique ? "FindMany" : "FindUnique";
        }

        public string BuildReturnTypeName(string elementName)
        {
            return IsNonUnique ? "RangeView<" + elementName + ">" : elementName;
        }

        public string BuildComparer()
        {
            return (StringComparisonOption == null)
                ? $"System.Collections.Generic.Comparer<{BuildTypeName()}>.Default"
                : "System.StringComparer." + StringComparisonOption.Split('.').Last();
        }
    }

    public class PrimaryKey : KeyBase
    {
        public override string SelectorName => "primaryIndexSelector";
    }

    public class SecondaryKey : KeyBase
    {
        public int IndexNo { get; set; }
        public override string SelectorName => $"secondaryIndex{IndexNo}Selector";
    }

    public class KeyProperty
    {
        public int KeyOrder { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }

        public override string ToString()
        {
            return $"{TypeName} {Name} : {KeyOrder}";
        }
    }
}

namespace MasterMemory.Generator
{
    public partial class DatabaseBuilderTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
    }

    public partial class MemoryDatabaseTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
    }

    public partial class ImmutableBuilderTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
    }

    public partial class TableTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public GenerationContext GenerationContext { get; set; }
    }
}