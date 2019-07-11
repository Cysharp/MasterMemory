using MasterMemory.Internal;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace MasterMemory
{
    public abstract class MemoryDatabaseBase
    {
        protected MemoryDatabaseBase()
        {

        }

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

        public static TableInfo[] GetTableInfo(byte[] databaseBinary, bool storeTableData = true)
        {
            var formatter = new DictionaryFormatter<string, (int, int)>();
            var header = formatter.Deserialize(databaseBinary, 0, new HeaderFormatterResolver(), out var headerOffset);

            return header.Select(x => new TableInfo(x.Key, x.Value.Item2, storeTableData ? databaseBinary : null, x.Value.Item1)).ToArray();
        }
    }

    /// <summary>
    /// Diagnostic info of MasterMemory's table.
    /// </summary>
    public class TableInfo
    {
        public string TableName { get; }
        public int Size { get; }
        byte[] binaryData;

        public TableInfo(string tableName, int size, byte[] rawBinary, int offset)
        {
            TableName = tableName;
            Size = size;
            if (rawBinary != null)
            {
                this.binaryData = new byte[size];
                Array.Copy(rawBinary, offset, binaryData, 0, size);
            }
        }

        public string DumpAsJson()
        {
            if (binaryData == null)
            {
                throw new InvalidOperationException("DumpAsJson can only call from GetTableInfo(storeTableData = true).");
            }
            return LZ4MessagePackSerializer.ToJson(binaryData);
        }
    }
}