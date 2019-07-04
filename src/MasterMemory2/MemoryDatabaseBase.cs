using MasterMemory.Annotations;
using MasterMemory.Internal;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MasterMemory
{
    public abstract class MemoryDatabaseBase
    {
        public MemoryDatabaseBase(byte[] databaseBinary, bool internString = true, IFormatterResolver formatterResolver = null)
        {
            var formatter = new DictionaryFormatter<string, (int, int)>();
            var header = formatter.Deserialize(databaseBinary, 0, new HeaderFormatterResolver(), out var headerOffset);

            var resolver = formatterResolver ?? MessagePackSerializer.DefaultResolver;
            if (internString)
            {
                resolver = new InternStringResolver(resolver);
            }

            Init(header, headerOffset, databaseBinary, resolver);
        }

        protected static TView ExtractTableData<T, TView>(Dictionary<string, (int offset, int count)> header, int headerOffset, byte[] databaseBinary, IFormatterResolver resolver, Func<T[], TView> createView)
        {
            var tableName = typeof(T).GetCustomAttribute<MemoryTableAttribute>();
            if (tableName == null) throw new InvalidOperationException("Type is not annotated MemoryTableAttribute. Type:" + typeof(T).FullName);

            if (header.TryGetValue(tableName.TableName, out var segment))
            {
                var data = LZ4MessagePackSerializer.Deserialize<T[]>(new ArraySegment<byte>(databaseBinary, headerOffset + segment.offset, segment.count), resolver);
                return createView(data);
            }
            else
            {
                return default(TView);
            }
        }

        protected abstract void Init(Dictionary<string, (int offset, int count)> header, int headerOffset, byte[] databaseBinary, IFormatterResolver resolver);


        // TODO: ToImmutableBuilder()
    }
}