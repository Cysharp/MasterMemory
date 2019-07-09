using MessagePack;
using MessagePack.Formatters;
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
                return (IMessagePackFormatter<T>)(object)new ValueTupleFormatter<int, int>();
            }
            else if (typeof(T) == typeof(int))
            {
                return (IMessagePackFormatter<T>)(object)Int32Formatter.Instance;
            }

            return null;
        }
    }
}