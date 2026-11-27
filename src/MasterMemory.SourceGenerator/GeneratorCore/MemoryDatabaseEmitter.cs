using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// Emits MemoryDatabase: a property per table, loading (one table after another, or in parallel), the conversion back
/// to the builder, validation and the metadata entry points.
/// </summary>
static class MemoryDatabaseEmitter
{
    // The tables are read one after another from a span, which can be over any memory. Reading several at the same
    // time hands the binary to other threads, and that takes a ReadOnlyMemory.
    const string SequentialParameters = "Dictionary<string, (int offset, int count)> header, System.ReadOnlySpan<byte> databaseBinary, MessagePack.MessagePackSerializerOptions options";
    const string ParallelParameters = "Dictionary<string, (int offset, int count)> header, System.ReadOnlyMemory<byte> databaseBinary, MessagePack.MessagePackSerializerOptions options, int maxDegreeOfParallelism";
    const string Options = "MasterMemory.MemoryDatabaseOptions? options";

    public static string Emit(DatabaseModel model)
    {
        var writer = CodeWriter.Rent();
        var name = model.MemoryDatabaseName;
        var tables = model.Tables;

        using (DatabaseModel.BeginFile(writer, model.Usings, model.Namespace))
        using (writer.Block($"public sealed partial class {name} : MemoryDatabaseBase"))
        {
            foreach (var table in tables)
            {
                writer.Line($"{table.Accessibility} {table.ClassName}Table {table.ClassName}Table {{ get; private set; }} = default!;");
            }

            // From the tables of a builder: what DatabaseBuilder.ToMemoryDatabase calls. It takes the builder and asks
            // it for each table, so that no constructor has a parameter per table. The options of the builder are the
            // options of the database.
            writer.Line();
            writer.Line($"internal {name}({model.DatabaseBuilderName} builder)");
            writer.Indent();
            writer.Line(": base(builder.Options)");
            writer.Unindent();
            using (writer.OpenScope())
            {
                foreach (var table in tables)
                {
                    writer.Line($"this.{table.ClassName}Table = builder.{DatabaseBuilderEmitter.BuildTable(table)}();");
                }
            }

            // Every constructor that reads a binary passes its options through the generated registration class, which
            // registers the formatter of each table array before the base class reads a table (TableFormattersEmitter).
            //
            // How the tables are read is in one object, MemoryDatabaseOptions: the formatters, the processor the tables
            // were compressed with, whether strings are interned. The same object is given to the builder. With none
            // given the tables are read with MessagePack's source-generated formatters, as they are (no compression),
            // and the strings are interned within the database.
            var options = $"{model.TableFormattersName}.Registered(options)";

            writer.Line();
            writer.Lines("""
                /// <summary>The database from its binary.</summary>
                /// <param name="databaseBinary">What DatabaseBuilder.Build returned.</param>
                /// <param name="options">
                /// How the tables are read: the formatters of the rows, the processor the tables were compressed with, whether
                /// strings are interned. The default is MemoryDatabaseOptions.Default: MessagePack's source-generated formatters,
                /// no compression, strings interned within the database. A database built by MasterMemory v3 is compressed with
                /// LZ4 and takes new MemoryDatabaseOptions { MessageProcessor = new Lz4BlockProcessor() }, with the processor of
                /// the MessagePack.LZ4 package. Make the options once and give the same ones to DatabaseBuilder.
                /// </param>
                /// <param name="maxDegreeOfParallelism">How many tables are deserialized at the same time.</param>
                """);
            writer.Line($"public {name}(byte[] databaseBinary, {Options} = null, int maxDegreeOfParallelism = 1)");
            writer.Indent();
            writer.Line($": base(databaseBinary, {options}, maxDegreeOfParallelism)");
            writer.Unindent();
            using (writer.OpenScope())
            {
            }

            writer.Line();
            writer.Line("/// <summary>");
            writer.Line("/// The database from memory that need not be an array of its own: a part of a larger buffer, a rented array,");
            writer.Line("/// native memory. The memory is read until the constructor returns and is not kept.");
            writer.Line("/// </summary>");
            writer.Line($"/// <inheritdoc cref=\"{name}(byte[], MasterMemory.MemoryDatabaseOptions, int)\" path=\"/param\"/>");
            writer.Line($"public {name}(System.ReadOnlyMemory<byte> databaseBinary, {Options} = null, int maxDegreeOfParallelism = 1)");
            writer.Indent();
            writer.Line($": base(databaseBinary, {options}, maxDegreeOfParallelism)");
            writer.Unindent();
            using (writer.OpenScope())
            {
            }

            writer.Line();
            writer.Line("/// <summary>");
            writer.Line("/// The database from a span, which can be over any memory. The tables are read one after another: reading");
            writer.Line("/// them at the same time (maxDegreeOfParallelism) takes a byte[] or a ReadOnlyMemory. The span is not kept.");
            writer.Line("/// </summary>");
            writer.Line($"/// <inheritdoc cref=\"{name}(byte[], MasterMemory.MemoryDatabaseOptions, int)\" path=\"/param\"/>");
            writer.Line($"public {name}(System.ReadOnlySpan<byte> databaseBinary, {Options} = null)");
            writer.Indent();
            writer.Line($": base(databaseBinary, {options})");
            writer.Unindent();
            using (writer.OpenScope())
            {
            }
            writer.Line();
            writer.Lines($$"""
                protected override void Init({{ParallelParameters}})
                {
                    if (maxDegreeOfParallelism == 1)
                    {
                        InitSequential(header, databaseBinary.Span, options);
                    }
                    else
                    {
                        InitParallel(header, databaseBinary, options, maxDegreeOfParallelism);
                    }
                }

                protected override void Init({{SequentialParameters}})
                {
                    InitSequential(header, databaseBinary, options);
                }
                """);

            writer.Line();
            using (writer.Block($"void InitSequential({SequentialParameters})"))
            {
                foreach (var table in tables)
                {
                    writer.Line($"this.{table.ClassName}Table = {ExtractTableData(table)};");
                }
            }

            writer.Line();
            using (writer.Block($"void InitParallel({ParallelParameters})"))
            {
                writer.Line("var extracts = new Action[]");
                using (writer.OpenScope(";"))
                {
                    foreach (var table in tables)
                    {
                        writer.Line($"() => this.{table.ClassName}Table = {ExtractTableData(table)},");
                    }
                }
                writer.Line();
                writer.Lines("""
                    System.Threading.Tasks.Parallel.Invoke(new System.Threading.Tasks.ParallelOptions
                    {
                        MaxDegreeOfParallelism = maxDegreeOfParallelism
                    }, extracts);
                    """);
            }

            writer.Line();
            writer.Line("/// <summary>");
            writer.Line("/// A builder that has every table of this database, to change some of them and make another database");
            writer.Line("/// (ToMemoryDatabase) or a database binary (Build). This database stays as it is.");
            writer.Line("/// </summary>");
            writer.Line("/// <param name=\"options\">");
            writer.Line("/// What the builder writes with. The default is the options of this database: a database that was read with a");
            writer.Line("/// processor is written with it again.");
            writer.Line("/// </param>");
            using (writer.Block($"public {model.DatabaseBuilderName} ToDatabaseBuilder({Options} = null)"))
            {
                writer.Line($"return new {model.DatabaseBuilderName}(this, options ?? Options);");
            }

            // GenerateValidator = false leaves this out, and the ITableUniqueValidate of every table with it
            if (model.GenerateValidator)
            {
                writer.Line();
                using (writer.Block("public ValidateResult Validate()"))
                {
                    writer.Line("var result = new ValidateResult();");
                    writer.Line("var database = new ValidationDatabase(new object[]");
                    using (writer.OpenScope(");"))
                    {
                        foreach (var table in tables)
                        {
                            writer.Line($"{table.ClassName}Table,");
                        }
                    }
                    writer.Line();
                    foreach (var table in tables)
                    {
                        writer.Line($"((ITableUniqueValidate){table.ClassName}Table).ValidateUnique(result);");
                        writer.Line($"ValidateTable({table.ClassName}Table.All, database, \"{table.PrimaryKey.BuildPropertyTupleName()}\", {table.ClassName}Table.PrimaryKeySelector, result);");
                    }
                    writer.Line();
                    writer.Line("return result;");
                }
            }

            writer.Line();
            using (writer.Block($"public static object? GetTable({name} db, string tableName)"))
            using (writer.Block("switch (tableName)"))
            {
                foreach (var table in tables)
                {
                    writer.Line($"case {table.MemoryTableNameLiteral}:");
                    writer.Indent().Line($"return db.{table.ClassName}Table;").Unindent();
                }
                writer.Line("default:");
                writer.Indent().Line("return null;").Unindent();
            }

            // GenerateMetaDatabase = false leaves this out, and the CreateMetaTable of every table with it
            if (model.GenerateMetaDatabase)
            {
                writer.Line();
                writer.Line("static MasterMemory.Meta.MetaDatabase? metaTable;");
                writer.Line();
                using (writer.Block("public static MasterMemory.Meta.MetaDatabase GetMetaDatabase()"))
                {
                    writer.Line("if (metaTable != null) return metaTable;");
                    writer.Line();
                    writer.Line("var dict = new Dictionary<string, MasterMemory.Meta.MetaTable>();");
                    foreach (var table in tables)
                    {
                        writer.Line($"dict.Add({table.MemoryTableNameLiteral}, {model.Namespace}.Tables.{table.ClassName}Table.CreateMetaTable());");
                    }
                    writer.Line();
                    writer.Line("metaTable = new MasterMemory.Meta.MetaDatabase(dict);");
                    writer.Line("return metaTable;");
                }
            }
        }

        return writer.ToStringAndReturn();
    }

    static string ExtractTableData(GenerationContext table)
    {
        return $"ExtractTableData<{table.FullTypeName}, {table.ClassName}Table>(header, databaseBinary, options, {table.MemoryTableNameLiteral}, xs => new {table.ClassName}Table(xs))";
    }
}
