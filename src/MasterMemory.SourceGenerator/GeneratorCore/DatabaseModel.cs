using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// What the database-wide files are emitted from: the builder, the database and the formatter factory.
/// </summary>
sealed class DatabaseModel
{
    public string Namespace { get; }
    public string PrefixClassName { get; }
    public GenerationContext[] Tables { get; }

    /// <summary>
    /// What every generated file imports. Table and key types are written in full, so nothing is taken from the files
    /// the tables are declared in.
    /// </summary>
    public static readonly string[] CommonUsings =
    [
        "using MasterMemory;",
        "using MasterMemory.Validation;",
        "using System;",
        "using System.Collections.Generic;",
    ];

    /// <summary><see cref="CommonUsings"/> plus the namespace of the generated table classes.</summary>
    public string[] Usings { get; }

    public string DatabaseBuilderName => PrefixClassName + "DatabaseBuilder";
    public string MemoryDatabaseName => PrefixClassName + "MemoryDatabase";

    public string TableFormattersName => PrefixClassName + "MasterMemoryTableFormatters";

    /// <summary>Whether GetMetaDatabase() is emitted, see the option of the same name.</summary>
    public bool GenerateMetaDatabase { get; }

    /// <summary>Whether Validate() is emitted, see the option of the same name.</summary>
    public bool GenerateValidator { get; }

    public DatabaseModel(string @namespace, string prefixClassName, GenerationContext[] tables, bool generateMetaDatabase, bool generateValidator)
    {
        Namespace = @namespace;
        PrefixClassName = prefixClassName;
        Tables = tables;
        GenerateMetaDatabase = generateMetaDatabase;
        GenerateValidator = generateValidator;
        Usings = [.. CommonUsings, "using " + @namespace + ".Tables;"];
    }

    /// <summary>The head of a generated file up to the opened namespace, which the returned scope closes.</summary>
    public static CodeWriter.BlockScope BeginFile(CodeWriter writer, IEnumerable<string> usings, string @namespace)
    {
        writer.FileHeader();
        writer.Line();
        foreach (var item in usings)
        {
            writer.Line(item);
        }
        writer.Line();
        return writer.Block("namespace " + @namespace);
    }
}
