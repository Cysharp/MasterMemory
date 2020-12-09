using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MasterMemory.GeneratorCore
{
    public class CodeGenerator
    {
        static readonly Encoding NoBomUtf8 = new UTF8Encoding(false);

        public void GenerateFile(string usingNamespace, string inputDirectory, string outputDirectory, string prefixClassName, bool addImmutableConstructor, bool throwIfKeyNotFound, bool forceOverwrite, Action<string> logger)
        {
            prefixClassName ??= "";
            var list = new List<GenerationContext>();

            // Collect
            if (inputDirectory.EndsWith(".csproj"))
            {
                throw new InvalidOperationException("Path must be directory but it is csproj. inputDirectory:" + inputDirectory);
            }

            foreach (var item in Directory.GetFiles(inputDirectory, "*.cs", SearchOption.AllDirectories))
            {
                list.AddRange(CreateGenerationContext(item));
            }

            list.Sort((a, b) => string.Compare(a.ClassName, b.ClassName, StringComparison.Ordinal));

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

                var usingStrings = string.Join(Environment.NewLine, list.SelectMany(x => x.UsingStrings).Distinct().OrderBy(x => x, StringComparer.Ordinal));

                var builderTemplate = new DatabaseBuilderTemplate();
                var databaseTemplate = new MemoryDatabaseTemplate();
                var immutableBuilderTemplate = new ImmutableBuilderTemplate();
                var resolverTemplate = new MessagePackResolverTemplate();
                builderTemplate.Namespace = databaseTemplate.Namespace = immutableBuilderTemplate.Namespace = resolverTemplate.Namespace = usingNamespace;
                builderTemplate.PrefixClassName = databaseTemplate.PrefixClassName = immutableBuilderTemplate.PrefixClassName = resolverTemplate.PrefixClassName = prefixClassName;
                builderTemplate.Using = databaseTemplate.Using = immutableBuilderTemplate.Using = resolverTemplate.Using = (usingStrings + Environment.NewLine + ("using " + usingNamespace + ".Tables;"));
                builderTemplate.GenerationContexts = databaseTemplate.GenerationContexts = immutableBuilderTemplate.GenerationContexts = resolverTemplate.GenerationContexts = list.ToArray();

                logger(WriteToFile(outputDirectory, builderTemplate.ClassName, builderTemplate.TransformText(), forceOverwrite));
                logger(WriteToFile(outputDirectory, immutableBuilderTemplate.ClassName, immutableBuilderTemplate.TransformText(), forceOverwrite));
                logger(WriteToFile(outputDirectory, databaseTemplate.ClassName, databaseTemplate.TransformText(), forceOverwrite));
                logger(WriteToFile(outputDirectory, resolverTemplate.ClassName, resolverTemplate.TransformText(), forceOverwrite));
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
                        Using = string.Join(Environment.NewLine, context.UsingStrings),
                        ThrowKeyIfNotFound = throwIfKeyNotFound
                    };

                    logger(WriteToFile(tableDir, context.ClassName + "Table", template.TransformText(), forceOverwrite));
                }
            }
            // Modify
            {
                if (addImmutableConstructor)
                {
                    var workspace = new AdhocWorkspace();
                    var byFilePath = list.GroupBy(x => x.InputFilePath);

                    foreach (var context in byFilePath)
                    {
                        var newFile = BuildRecordConstructorFile(workspace, context.Select(x => x.OriginalClassDeclaration));
                        if (newFile != null)
                        {
                            File.WriteAllText(context.Key, newFile, NoBomUtf8);
                            logger("Modified " + context.Key);
                        }
                    }
                }
            }
        }

        static string NormalizeNewLines(string content)
        {
            // The T4 generated code may be text with mixed line ending types. (CR + CRLF)
            // We need to normalize the line ending type in each Operating Systems. (e.g. Windows=CRLF, Linux/macOS=LF)
            return content.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }

        static string WriteToFile(string directory, string fileName, string content, bool forceOverwrite)
        {
            var path = Path.Combine(directory, fileName + ".cs");
            var contentBytes = Encoding.UTF8.GetBytes(NormalizeNewLines(content));
            
            // If the generated content is unchanged, skip the write.
            if (!forceOverwrite && File.Exists(path))
            {
                if (new FileInfo(path).Length == contentBytes.Length && contentBytes.AsSpan().SequenceEqual(File.ReadAllBytes(path)))
                {
                    return $"Generate {fileName} to: {path} (Skipped)";
                }
            }

            File.WriteAllBytes(path, contentBytes);
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
                .Concat(new[] { "using MasterMemory", "using MasterMemory.Validation", "using System", "using System.Collections.Generic" })
                .Concat(ns)
                .Select(x => x.Trim(';') + ";")
                .Distinct()
                .OrderBy(x => x, StringComparer.Ordinal)
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
                    context.InputFilePath = filePath;
                    yield return context;
                }
            }
        }

        (PrimaryKey, List<SecondaryKey>, PropertyDeclarationSyntax) ExtractPropertyAttribute(PropertyDeclarationSyntax property)
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

        string BuildRecordConstructorFile(AdhocWorkspace workspace, IEnumerable<ClassDeclarationSyntax> classDeclarations)
        {
            // using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

            var modify = false;
            var editor = new SyntaxEditor(classDeclarations.First().SyntaxTree.GetRoot(), workspace);
            foreach (var classDeclaration in classDeclarations)
            {
                var members = classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
                    .Where(x => x.Modifiers.Any(y => y.IsKind(SyntaxKind.PublicKeyword)))
                    .Where(x =>
                    {
                        foreach (var attr in x.AttributeLists.SelectMany(y => y.Attributes))
                        {
                            var attrName = attr.Name.ToFullString().Trim();
                            if (attrName == "IgnoreMember" || attrName == "MessagePack.IgnoreMember")
                            {
                                return false;
                            }
                            if (attrName == "IgnoreDataMember" || attrName == "System.Runtime.Serialization.IgnoreDataMember")
                            {
                                return false;
                            }
                        }
                        return true;
                    })
                    .ToArray();

                var parameters = ParameterList(SeparatedList(members.Select(x => Parameter(attributeLists: default, modifiers: default, type: x.Type, x.Identifier, @default: null))));

                var parameterStrings = parameters.Parameters.Select(x => x.Type.ToFullStringTrim()).ToArray();

                // check existing constructor
                var matchedConstructor = classDeclaration.Members.OfType<ConstructorDeclarationSyntax>()
                    .Where(x => x.ParameterList.Parameters.Select(y => y.Type.ToFullStringTrim()).SequenceEqual(parameterStrings))
                    .ToArray();

                if (matchedConstructor.Length != 0)
                {
                    continue;
                }

                var body = members.Select(x => AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression, MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName(x.Identifier)),
                        Token(SyntaxKind.EqualsToken),
                        IdentifierName(x.Identifier)))
                    .Select(x => ExpressionStatement(x));

                var recordCtor = ConstructorDeclaration(classDeclaration.Identifier)
                    .WithModifiers(TokenList(Token(Sy‌​ntaxKind.PublicKeywo‌​rd)))
                    .WithParameterList(parameters)
                    .WithBody(Block(body))
                    .NormalizeWhitespace()
                    .WithLeadingTrivia(LineFeed)
                    .WithTrailingTrivia(LineFeed);

                var newClassDecl = classDeclaration.AddMembers(recordCtor);

                modify = true;
                editor.ReplaceNode(classDeclaration, newClassDecl);
            }

            if (modify)
            {
                var newCodeString = Microsoft.CodeAnalysis.Formatting.Formatter.Format(editor.GetChangedRoot(), workspace).ToFullString();
                return newCodeString;
            }
            else
            {
                return null;
            }
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
