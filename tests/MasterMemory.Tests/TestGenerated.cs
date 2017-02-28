
namespace MessagePack.Resolvers
{
    using System;
    using MessagePack;

    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        GeneratedResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(1)
            {
                {typeof(global::MasterMemory.Tests.Sample), 0 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new MessagePack.Formatters.MasterMemory.Tests.SampleFormatter();
                default: return null;
            }
        }
    }
}




namespace MessagePack.Formatters.MasterMemory.Tests
{
    using System;
    using MessagePack;


    public sealed class SampleFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::MasterMemory.Tests.Sample>
    {

        public int Serialize(ref byte[] bytes, int offset, global::MasterMemory.Tests.Sample value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Id);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Age);
            offset += MessagePackBinary.WriteString(ref bytes, offset, value.FirstName);
            offset += MessagePackBinary.WriteString(ref bytes, offset, value.LastName);
            return offset - startOffset;
        }

        public global::MasterMemory.Tests.Sample Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Id__ = default(int);
            var __Age__ = default(int);
            var __FirstName__ = default(string);
            var __LastName__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Id__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Age__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __FirstName__ = MessagePackBinary.ReadString(bytes, offset, out readSize);
                        break;
                    case 3:
                        __LastName__ = MessagePackBinary.ReadString(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNext(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::MasterMemory.Tests.Sample();
            ____result.Id = __Id__;
            ____result.Age = __Age__;
            ____result.FirstName = __FirstName__;
            ____result.LastName = __LastName__;
            return ____result;
        }
    }

}
