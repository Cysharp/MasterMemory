using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// Emits ImmutableBuilder: per table ReplaceAll, and Remove / Diff where the primary key is unique. Each of them builds
/// a new MemoryDatabase that shares every other table with the current one.
/// </summary>
static class ImmutableBuilderEmitter
{
    public static string Emit(DatabaseModel model)
    {
        var writer = CodeWriter.Rent();
        var name = model.ImmutableBuilderName;
        var database = model.MemoryDatabaseName;

        using (DatabaseModel.BeginFile(writer, model.Usings, model.Namespace))
        using (writer.Block($"public sealed class {name} : ImmutableBuilderBase"))
        {
            writer.Line($"{database} memory;");
            writer.Line();
            using (writer.Block($"public {name}({database} memory)"))
            {
                writer.Line("this.memory = memory;");
            }
            writer.Line();
            using (writer.Block($"public {database} Build()"))
            {
                writer.Line("return memory;");
            }

            for (var i = 0; i < model.Tables.Length; i++)
            {
                var table = model.Tables[i];
                var key = table.PrimaryKey;
                var keyAccessor = key.BuildKeyAccessor("x");
                var comparer = key.BuildComparer();

                writer.Line();
                using (writer.Block($"public void ReplaceAll(System.Collections.Generic.IList<{table.ClassName}> data)"))
                {
                    writer.Line($"var newData = CloneAndSortBy(data, x => {keyAccessor}, {comparer});");
                    EmitReplaceTable(writer, model, i);
                }

                if (key.IsNonUnique)
                {
                    continue; // a row cannot be addressed by a key that several rows share
                }

                writer.Line();
                using (writer.Block($"public void Remove{table.ClassName}({key.BuildTypeName()}[] keys)"))
                {
                    writer.Line($"var data = RemoveCore(memory.{table.ClassName}Table.GetRawDataUnsafe(), keys, x => {keyAccessor}, {comparer});");
                    writer.Line($"var newData = CloneAndSortBy(data, x => {keyAccessor}, {comparer});");
                    EmitReplaceTable(writer, model, i);
                }

                writer.Line();
                using (writer.Block($"public void Diff({table.ClassName}[] addOrReplaceData)"))
                {
                    writer.Line($"var data = DiffCore(memory.{table.ClassName}Table.GetRawDataUnsafe(), addOrReplaceData, x => {keyAccessor}, {comparer});");
                    writer.Line($"var newData = CloneAndSortBy(data, x => {keyAccessor}, {comparer});");
                    EmitReplaceTable(writer, model, i);
                }
            }
        }

        return writer.ToStringAndReturn();
    }

    // newData becomes the table at tableIndex; the database constructor takes every table, in declaration order
    static void EmitReplaceTable(CodeWriter writer, DatabaseModel model, int tableIndex)
    {
        writer.Line($"var table = new {model.Tables[tableIndex].ClassName}Table(newData);");
        writer.Line($"memory = new {model.MemoryDatabaseName}(");
        writer.Indent();
        for (var i = 0; i < model.Tables.Length; i++)
        {
            var argument = (i == tableIndex) ? "table" : "memory." + model.Tables[i].ClassName + "Table";
            var separator = (i == model.Tables.Length - 1) ? "" : ",";
            writer.Line(argument + separator);
        }
        writer.Unindent();
        writer.Line(");");
    }
}
