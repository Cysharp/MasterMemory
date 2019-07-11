using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MasterMemory.Tests
{
    public class MessagePackResolver : IFormatterResolver
    {
        public static IFormatterResolver Instance = new MessagePackResolver();

        MessagePackResolver()
        {

        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return MasterMemoryResolver.Instance.GetFormatter<T>()
                ?? GeneratedResolver.Instance.GetFormatter<T>()
                ?? StandardResolver.Instance.GetFormatter<T>();
        }
    }
}
