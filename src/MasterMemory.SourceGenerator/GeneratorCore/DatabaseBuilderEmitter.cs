using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// Emits DatabaseBuilder: a field per table, the methods that set or change the rows of each table (Append, ReplaceAll,
/// and AddOrReplace / Remove where the primary key is unique), and ToMemoryDatabase.
/// </summary>
/// <remarks>
/// What is emitted grows with the number of tables and no faster: a method names its own table only, and the one place
/// that names them all, the call of the database constructor, is there once.
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
            writer.Line($"public {name}() : this(null) {{ }}");
            writer.Line($"public {name}(MessagePack.MessagePackFormatterFactory? formatterFactory) : base({model.FormatterFactoryName}.Instance, formatterFactory) {{ }}");

            // what MemoryDatabase.ToDatabaseBuilder calls
            writer.Line();
            writer.Line($"internal {name}({database} database, MessagePack.MessagePackFormatterFactory? formatterFactory) : this(formatterFactory)");
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
                writer.Line($"return new {database}(");
                writer.Indent();
                for (var i = 0; i < tables.Length; i++)
                {
                    var separator = (i == tables.Length - 1) ? "" : ",";
                    writer.Line($"GetTable({Entry(tables[i])}, x => new {tables[i].ClassName}Table(x)){separator}");
                }
                writer.Unindent();
                writer.Line(");");
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
}
