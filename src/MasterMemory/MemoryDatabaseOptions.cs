using MessagePack;
using System;
using System.Threading;

namespace MasterMemory
{
    /// <summary>
    /// How the tables of a database are read and written: the formatters of the rows, the processor the tables are
    /// compressed with, whether strings are interned. One instance is given to the database and to the builder alike,
    /// and is meant to be made once and kept: it holds what MessagePack has made for the formatters, so that a second
    /// builder does not make it again, nor a second database that does not intern or that shares its pool.
    /// </summary>
    /// <remarks>
    /// <code>
    /// var options = new MemoryDatabaseOptions { MessageProcessor = new Lz4BlockProcessor() };
    /// var binary = new DatabaseBuilder(options).Append(rows).Build();
    /// var database = new MemoryDatabase(binary, options);
    /// </code>
    /// A changed copy is made with <c>with</c>: <c>options with { InternString = false }</c>.
    /// </remarks>
    public sealed record MemoryDatabaseOptions
    {
        /// <summary>
        /// What a database and a builder use when they are given no options: MessagePack's source-generated and
        /// built-in formatters, no compression, and the strings interned within each database.
        /// </summary>
        public static MemoryDatabaseOptions Default { get; } = new MemoryDatabaseOptions();

        // The formatters of the default factory are the same for every options that have no factory of their own.
        // Master data is not attacker-controlled input, so the dictionaries and sets inside a row keep the default comparer.
        static readonly MessagePackFormatterResolver defaultResolver = CreateResolver(MessagePackFormatterFactory.DefaultAot);

        // What MessagePack made for these options. Not a part of what the options say: a copy made by `with` starts
        // without them (the copy constructor below), and two options that say the same are equal whatever they hold.
        MessagePackSerializerOptions? plain;
        MessagePackSerializerOptions? sharedInterning;

        public MemoryDatabaseOptions()
        {
        }

        // what `with` calls
        MemoryDatabaseOptions(MemoryDatabaseOptions original)
        {
            InternString = original.InternString;
            ShareInternedStrings = original.ShareInternedStrings;
            FormatterFactory = original.FormatterFactory;
            MessageProcessor = original.MessageProcessor;
        }

        /// <summary>
        /// Whether a string is one object within a database, wherever it occurs: in two rows, in two tables. The
        /// default is true. It is about reading; a builder writes the same either way.
        /// </summary>
        public bool InternString { get; init; } = true;

        /// <summary>
        /// Whether the databases that are read with these options share their strings with one another. The default
        /// is false: each database has a pool of its own, which is gone when the database is loaded, so nothing but a
        /// database keeps its strings alive. Set to true, the pool is these options' own: a database that is loaded
        /// again (master data that is reloaded while the application runs) gets the string objects the one before it
        /// has, for every string that did not change. In return a string stays alive as long as the options do, also
        /// when no database has it any more. Has no meaning when <see cref="InternString"/> is false.
        /// </summary>
        public bool ShareInternedStrings { get; init; }

        /// <summary>
        /// The formatters of the rows and their members. The default, null, is
        /// <see cref="MessagePackFormatterFactory.DefaultAot"/>: source-generated and built-in formatters only, the
        /// same on the JIT as under IL2CPP and Native AOT.
        /// </summary>
        public MessagePackFormatterFactory? FormatterFactory { get; init; }

        /// <summary>
        /// What the tables are compressed with: a builder writes them through it, and a database reads them through
        /// it. The default, null, leaves a table as it is. The Lz4BlockProcessor of the MessagePack.LZ4 package is what
        /// a database built by MasterMemory v3 is read with; a processor also reads a table that is not compressed.
        /// </summary>
        public MessagePackMessageProcessor? MessageProcessor { get; init; }

        public bool Equals(MemoryDatabaseOptions? other)
        {
            return other is not null
                && InternString == other.InternString
                && ShareInternedStrings == other.ShareInternedStrings
                && Equals(FormatterFactory, other.FormatterFactory)
                && Equals(MessageProcessor, other.MessageProcessor);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = InternString ? 1 : 0;
                hash = (hash * 397) ^ (ShareInternedStrings ? 1 : 0);
                hash = (hash * 397) ^ (FormatterFactory?.GetHashCode() ?? 0);
                hash = (hash * 397) ^ (MessageProcessor?.GetHashCode() ?? 0);
                return hash;
            }
        }

        // What a builder writes with, and a database reads with when strings are not interned. Made once.
        internal MessagePackSerializerOptions GetWriteOptions()
        {
            return plain ?? Publish(ref plain, CreateOptions(FormatterFactory == null ? defaultResolver : CreateResolver(FormatterFactory)));
        }

        // What one database is read with.
        internal MessagePackSerializerOptions GetReadOptions()
        {
            if (!InternString)
            {
                return GetWriteOptions();
            }

            if (ShareInternedStrings)
            {
                return sharedInterning ?? Publish(ref sharedInterning, CreateOptions(CreateInterningResolver()));
            }

            // Made for this load. The pool is the interning factory's own, and the factory is this resolver's own:
            // both are gone when the database is loaded. The strings of a database are therefore one instance each
            // within that database, and nothing keeps them alive after the database itself is dropped. Making a
            // resolver for a load costs a kilobyte or two, measured, and no time that could be told apart.
            return CreateOptions(CreateInterningResolver());
        }

        // MessagePack's StringInterningFormatterFactory, in front of everything else: a string that was read before is
        // looked up in its pool and the instance found there is used (on .NET 9 and later by its characters, so a
        // string that is already there is not allocated a second time). It is put in front here and not by the
        // application, because a resolver does not give its chain back: nothing can be put in front of options that
        // are made already.
        MessagePackFormatterResolver CreateInterningResolver()
        {
            return CreateResolver(MessagePackFormatterFactory.Combine(new StringInterningFormatterFactory(), FormatterFactory ?? MessagePackFormatterFactory.DefaultAot));
        }

        MessagePackSerializerOptions CreateOptions(MessagePackFormatterResolver resolver)
        {
            return new MessagePackSerializerOptions(resolver) { MessageProcessor = MessageProcessor };
        }

        static MessagePackFormatterResolver CreateResolver(MessagePackFormatterFactory factory)
        {
            return new MessagePackFormatterResolver(factory) { HashFloodingResistant = false };
        }

        // Two threads that come here at the same time both get the one that was stored first: a pool that is shared
        // has to be one pool.
        static MessagePackSerializerOptions Publish(ref MessagePackSerializerOptions? field, MessagePackSerializerOptions made)
        {
            return Interlocked.CompareExchange(ref field, made, null) ?? made;
        }
    }
}

#if !NET5_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
    // what the compiler asks for where a property has an init accessor; in the framework from .NET 5 on
    internal static class IsExternalInit
    {
    }
}

#endif
