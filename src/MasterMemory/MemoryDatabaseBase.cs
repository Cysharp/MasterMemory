using MasterMemory.Internal;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Buffers;
using MasterMemory.Validation;

namespace MasterMemory
{
    public abstract class MemoryDatabaseBase
    {
        protected MemoryDatabaseBase()
        {

        }

        public MemoryDatabaseBase(byte[] databaseBinary, bool internString = true, IFormatterResolver formatterResolver = null, int maxDegreeOfParallelism = 1)
        {
            var reader = new MessagePackReader(databaseBinary);
            var formatter = new DictionaryFormatter<string, (int, int)>();

            var header = formatter.Deserialize(ref reader, HeaderFormatterResolver.StandardOptions);
            var resolver = formatterResolver ?? MessagePackSerializer.DefaultOptions.Resolver;
            if (internString)
            {
                resolver = new InternStringResolver(resolver);
            }
            if (maxDegreeOfParallelism < 1)
            {
                maxDegreeOfParallelism = 1;
            }

            Init(header, databaseBinary.AsMemory((int)reader.Consumed), MessagePackSerializer.DefaultOptions.WithResolver(resolver).WithCompression(MessagePackCompression.Lz4Block), maxDegreeOfParallelism);
        }

        protected static TView ExtractTableData<T, TView>(Dictionary<string, (int offset, int count)> header, ReadOnlyMemory<byte> databaseBinary, MessagePackSerializerOptions options, Func<T[], TView> createView)
        {
            var tableName = typeof(T).GetCustomAttribute<MemoryTableAttribute>();
            if (tableName == null) throw new InvalidOperationException("Type is not annotated MemoryTableAttribute. Type:" + typeof(T).FullName);

            if (header.TryGetValue(tableName.TableName, out var segment))
            {
                var data = MessagePackSerializer.Deserialize<T[]>(databaseBinary.Slice(segment.offset, segment.count), options);
                return createView(data);
            }
            else
            {
                // return empty
                var data = Array.Empty<T>();
                return createView(data);
            }
        }

        protected abstract void Init(Dictionary<string, (int offset, int count)> header, ReadOnlyMemory<byte> databaseBinary, MessagePackSerializerOptions options, int maxDegreeOfParallelism);

        public static TableInfo[] GetTableInfo(byte[] databaseBinary, bool storeTableData = true)
        {
            var formatter = new DictionaryFormatter<string, (int, int)>();
            var reader = new MessagePackReader(databaseBinary);
            var header = formatter.Deserialize(ref reader, HeaderFormatterResolver.StandardOptions);

            return header.Select(x => new TableInfo(x.Key, x.Value.Item2, storeTableData ? databaseBinary : null, x.Value.Item1)).ToArray();
        }

        protected void ValidateTable<TElement>(IReadOnlyList<TElement> table, ValidationDatabase database, string pkName, Delegate pkSelector, ValidateResult result)
        {
            var onceCalled = new System.Runtime.CompilerServices.StrongBox<bool>(false);
            foreach (var item in table)
            {
                if (item is IValidatable<TElement> validatable)
                {
                    var validator = new Validator<TElement>(database, item, result, onceCalled, pkName, pkSelector);
                    validatable.Validate(validator);
                }
            }
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
            return DumpAsJson(MessagePackSerializer.DefaultOptions);
        }

        public string DumpAsJson(MessagePackSerializerOptions options)
        {
            if (binaryData == null)
            {
                throw new InvalidOperationException("DumpAsJson can only call from GetTableInfo(storeTableData = true).");
            }

            return MessagePackSerializer.ConvertToJson(binaryData, options.WithCompression(MessagePackCompression.Lz4Block));
        }
    }
}