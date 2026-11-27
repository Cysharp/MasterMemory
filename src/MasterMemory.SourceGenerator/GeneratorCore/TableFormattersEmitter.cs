using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// Emits MasterMemoryTableFormatters: the class that puts the formatter of every table array into MessagePack's
/// registry of generated formatters.
/// </summary>
/// <remarks>
/// A table is stored as T[]. That array is a root type no member declares, so MessagePack's source generator does not
/// make a formatter for it, and its chain for Native AOT and IL2CPP (DefaultAot) does not find one. The generator
/// cannot be told from here either: one generator does not see what another emits. What it does offer is the registry
/// its own generated code writes to, SourceGeneratedFormatterFactory, which every chain MessagePack builds consults
/// first. With the arrays registered there, they are found through whatever factory the application gives (a chain
/// around Default or DefaultAot, as every chain is that serves the built-in types), and MasterMemory needs no factory
/// of its own for them in the chain.
///
/// They are registered with RegisterHarvested, which leaves an entry that is already there alone: a formatter the
/// application declared for the array of a table keeps its place.
///
/// The registration runs in the static constructor of this class, which the constructors of the generated database
/// and builder go through (Registered). A module initializer would do it sooner, for an application that serializes a
/// table array before it has made either; it needs an attribute that the older target frameworks do not have, and
/// whether it runs at all is the platform's to say. Everything MasterMemory itself reads or writes goes through those
/// two classes.
/// </remarks>
static class TableFormattersEmitter
{
    public static string Emit(DatabaseModel model)
    {
        var writer = CodeWriter.Rent();
        var name = model.TableFormattersName;

        using (DatabaseModel.BeginFile(writer, model.Usings, model.Namespace))
        {
            writer.Line("/// <summary>");
            writer.Line("/// Registers the MessagePack formatter of every table array, so that a table is read and written through any");
            writer.Line("/// formatter factory: the registry is what MessagePack's own chains (Default, DefaultAot) look in first.");
            writer.Line("/// </summary>");
            using (writer.Block($"internal static class {name}"))
            {
                writer.Line("// A static constructor, not field initializers: it runs before the first call of Registered, and once.");
                using (writer.Block($"static {name}()"))
                {
                    writer.Line("var registry = global::MessagePack.SourceGeneratedFormatterFactory.Instance;");
                    foreach (var table in model.Tables)
                    {
                        writer.Line($"registry.RegisterHarvested<{table.FullTypeName}[]>(new global::MessagePack.Formatters.ArrayFormatterFactory<{table.FullTypeName}>());");
                    }
                }

                writer.Line();
                writer.Lines("""
                    /// <summary>
                    /// The options as they are, handed back once the table arrays are registered: what the constructors of the
                    /// database and the builder pass their options through, on the way to the base class that uses them.
                    /// </summary>
                    internal static global::MasterMemory.MemoryDatabaseOptions? Registered(global::MasterMemory.MemoryDatabaseOptions? options)
                    {
                        return options;
                    }
                    """);
            }
        }

        return writer.ToStringAndReturn();
    }
}
