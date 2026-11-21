using MessagePack;
using SerializerFoundation;
using System;

namespace MasterMemory.Internal
{
    // MsgPack104 steers formatters to the resolver-configured string formatter; this IS that customization.
#pragma warning disable MsgPack104

    // string.Intern, as MasterMemory has always done, not MessagePack's StringInterningFormatterFactory: the runtime's pool
    // is process-wide (every database shares it) and takes no lock of ours, so tables loaded in parallel do not queue on it.
    internal sealed partial class InternStringFormatter<TWriteBuffer, TReadBuffer> : IMessagePackFormatter<TWriteBuffer, TReadBuffer, string?>
    {
        public void Initialize(MessagePackFormatterResolver resolver)
        {
        }

        public void Serialize(ref TWriteBuffer buffer, ref SerializeState state, string? value)
        {
            buffer.WriteString(value);
        }

        public void Deserialize(ref TReadBuffer buffer, ref DeserializeState state, ref string? value)
        {
            var str = buffer.ReadString();
            value = (str == null) ? null : string.Intern(str);
        }
    }

    internal sealed partial class InternStringFormatterFactory : MessagePackFormatterFactory
    {
        public static readonly InternStringFormatterFactory Instance = new InternStringFormatterFactory();

        InternStringFormatterFactory()
        {
        }

        // one method, two signatures: net9+ overrides the base virtual (constraints inherited); the netstandard builds
        // have no such base member, so the constraints are spelled out
#if NET9_0_OR_GREATER
        public override object? CreateFormatter<TWriteBuffer, TReadBuffer>(Type type)
#else
        public object? CreateFormatter<TWriteBuffer, TReadBuffer>(Type type)
            where TWriteBuffer : struct, IWriteBuffer
            where TReadBuffer : struct, IReadBuffer
#endif
        {
            if (type == typeof(string))
            {
                return new InternStringFormatter<TWriteBuffer, TReadBuffer>();
            }

            return null;
        }
    }
}
