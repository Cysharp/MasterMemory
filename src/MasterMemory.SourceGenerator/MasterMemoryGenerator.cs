using MasterMemory.GeneratorCore;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace MasterMemory.SourceGenerator;

/// <summary>
/// The pipeline: each [MemoryTable] class is read into a model, the models are checked against each other, and three
/// outputs are produced from the result: the diagnostics, one file per table, and the three database-wide files.
/// </summary>
/// <remarks>
/// Every value handed from one step to the next is compared by value and holds no symbol, syntax node or compilation.
/// That is what makes the caching work (editing one table writes that table's file again and no other table's), and
/// what keeps the compilation from being held in memory by the cache.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public partial class MasterMemoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(MasterMemoryGeneratorOptions.EmitAttribute);

        var defaultNamespace = context.AnalyzerConfigOptionsProvider.Select((x, _) =>
            {
                x.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
                return rootNamespace;
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

        var settings = generatorOptions.Combine(defaultNamespace)
            .Select((x, _) =>
            {
                var (options, rootNamespace) = x;
                var @namespace = !string.IsNullOrWhiteSpace(options.Namespace) ? options.Namespace!
                    : !string.IsNullOrWhiteSpace(rootNamespace) ? rootNamespace!
                    : "MasterMemory";
                return new GeneratorSettings(@namespace, options.PrefixClassName ?? "", ThrowIfKeyNotFound: !options.IsReturnNullIfKeyNotFound); // be careful, reversed
            })
            .WithTrackingName("MasterMemory.Settings");

        // one [MemoryTable] class -> its model, or what is wrong with it
        var tables = context.SyntaxProvider.ForAttributeWithMetadataName("MasterMemory.MemoryTableAttribute",
            static (node, _) => node is TypeDeclarationSyntax, // class or record
            static (source, cancellationToken) => TableParser.Parse(source, cancellationToken))
            .WithTrackingName("MasterMemory.SyntaxProvider.0_Table");

        // all of them together: the checks that need two tables, and one order for everything that is emitted
        var database = tables.Collect()
            .Select(static (results, _) => DatabaseParseResult.Create(results))
            .WithTrackingName("MasterMemory.SyntaxProvider.1_Database");

        var diagnostics = database
            .Select(static (x, _) => x.Diagnostics)
            .WithTrackingName("MasterMemory.SyntaxProvider.2_Diagnostics");

        var eachTable = database
            .SelectMany(static (x, _) => ImmutableArray.Create(x.Tables.ToArray()))
            .Combine(settings)
            .WithTrackingName("MasterMemory.SyntaxProvider.3_EachTable");

        var databaseEntries = database
            .Select(static (x, _) => new EquatableArray<GenerationContext>(x.Tables.ToArray().Select(t => t.ToDatabaseEntry()).ToArray()))
            .Combine(settings)
            .WithTrackingName("MasterMemory.SyntaxProvider.4_DatabaseEntries");

        context.RegisterSourceOutput(diagnostics, static (context, diagnostics) =>
        {
            foreach (var item in diagnostics)
            {
                context.ReportDiagnostic(item.ToDiagnostic());
            }
        });

        context.RegisterSourceOutput(eachTable, static (context, value) =>
        {
            var (table, settings) = value;
            AddSource(context, table.ClassName + "Table", TableEmitter.Emit(settings.Namespace, table, settings.ThrowIfKeyNotFound));
        });

        context.RegisterSourceOutput(databaseEntries, static (context, value) =>
        {
            var (tables, settings) = value;
            if (tables.Length == 0)
            {
                return;
            }

            var model = new DatabaseModel(settings.Namespace, settings.PrefixClassName, tables.ToArray());
            AddSource(context, model.DatabaseBuilderName, DatabaseBuilderEmitter.Emit(model));

            AddSource(context, model.MemoryDatabaseName, MemoryDatabaseEmitter.Emit(model));
            AddSource(context, model.FormatterFactoryName, FormatterFactoryEmitter.Emit(model));
        });
    }

    static void AddSource(SourceProductionContext context, string fileName, string content)
    {
        context.AddSource($"MasterMemory.{fileName}.g.cs", content);
    }
}

/// <summary>What the options attribute and the project say about all generated code.</summary>
readonly record struct GeneratorSettings(string Namespace, string PrefixClassName, bool ThrowIfKeyNotFound);

/// <summary>
/// The tables that are generated, in the order everything is emitted in, and every diagnostic of the run.
/// </summary>
sealed record DatabaseParseResult(EquatableArray<GenerationContext> Tables, EquatableArray<DiagnosticInfo> Diagnostics)
{
    public static DatabaseParseResult Create(ImmutableArray<TableParseResult> results)
    {
        var diagnostics = new List<DiagnosticInfo>();
        var tables = new List<(GenerationContext Table, LocationInfo? Location)>();
        foreach (var result in results)
        {
            diagnostics.AddRange(result.Diagnostics.ToArray());
            if (result.Table != null)
            {
                tables.Add((result.Table, result.Location));
            }
        }

        // The generated members are named after the simple class name (ItemTable, db.ItemTable, RemoveItem), so two
        // tables that share it, in different namespaces, cannot both be generated. Say so on each of them and leave
        // them out; adding the same file name twice would otherwise fail the whole generator.
        foreach (var duplicated in tables.GroupBy(x => x.Table.ClassName, StringComparer.Ordinal).Where(x => x.Count() > 1).ToArray())
        {
            var names = string.Join(", ", duplicated.Select(x => x.Table.FullTypeName));
            foreach (var item in duplicated)
            {
                diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.DuplicateClassName, item.Location, item.Table.ClassName, names));
                tables.Remove(item);
            }
        }

        // The table name is the key of a table in the database binary, and a case label in generated code.
        foreach (var duplicated in tables.GroupBy(x => x.Table.MemoryTableName, StringComparer.Ordinal).Where(x => x.Count() > 1).ToArray())
        {
            var names = string.Join(", ", duplicated.Select(x => x.Table.FullTypeName));
            foreach (var item in duplicated)
            {
                diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.DuplicateTableName, item.Location, item.Table.MemoryTableName, names));
                tables.Remove(item);
            }
        }

        tables.Sort((a, b) => string.Compare(a.Table.ClassName, b.Table.ClassName, StringComparison.Ordinal));
        return new DatabaseParseResult(tables.Select(x => x.Table).ToArray(), diagnostics.ToArray());
    }
}
