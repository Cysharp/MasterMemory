using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// Emits MemoryDatabase: a property per table, loading (one table after another, or in parallel), the conversions back
/// to the builders, validation and the metadata entry points.
/// </summary>
static class MemoryDatabaseEmitter
{
    const string InitParameters = "Dictionary<string, (int offset, int count)> header, System.ReadOnlyMemory<byte> databaseBinary, MessagePack.MessagePackSerializerOptions options, int maxDegreeOfParallelism";

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

            // from tables that already exist: what ImmutableBuilder calls. It names every table, so it is internal as
            // soon as one of them is.
            var allTablesAccessibility = tables.Any(x => x.Accessibility != "public") ? "internal" : "public";
            writer.Line();
            writer.Line($"{allTablesAccessibility} {name}(");
            writer.Indent();
            for (var i = 0; i < tables.Length; i++)
            {
                var separator = (i == tables.Length - 1) ? "" : ",";
                writer.Line($"{tables[i].ClassName}Table {tables[i].ClassName}Table{separator}");
            }
            writer.Unindent();
            writer.Line(")");
            using (writer.OpenScope())
            {
                foreach (var table in tables)
                {
                    writer.Line($"this.{table.ClassName}Table = {table.ClassName}Table;");
                }
            }

            writer.Line();
            writer.Line($"public {name}(byte[] databaseBinary, bool internString = true, MessagePack.MessagePackFormatterFactory? formatterFactory = null, int maxDegreeOfParallelism = 1)");
            writer.Indent();
            writer.Line($": base(databaseBinary, internString, {model.FormatterFactoryName}.Instance, formatterFactory, maxDegreeOfParallelism)");
            writer.Unindent();
            using (writer.OpenScope())
            {
            }

            writer.Line();
            writer.Lines($$"""
                protected override void Init({{InitParameters}})
                {
                    if (maxDegreeOfParallelism == 1)
                    {
                        InitSequential(header, databaseBinary, options, maxDegreeOfParallelism);
                    }
                    else
                    {
                        InitParallel(header, databaseBinary, options, maxDegreeOfParallelism);
                    }
                }
                """);

            writer.Line();
            using (writer.Block($"void InitSequential({InitParameters})"))
            {
                foreach (var table in tables)
                {
                    writer.Line($"this.{table.ClassName}Table = {ExtractTableData(table)};");
                }
            }

            writer.Line();
            using (writer.Block($"void InitParallel({InitParameters})"))
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
            using (writer.Block($"public {model.ImmutableBuilderName} ToImmutableBuilder()"))
            {
                writer.Line($"return new {model.ImmutableBuilderName}(this);");
            }

            writer.Line();
            using (writer.Block($"public {model.DatabaseBuilderName} ToDatabaseBuilder()"))
            {
                writer.Line($"var builder = new {model.DatabaseBuilderName}();");
                EmitAppendTables(writer, tables);
            }

            writer.Line();
            using (writer.Block($"public {model.DatabaseBuilderName} ToDatabaseBuilder(MessagePack.MessagePackFormatterFactory formatterFactory)"))
            {
                writer.Line($"var builder = new {model.DatabaseBuilderName}(formatterFactory);");
                EmitAppendTables(writer, tables);
            }

            writer.Line();
            writer.Directive("#if !DISABLE_MASTERMEMORY_VALIDATOR");
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
            writer.Line();
            writer.Directive("#endif");

            writer.Line();
            writer.Line("static MasterMemory.Meta.MetaDatabase? metaTable;");
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

            writer.Line();
            writer.Directive("#if !DISABLE_MASTERMEMORY_METADATABASE");
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
            writer.Line();
            writer.Directive("#endif");
        }

        return writer.ToStringAndReturn();
    }

    static string ExtractTableData(GenerationContext table)
    {
        return $"ExtractTableData<{table.FullTypeName}, {table.ClassName}Table>(header, databaseBinary, options, xs => new {table.ClassName}Table(xs))";
    }

    static void EmitAppendTables(CodeWriter writer, GenerationContext[] tables)
    {
        foreach (var table in tables)
        {
            writer.Line($"builder.Append(this.{table.ClassName}Table.GetRawDataUnsafe());");
        }
        writer.Line("return builder;");
    }
}
