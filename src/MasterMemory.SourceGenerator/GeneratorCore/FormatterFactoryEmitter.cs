using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// Emits MasterMemoryFormatterFactory: the table types handed to TableFormatterFactory, which serves T[] of each.
/// A table array is a root type no member declares, so nothing else would construct its formatter for IL2CPP / Native AOT.
/// </summary>
static class FormatterFactoryEmitter
{
    public static string Emit(DatabaseModel model)
    {
        var writer = CodeWriter.Rent();

        using (DatabaseModel.BeginFile(writer, model.Usings, model.Namespace))
        {
            writer.Line("/// <summary>");
            writer.Line("/// The MessagePack formatters of every table array. MemoryDatabase and DatabaseBuilder compose it into the serializer themselves.");
            writer.Line("/// </summary>");
            using (writer.Block($"public static class {model.FormatterFactoryName}"))
            {
                writer.Line($"public static readonly global::MessagePack.MessagePackFormatterFactory Instance = new global::MasterMemory.TableFormatterFactory.Builder({model.Tables.Length})");
                writer.Indent();
                foreach (var table in model.Tables)
                {
                    writer.Line($".Add<{table.ClassName}>()");
                }
                writer.Line(".Build();");
                writer.Unindent();
            }
        }

        return writer.ToStringAndReturn();
    }
}
