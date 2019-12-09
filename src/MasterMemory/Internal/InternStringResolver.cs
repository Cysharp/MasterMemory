using MessagePack;
using MessagePack.Formatters;
using System;

namespace MasterMemory.Internal
{
    internal class InternStringResolver : IFormatterResolver, IMessagePackFormatter<string>
    {
        readonly IFormatterResolver innerResolver;

        public InternStringResolver(IFormatterResolver innerResolver)
        {
            this.innerResolver = innerResolver;
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (typeof(T) == typeof(string))
            {
                return (IMessagePackFormatter<T>)this;
            }

            return innerResolver.GetFormatter<T>();
        }

        string IMessagePackFormatter<string>.Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var str = reader.ReadString();
            if (str == null)
            {
                return null;
            }

            return string.Intern(str);
        }

        void IMessagePackFormatter<string>.Serialize(ref MessagePackWriter writer, string value, MessagePackSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}