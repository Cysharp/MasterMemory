using MasterMemory.GeneratorCore;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MasterMemory.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class MasterMemoryGenerator : IIncrementalGenerator
{
    //[Option("i", "Input file directory(search recursive).")]
    //string inputDirectory,

    //   [Option("o", "Output file directory.")]string outputDirectory,

    //   [Option("n", "Namespace of generated files.")]string usingNamespace,

    //   [Option("p", "Prefix of class names.")]string prefixClassName = "",

    //   [Option("c", "Add immutable constructor to MemoryTable class.")]bool addImmutableConstructor = false,

    //   [Option("t", "Return null if key not found on unique find method.")]bool returnNullIfKeyNotFound = false,

    //   [Option("f", "Overwrite generated files if the content is unchanged.")]bool forceOverwrite = false)

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // TODO:...
        // Input and Output is no needed.
        // prefix should be configurable?(to assemblyattribute)
        // remove immutableconstructor feature
        // returnnull

        var memoryTables = context.SyntaxProvider.ForAttributeWithMetadataName("MasterMemory.MemoryTableAttribute",
            (node, token) => true,
            (ctx, token) => ctx)
            .Collect();

        context.RegisterSourceOutput(memoryTables, EmitMemoryTable);
    }

    void EmitMemoryTable(SourceProductionContext context, ImmutableArray<GeneratorAttributeSyntaxContext> memoryTables)
    {
        var usingNamespace = "FooBarBaz"; // TODO:from option?
        var prefixClassName = ""; // TODO
        var throwIfKeyNotFound = false;

        var list = memoryTables.Select(x =>
            {
                // TODO: RecordDeclaration
                var classDecl = x.TargetNode as ClassDeclarationSyntax;
                return CodeGenerator.CreateGenerationContext(classDecl!);
            })
            .ToList();

        list.Sort((a, b) => string.Compare(a.ClassName, b.ClassName, StringComparison.Ordinal));

        var usingStrings = string.Join(Environment.NewLine, list.SelectMany(x => x.UsingStrings).Distinct().OrderBy(x => x, StringComparer.Ordinal));

        var builderTemplate = new DatabaseBuilderTemplate();
        var databaseTemplate = new MemoryDatabaseTemplate();
        var immutableBuilderTemplate = new ImmutableBuilderTemplate();
        var resolverTemplate = new MessagePackResolverTemplate();
        builderTemplate.Namespace = databaseTemplate.Namespace = immutableBuilderTemplate.Namespace = resolverTemplate.Namespace = usingNamespace;
        builderTemplate.PrefixClassName = databaseTemplate.PrefixClassName = immutableBuilderTemplate.PrefixClassName = resolverTemplate.PrefixClassName = prefixClassName;
        builderTemplate.Using = databaseTemplate.Using = immutableBuilderTemplate.Using = resolverTemplate.Using = (usingStrings + Environment.NewLine + ("using " + usingNamespace + ".Tables;"));
        builderTemplate.GenerationContexts = databaseTemplate.GenerationContexts = immutableBuilderTemplate.GenerationContexts = resolverTemplate.GenerationContexts = list.ToArray();

        Log(AddSource(context, builderTemplate.ClassName, builderTemplate.TransformText()));
        Log(AddSource(context, immutableBuilderTemplate.ClassName, immutableBuilderTemplate.TransformText()));
        Log(AddSource(context, databaseTemplate.ClassName, databaseTemplate.TransformText()));
        Log(AddSource(context, resolverTemplate.ClassName, resolverTemplate.TransformText()));

        foreach (var generationContext in list)
        {
            var template = new TableTemplate()
            {
                Namespace = usingNamespace,
                GenerationContext = generationContext,
                Using = string.Join(Environment.NewLine, generationContext.UsingStrings),
                ThrowKeyIfNotFound = throwIfKeyNotFound
            };

            Log(AddSource(context, generationContext.ClassName + "Table", template.TransformText()));
        }
    }

    static void Log(string msg) => Trace.WriteLine(msg);

    static string AddSource(SourceProductionContext context, string fileName, string content)
    {
        var contentString = NormalizeNewLines(content);
        context.AddSource($"MasterMemory.{fileName}.g.cs", contentString);
        return $"Generate {fileName}.";

        static string NormalizeNewLines(string content)
        {
            // The T4 generated code may be text with mixed line ending types. (CR + CRLF)
            // We need to normalize the line ending type in each Operating Systems. (e.g. Windows=CRLF, Linux/macOS=LF)
            return content.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }
    }
}
