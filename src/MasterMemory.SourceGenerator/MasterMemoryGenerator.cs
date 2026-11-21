using MasterMemory.GeneratorCore;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace MasterMemory.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class MasterMemoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(MasterMemoryGeneratorOptions.EmitAttribute);

        var namespaceProvider = context.AnalyzerConfigOptionsProvider.Select((x, _) =>
            {
                x.GlobalOptions.TryGetValue("build_property.RootNamespace", out var defaultNamespace);
                return defaultNamespace;
            })
            .WithTrackingName("MasterMemory.AnalyzerConfig");

        var generatorOptions = context.CompilationProvider.Select((compilation, _) =>
            {
                foreach (var attr in compilation.Assembly.GetAttributes())
                {
                    if (attr.AttributeClass?.Name == "MasterMemoryGeneratorOptionsAttribute")
                    {
                        return MasterMemoryGeneratorOptions.FromAttribute(attr);
                    }
                }

                return default;
            })
            .WithTrackingName("MasterMemory.CompilationProvider");

        var memoryTables = context.SyntaxProvider.ForAttributeWithMetadataName("MasterMemory.MemoryTableAttribute",
            (node, token) => true,
            (ctx, token) => ctx)
            .WithTrackingName("MasterMemory.SyntaxProvider.0_ForAttributeWithMetadataName")
            .Collect()
            .Select((xs, _) =>
            {
                var list = new List<GenerationContext>();
                var reporter = new DiagnosticReporter();
                foreach (var ctx in xs)
                {
                    var memoryTableAttr = ctx.Attributes[0]; // AllowMultiple=false
                    var classDecl = ctx.TargetNode as TypeDeclarationSyntax; // class or record
                    var context = CodeGenerator.CreateGenerationContext(classDecl!, memoryTableAttr, reporter);
                    if (context != null)
                    {
                        list.Add(context);
                    }
                }
                list.Sort((a, b) => string.Compare(a.ClassName, b.ClassName, StringComparison.Ordinal));
                return (reporter, new EquatableArray<GenerationContext>(list.ToArray()));
            })
            .WithTrackingName("MasterMemory.SyntaxProvider.1_CollectAndSelect");

        var allCombined = memoryTables
            .Combine(namespaceProvider)
            .Combine(generatorOptions)
            .WithTrackingName("MasterMemory.SyntaxProvider.2_AllCombined");

        context.RegisterSourceOutput(allCombined, EmitMemoryTable);
    }

    void EmitMemoryTable(SourceProductionContext context, (((DiagnosticReporter, EquatableArray<GenerationContext>), string?), MasterMemoryGeneratorOptions) value)
    {
        var (((diagnostic, memoryTables), defaultNamespace), generatorOptions) = value;
        diagnostic.ReportToContext(context);
        if (memoryTables.Length == 0)
        {
            return;
        }

        var usingNamespace = generatorOptions.Namespace ?? defaultNamespace ?? "MasterMemory";
        var prefixClassName = generatorOptions.PrefixClassName ?? "";
        var throwIfKeyNotFound = !generatorOptions.IsReturnNullIfKeyNotFound; // becareful, reverse!

        var model = new DatabaseModel(usingNamespace, prefixClassName, memoryTables.ToArray());

        Log(AddSource(context, model.DatabaseBuilderName, DatabaseBuilderEmitter.Emit(model)));
        Log(AddSource(context, model.ImmutableBuilderName, ImmutableBuilderEmitter.Emit(model)));
        Log(AddSource(context, model.MemoryDatabaseName, MemoryDatabaseEmitter.Emit(model)));
        Log(AddSource(context, model.FormatterFactoryName, FormatterFactoryEmitter.Emit(model)));

        foreach (var table in model.Tables)
        {
            Log(AddSource(context, table.ClassName + "Table", TableEmitter.Emit(usingNamespace, table, throwIfKeyNotFound)));
        }
    }

    static void Log(string msg) => Trace.WriteLine(msg);

    static string AddSource(SourceProductionContext context, string fileName, string content)
    {
        context.AddSource($"MasterMemory.{fileName}.g.cs", content);
        return $"Generate {fileName}.";
    }
}
