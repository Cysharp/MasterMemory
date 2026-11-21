using MasterMemory.Internal;
using MessagePack;
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

        /// <param name="databaseBinary">The database, as <see cref="DatabaseBuilderBase.Build"/> wrote it.</param>
        /// <param name="internString">Intern every string read from the tables.</param>
        /// <param name="tableFormatterFactory">The generated factory that serves the array of each table type.</param>
        /// <param name="formatterFactory">The formatters of the table types and their members; null is <see cref="MessagePackFormatterFactory.DefaultAot"/>.</param>
        /// <param name="maxDegreeOfParallelism">How many tables are deserialized at the same time.</param>
        protected MemoryDatabaseBase(byte[] databaseBinary, bool internString, MessagePackFormatterFactory tableFormatterFactory, MessagePackFormatterFactory? formatterFactory, int maxDegreeOfParallelism)
        {
            var header = DatabaseHeader.Read(databaseBinary, out var headerLength);
            if (maxDegreeOfParallelism < 1)
            {
                maxDegreeOfParallelism = 1;
            }

            var options = SerializerOptionsCache.Get(tableFormatterFactory, formatterFactory, internString);
            Init(header, databaseBinary.AsMemory(headerLength), options, maxDegreeOfParallelism);
        }

        protected static TView ExtractTableData<T, TView>(Dictionary<string, (int offset, int count)> header, ReadOnlyMemory<byte> databaseBinary, MessagePackSerializerOptions options, Func<T[], TView> createView)
        {
            var tableName = typeof(T).GetCustomAttribute<MemoryTableAttribute>();
            if (tableName == null) throw new InvalidOperationException("Type is not annotated MemoryTableAttribute. Type:" + typeof(T).FullName);

            if (header.TryGetValue(tableName.TableName, out var segment))
            {
                var data = MessagePackSerializer.Deserialize<T[]>(databaseBinary.Span.Slice(segment.offset, segment.count), options);
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
            var header = DatabaseHeader.Read(databaseBinary, out var headerLength);

            // a table's offset counts from the end of the header
            return header.Select(x => new TableInfo(x.Key, x.Value.count, storeTableData ? databaseBinary : null, headerLength + x.Value.offset)).ToArray();
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
        byte[]? binaryData;

        public TableInfo(string tableName, int size, byte[]? rawBinary, int offset)
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

            // a small table is stored without the Lz4Block envelope, and TryDecode says so
            if (!SerializerOptionsCache.TableProcessor.TryDecode(binaryData, out var decoded))
            {
                return MessagePackSerializer.ConvertToJson(binaryData);
            }

            using (decoded)
            {
                if (decoded.TryGetValue(out DecodedMessage.Buffer buffer))
                {
                    return MessagePackSerializer.ConvertToJson(buffer.Sequence);
                }
                if (decoded.TryGetValue(out DecodedMessage.SourceSlice slice))
                {
                    return MessagePackSerializer.ConvertToJson(binaryData.AsSpan(slice.Offset, slice.Length));
                }

                throw new InvalidOperationException("The table data could not be decoded. TableName: " + TableName);
            }
        }
    }
}