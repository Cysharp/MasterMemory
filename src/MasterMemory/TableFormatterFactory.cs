using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;

namespace MasterMemory
{
    /// <summary>
    /// Serves the array formatter of every table type. A table is stored as <c>T[]</c>, a root type that no member
    /// declares, so <see cref="MessagePackFormatterFactory.DefaultAot"/> alone cannot construct it; the MasterMemory
    /// source generator lists the table types here instead, which keeps a database loadable under IL2CPP and Native AOT.
    /// One lookup by type, however many tables there are.
    /// </summary>
    public sealed class TableFormatterFactory : MessagePackFormatterFactory
    {
        readonly Dictionary<Type, MessagePackFormatterFactory> arrayFactories;

        TableFormatterFactory(Dictionary<Type, MessagePackFormatterFactory> arrayFactories)
        {
            this.arrayFactories = arrayFactories;
        }

#if NET9_0_OR_GREATER
        public override object? CreateFormatter<TWriteBuffer, TReadBuffer>(Type type)
        {
            return arrayFactories.TryGetValue(type, out var factory)
                ? factory.CreateFormatter<TWriteBuffer, TReadBuffer>(type)
                : null;
        }
#endif

        public override object? CreateFormatter(Type writeBufferType, Type readBufferType, Type valueType)
        {
            return arrayFactories.TryGetValue(valueType, out var factory)
                ? factory.CreateFormatter(writeBufferType, readBufferType, valueType)
                : null;
        }

        /// <summary>Collects the table types; the generated code is its only intended caller.</summary>
        public sealed class Builder
        {
            readonly Dictionary<Type, MessagePackFormatterFactory> arrayFactories;

            public Builder(int capacity)
            {
                arrayFactories = new Dictionary<Type, MessagePackFormatterFactory>(capacity);
            }

            public Builder Add<T>()
            {
                arrayFactories[typeof(T[])] = new ArrayFormatterFactory<T>();
                return this;
            }

            public TableFormatterFactory Build()
            {
                // a copy: the factory must not change once a resolver has cached formatters from it
                return new TableFormatterFactory(new Dictionary<Type, MessagePackFormatterFactory>(arrayFactories));
            }
        }
    }
}
