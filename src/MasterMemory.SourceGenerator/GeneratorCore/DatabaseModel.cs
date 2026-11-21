using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// What the database-wide files are emitted from: the builder, the database, the immutable builder and the formatter factory.
/// </summary>
sealed class DatabaseModel
{
    public string Namespace { get; }
    public string PrefixClassName { get; }
    public GenerationContext[] Tables { get; }

    /// <summary>The using directives of every table's source file, plus the generated Tables namespace.</summary>
    public string[] Usings { get; }

    public string DatabaseBuilderName => PrefixClassName + "DatabaseBuilder";
    public string MemoryDatabaseName => PrefixClassName + "MemoryDatabase";
    public string ImmutableBuilderName => PrefixClassName + "ImmutableBuilder";
    public string FormatterFactoryName => PrefixClassName + "MasterMemoryFormatterFactory";

    public DatabaseModel(string @namespace, string prefixClassName, GenerationContext[] tables)
    {
        Namespace = @namespace;
        PrefixClassName = prefixClassName;
        Tables = tables;
        Usings = tables.SelectMany(x => x.UsingStrings)
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal)
            .Append("using " + @namespace + ".Tables;")
            .ToArray();
    }

    /// <summary>The head of a generated file up to the opened namespace, which the returned scope closes.</summary>
    public static CodeWriter.BlockScope BeginFile(CodeWriter writer, IEnumerable<string> usings, string @namespace)
    {
        writer.FileHeader();
        writer.Line();
        foreach (var item in usings)
        {
            writer.Lines(item); // a directive carries the comment lines that preceded it in the source
        }
        writer.Line();
        return writer.Block("namespace " + @namespace);
    }
}
