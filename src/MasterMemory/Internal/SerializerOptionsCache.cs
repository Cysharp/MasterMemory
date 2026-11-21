using MessagePack;
using System.Runtime.CompilerServices;

namespace MasterMemory.Internal
{
    // A MessagePackFormatterResolver owns the formatters it has created, so a resolver per database would build every
    // table's formatter again on each load. The options are therefore kept per (table factory, user factory) pair;
    // both keys are held weakly, so a factory the application drops takes its formatters with it.
    internal static class SerializerOptionsCache
    {
        // Each table is one Lz4Block message. A table is the application's own data and is read from a byte[] the
        // caller already holds, so it is not held to the 64 MB an untrusted message gets by default.
        public static readonly Lz4BlockProcessor TableProcessor = new Lz4BlockProcessor(int.MaxValue);

        static readonly ConditionalWeakTable<MessagePackFormatterFactory, ConditionalWeakTable<MessagePackFormatterFactory, Entry>> cache
            = new ConditionalWeakTable<MessagePackFormatterFactory, ConditionalWeakTable<MessagePackFormatterFactory, Entry>>();

        public static MessagePackSerializerOptions Get(MessagePackFormatterFactory tableFormatterFactory, MessagePackFormatterFactory? formatterFactory, bool internString)
        {
            // DefaultAot serves source-generated and built-in formatters only, so a database behaves the same on the JIT
            // as it does under IL2CPP or Native AOT
            var userFactory = formatterFactory ?? MessagePackFormatterFactory.DefaultAot;

            var entry = cache
                .GetValue(tableFormatterFactory, static _ => new ConditionalWeakTable<MessagePackFormatterFactory, Entry>())
                .GetValue(userFactory, static _ => new Entry());

            return entry.GetOptions(tableFormatterFactory, userFactory, internString);
        }

        sealed class Entry
        {
            MessagePackSerializerOptions? options;
            MessagePackSerializerOptions? internStringOptions;

            public MessagePackSerializerOptions GetOptions(MessagePackFormatterFactory tableFormatterFactory, MessagePackFormatterFactory userFactory, bool internString)
            {
                lock (this)
                {
                    if (internString)
                    {
                        return internStringOptions ??= Create(MessagePackFormatterFactory.Combine(InternStringFormatterFactory.Instance, tableFormatterFactory, userFactory));
                    }
                    else
                    {
                        return options ??= Create(MessagePackFormatterFactory.Combine(tableFormatterFactory, userFactory));
                    }
                }
            }

            static MessagePackSerializerOptions Create(MessagePackFormatterFactory factory)
            {
                // master data is not attacker-controlled input, so the dictionaries and sets inside a row keep the default comparer
                var resolver = new MessagePackFormatterResolver(factory) { HashFloodingResistant = false };
                return new MessagePackSerializerOptions(resolver) { MessageProcessor = TableProcessor };
            }
        }
    }
}
