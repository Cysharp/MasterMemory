using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>Emits DatabaseBuilder: one Append overload per table, which sorts the rows by the primary key.</summary>
static class DatabaseBuilderEmitter
{
    public static string Emit(DatabaseModel model)
    {
        var writer = CodeWriter.Rent();
        var name = model.DatabaseBuilderName;

        using (DatabaseModel.BeginFile(writer, model.Usings, model.Namespace))
        using (writer.Block($"public sealed class {name} : DatabaseBuilderBase"))
        {
            writer.Line($"public {name}() : this(null) {{ }}");
            writer.Line($"public {name}(MessagePack.MessagePackFormatterFactory? formatterFactory) : base({model.FormatterFactoryName}.Instance, formatterFactory) {{ }}");

            foreach (var table in model.Tables)
            {
                writer.Line();
                using (writer.Block($"public {name} Append(System.Collections.Generic.IEnumerable<{table.FullTypeName}> dataSource)"))
                {
                    writer.Line($"AppendCore(dataSource, x => {table.PrimaryKey.BuildKeyAccessor("x")}, {table.PrimaryKey.BuildComparer()});");
                    writer.Line("return this;");
                }
            }
        }

        return writer.ToStringAndReturn();
    }
}
