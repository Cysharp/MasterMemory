using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// Emits DatabaseBuilder: a field per table, the methods that set or change the rows of each table (Append, ReplaceAll,
/// and AddOrReplace / Remove where the primary key is unique), and ToMemoryDatabase.
/// </summary>
/// <remarks>
/// What is emitted grows with the number of tables and no faster: a method names its own table only. The database is
/// made by handing it the builder, and asks for each table through a method of that table (BuildTable).
/// </remarks>
static class DatabaseBuilderEmitter
{
    public static string Emit(DatabaseModel model)
    {
        var writer = CodeWriter.Rent();
        var name = model.DatabaseBuilderName;
        var database = model.MemoryDatabaseName;
        var tables = model.Tables;

        using (DatabaseModel.BeginFile(writer, model.Usings, model.Namespace))
        using (writer.Block($"public sealed class {name} : DatabaseBuilderBase"))
        {
            foreach (var table in tables)
            {
                writer.Line($"TableEntry<{table.FullTypeName}, {table.ClassName}Table>? {Entry(table)};");
            }

            writer.Line();
            // The options go through the generated registration class on their way to the base class, as in the
            // database: the formatter of each table array is registered before a table is written.
            var options = $"{model.TableFormattersName}.Registered(options)";

            writer.Line("/// <summary>A builder that writes with MessagePack's source-generated formatters and without compression.</summary>");
            writer.Line($"public {name}() : this(null) {{ }}");
            writer.Line();
            writer.Lines("""
                /// <summary>A builder that writes as the options say.</summary>
                /// <param name="options">
                /// How the tables are written: the formatters of the rows, and the processor if the tables are to be compressed
                /// (new MemoryDatabaseOptions { MessageProcessor = new Lz4BlockProcessor() }, with the MessagePack.LZ4 package).
                /// Null is MemoryDatabaseOptions.Default. Give the same options to the MemoryDatabase that reads the binary.
                /// </param>
                """);
            writer.Line($"public {name}(MasterMemory.MemoryDatabaseOptions? options) : base({options}) {{ }}");

            // what MemoryDatabase.ToDatabaseBuilder calls
            writer.Line();
            writer.Line($"internal {name}({database} database, MasterMemory.MemoryDatabaseOptions? options) : base({options})");
            using (writer.OpenScope())
            {
                foreach (var table in tables)
                {
                    writer.Line($"SetTable(ref {Entry(table)}, {table.MemoryTableNameLiteral}, database.{table.ClassName}Table);");
                }
            }

            writer.Line();
            writer.Line("/// <summary>");
            writer.Line("/// The tables as a database, without writing a database binary and reading it back. The rows are used as they");
            writer.Line("/// are, and a table that was not changed since the builder was made from a database is the table of that database.");
            writer.Line("/// </summary>");
            using (writer.Block($"public {database} ToMemoryDatabase()"))
            {
                writer.Line($"return new {database}(this);");
            }

            // what the constructor of the database that ToMemoryDatabase calls asks for, table by table
            writer.Line();
            foreach (var table in tables)
            {
                writer.Line($"internal {table.ClassName}Table {BuildTable(table)}() => GetTable({Entry(table)}, x => new {table.ClassName}Table(x));");
            }

            // What AppendDynamic calls with a row type known only at run time. Each branch names its type, so the
            // Append it calls is bound at compile time: no reflection, which trimming and Native AOT could not follow.
            writer.Line();
            using (writer.Block("protected override bool AppendDynamicCore(Type dataType, System.Collections.Generic.IList<object> tableData)"))
            {
                foreach (var table in tables)
                {
                    writer.Line($"if (dataType == typeof({table.FullTypeName})) {{ Append(CastRows<{table.FullTypeName}>(tableData)); return true; }}");
                }
                writer.Line("return false;");
            }

            foreach (var table in tables)
            {
                var key = table.PrimaryKey;
                var element = table.FullTypeName;
                var entry = Entry(table);
                var keyOf = "KeyOf" + table.ClassName;
                var comparer = key.BuildComparer(table.ClassName + "Table.");
                var tableName = table.MemoryTableNameLiteral;

                writer.Line();
                writer.Line($"static {key.BuildTypeName()} {keyOf}({element} x) => {key.BuildKeyAccessor("x")};");

                writer.Line();
                writer.Line("/// <summary>The rows of a table the builder does not have yet. A table that is there is changed by ReplaceAll.</summary>");
                using (writer.Block($"{table.Accessibility} {name} Append(System.Collections.Generic.IEnumerable<{element}> dataSource)"))
                {
                    writer.Line($"AppendCore(ref {entry}, {tableName}, dataSource, {keyOf}, {comparer});");
                    writer.Line("return this;");
                }

                writer.Line();
                writer.Line("/// <summary>The rows of the table, in place of the ones it has.</summary>");
                using (writer.Block($"{table.Accessibility} {name} ReplaceAll(System.Collections.Generic.IEnumerable<{element}> dataSource)"))
                {
                    writer.Line($"ReplaceAllCore(ref {entry}, {tableName}, dataSource, {keyOf}, {comparer});");
                    writer.Line("return this;");
                }

                if (key.IsNonUnique)
                {
                    continue; // a row cannot be addressed by a key that several rows share
                }

                writer.Line();
                writer.Line("/// <summary>Each row takes the place of the row that has its primary key, or is added when there is none.</summary>");
                using (writer.Block($"{table.Accessibility} {name} AddOrReplace(System.Collections.Generic.IEnumerable<{element}> dataSource)"))
                {
                    writer.Line($"AddOrReplaceCore(ref {entry}, {tableName}, dataSource, {keyOf}, {comparer});");
                    writer.Line("return this;");
                }

                writer.Line();
                writer.Line("/// <summary>Takes out the rows that have one of the primary keys.</summary>");
                using (writer.Block($"{table.Accessibility} {name} Remove{table.ClassName}(params {key.BuildTypeName()}[] keys)"))
                {
                    writer.Line($"RemoveCore(ref {entry}, keys, {keyOf}, {comparer});");
                    writer.Line("return this;");
                }
            }
        }

        return writer.ToStringAndReturn();
    }

    // no member of the builder, generated or inherited, starts with a lower-case letter
    static string Entry(GenerationContext table)
    {
        return "entry" + table.ClassName;
    }

    /// <summary>
    /// The internal method of the builder that gives the table of one class for a database. Class names are unique, and
    /// the other members named after a class are Remove***, so the name is no other member's.
    /// </summary>
    public static string BuildTable(GenerationContext table)
    {
        return "Build" + table.ClassName + "Table";
    }
}
