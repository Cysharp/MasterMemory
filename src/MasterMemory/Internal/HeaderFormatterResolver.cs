using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;

namespace MasterMemory.Internal
{
    // for AOT(IL2CPP) concrete generic formatter.
    internal class HeaderFormatterResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new HeaderFormatterResolver();
        public static readonly MessagePackSerializerOptions StandardOptions = MessagePackSerializerOptions.Standard.WithResolver(Instance);

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (typeof(T) == typeof(Dictionary<string, (int, int)>))
            {
                return (IMessagePackFormatter<T>)(object)new DictionaryFormatter<string, (int, int)>();
            }
            else if (typeof(T) == typeof(string))
            {
                return (IMessagePackFormatter<T>)(object)NullableStringFormatter.Instance;
            }
            else if (typeof(T) == typeof((int, int)))
            {
                return (IMessagePackFormatter<T>)(object)new IntIntValueTupleFormatter();
            }
            else if (typeof(T) == typeof(int))
            {
                return (IMessagePackFormatter<T>)(object)Int32Formatter.Instance;
            }

            return null;
        }
    }

    internal sealed class IntIntValueTupleFormatter : IMessagePackFormatter<ValueTuple<int, int>>
    {
        public void Serialize(ref MessagePackWriter writer, (int, int) value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(2);
            writer.WriteInt32(value.Item1);
            writer.WriteInt32(value.Item2);
        }

        public (int, int) Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.IsNil)
            {
                throw new InvalidOperationException("Data is Nil, ValueTuple can not be null.");
            }

            var count = reader.ReadArrayHeader();
            if (count != 2) throw new InvalidOperationException("Invalid ValueTuple count");

            var item1 = reader.ReadInt32();
            var item2 = reader.ReadInt32();

            return new ValueTuple<int, int>(item1, item2);
        }
    }
}