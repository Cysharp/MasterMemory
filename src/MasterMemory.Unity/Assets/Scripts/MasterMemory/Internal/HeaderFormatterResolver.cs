using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;

namespace MasterMemory.Internal
{
    // for AOT(IL2CPP) concrete generic formatter.
    internal class HeaderFormatterResolver : IFormatterResolver
    {
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
        public int Serialize(ref byte[] bytes, int offset, ValueTuple<int, int> value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;
            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 2);

            offset += formatterResolver.GetFormatterWithVerify<int>().Serialize(ref bytes, offset, value.Item1, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<int>().Serialize(ref bytes, offset, value.Item2, formatterResolver);

            return offset - startOffset;
        }

        public ValueTuple<int, int> Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                throw new InvalidOperationException("Data is Nil, ValueTuple can not be null.");
            }
            else
            {
                var startOffset = offset;
                var count = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
                if (count != 2) throw new InvalidOperationException("Invalid ValueTuple count");
                offset += readSize;

                var item1 = formatterResolver.GetFormatterWithVerify<int>().Deserialize(bytes, offset, formatterResolver, out readSize);
                offset += readSize;
                var item2 = formatterResolver.GetFormatterWithVerify<int>().Deserialize(bytes, offset, formatterResolver, out readSize);
                offset += readSize;

                readSize = offset - startOffset;
                return new ValueTuple<int, int>(item1, item2);
            }
        }
    }
}