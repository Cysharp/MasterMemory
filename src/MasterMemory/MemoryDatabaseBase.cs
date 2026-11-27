using MasterMemory.Internal;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Buffers;
using MasterMemory.Validation;

namespace MasterMemory
{
    public abstract class MemoryDatabaseBase
    {
        /// <summary>
        /// What the database was read with, or what the builder it was made from writes with. A builder made by the
        /// generated ToDatabaseBuilder writes with these, unless it is given others.
        /// </summary>
        public MemoryDatabaseOptions Options { get; }

        /// <summary>A database whose tables the generated constructor sets itself: the one made from a builder.</summary>
        /// <param name="options">The options of the builder; null is <see cref="MemoryDatabaseOptions.Default"/>.</param>
        protected MemoryDatabaseBase(MemoryDatabaseOptions? options)
        {
            Options = options ?? MemoryDatabaseOptions.Default;
        }

        /// <param name="databaseBinary">The database, as <see cref="DatabaseBuilderBase.Build"/> wrote it.</param>
        /// <param name="options">
        /// How every table is read: the formatters of the rows and their members, the processor if the tables are
        /// compressed, whether strings are interned. Null is <see cref="MemoryDatabaseOptions.Default"/>.
        /// </param>
        /// <param name="maxDegreeOfParallelism">How many tables are deserialized at the same time.</param>
        protected MemoryDatabaseBase(byte[] databaseBinary, MemoryDatabaseOptions? options, int maxDegreeOfParallelism)
            : this(new ReadOnlyMemory<byte>(databaseBinary ?? throw new ArgumentNullException(nameof(databaseBinary))), options, maxDegreeOfParallelism)
        {
        }

        /// <summary>
        /// The database from memory that need not be an array of its own: a part of a larger buffer, a rented array,
        /// native memory behind a <see cref="MemoryManager{T}"/>. The memory is read until the constructor returns and
        /// not kept; every row is an object of its own.
        /// </summary>
        /// <inheritdoc cref="MemoryDatabaseBase(byte[], MemoryDatabaseOptions, int)"/>
        protected MemoryDatabaseBase(ReadOnlyMemory<byte> databaseBinary, MemoryDatabaseOptions? options, int maxDegreeOfParallelism)
            : this(options)
        {
            var header = DatabaseHeader.Read(databaseBinary, out var headerLength);
            if (maxDegreeOfParallelism < 1)
            {
                maxDegreeOfParallelism = 1;
            }

            Init(header, databaseBinary.Slice(headerLength), Options.GetReadOptions(), maxDegreeOfParallelism);
        }

        /// <summary>
        /// The database from a span, which can be over anything: native memory, a stack buffer. The tables are read one
        /// after another; reading them at the same time takes memory that can be handed to other threads, which is what
        /// the other constructors are given. The span is not kept.
        /// </summary>
        /// <inheritdoc cref="MemoryDatabaseBase(byte[], MemoryDatabaseOptions, int)"/>
        protected MemoryDatabaseBase(ReadOnlySpan<byte> databaseBinary, MemoryDatabaseOptions? options)
            : this(options)
        {
            var header = DatabaseHeader.Read(databaseBinary, out var headerLength);
            Init(header, databaseBinary.Slice(headerLength), Options.GetReadOptions());
        }

        protected static TView ExtractTableData<T, TView>(Dictionary<string, (int offset, int count)> header, ReadOnlyMemory<byte> databaseBinary, MessagePackSerializerOptions options, string tableName, Func<T[], TView> createView)
        {
            return ExtractTableData(header, databaseBinary.Span, options, tableName, createView);
        }

        // tableName is the name the table has in the header: what its [MemoryTable] says, which the generated code knows
        protected static TView ExtractTableData<T, TView>(Dictionary<string, (int offset, int count)> header, ReadOnlySpan<byte> databaseBinary, MessagePackSerializerOptions options, string tableName, Func<T[], TView> createView)
        {
            if (header.TryGetValue(tableName, out var segment))
            {
                DatabaseHeader.ThrowIfOutside(tableName, segment.offset, segment.count, databaseBinary.Length);
                var table = databaseBinary.Slice(segment.offset, segment.count);

                // The formatter of the rows would say only that an ext is where an array should be.
                if (options.MessageProcessor == null && TableSerialization.LooksCompressed(table))
                {
                    throw TableSerialization.CompressedWithoutProcessor(tableName);
                }

                var data = MessagePackSerializer.Deserialize<T[]>(table, options);

                // A table is an array of rows. Nil in place of the array or of a row is well-formed MessagePack and so
                // gets this far, where a table would fail on it with a NullReferenceException somewhere inside.
                if (data == null)
                {
                    throw new InvalidOperationException($"Invalid database, the table '{tableName}' is nil where its rows should be. The binary may be damaged.");
                }
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == null)
                    {
                        throw new InvalidOperationException($"Invalid database, the table '{tableName}' has nil where its row {i} should be. The binary may be damaged.");
                    }
                }
                return createView(data);
            }
            else
            {
                // return empty
                var data = Array.Empty<T>();
                return createView(data);
            }
        }

        /// <summary>Reads the tables, one after another or <paramref name="maxDegreeOfParallelism"/> at the same time.</summary>
        protected abstract void Init(Dictionary<string, (int offset, int count)> header, ReadOnlyMemory<byte> databaseBinary, MessagePackSerializerOptions options, int maxDegreeOfParallelism);

        /// <summary>Reads the tables one after another.</summary>
        protected abstract void Init(Dictionary<string, (int offset, int count)> header, ReadOnlySpan<byte> databaseBinary, MessagePackSerializerOptions options);

        public static TableInfo[] GetTableInfo(byte[] databaseBinary, bool storeTableData = true)
        {
            if (databaseBinary == null) throw new ArgumentNullException(nameof(databaseBinary));
            return GetTableInfo(new ReadOnlySpan<byte>(databaseBinary), storeTableData);
        }

        public static TableInfo[] GetTableInfo(ReadOnlySpan<byte> databaseBinary, bool storeTableData = true)
        {
            var header = DatabaseHeader.Read(databaseBinary, out var headerLength);

            // a table's offset counts from the end of the header
            foreach (var item in header)
            {
                DatabaseHeader.ThrowIfOutside(item.Key, item.Value.offset, item.Value.count, databaseBinary.Length - headerLength);
            }

            var tables = new TableInfo[header.Count];
            var index = 0;
            foreach (var item in header)
            {
                var data = storeTableData ? databaseBinary.Slice(headerLength + item.Value.offset, item.Value.count).ToArray() : null;
                tables[index++] = new TableInfo(item.Key, item.Value.count, data);
            }
            return tables;
        }

        protected void ValidateTable<TElement>(RangeView<TElement> table, ValidationDatabase database, string pkName, Delegate pkSelector, ValidateResult result)
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

        // binaryData is the table alone, and this TableInfo's own
        internal TableInfo(string tableName, int size, byte[]? binaryData)
        {
            TableName = tableName;
            Size = size;
            this.binaryData = binaryData;
        }

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

        /// <summary>The table as JSON. A table that is compressed needs the processor it was written with: <see cref="DumpAsJson(MemoryDatabaseOptions)"/>.</summary>
        public string DumpAsJson()
        {
            return DumpAsJson(null);
        }

        /// <summary>
        /// The table as JSON, read with the processor of <paramref name="options"/>: the options the database was
        /// built with, for a table that is compressed. A table that is not compressed is read with or without one.
        /// </summary>
        public string DumpAsJson(MemoryDatabaseOptions? options)
        {
            if (binaryData == null)
            {
                throw new InvalidOperationException("DumpAsJson can only call from GetTableInfo(storeTableData = true).");
            }

            var processor = options?.MessageProcessor;
            if (processor == null)
            {
                if (TableSerialization.LooksCompressed(binaryData))
                {
                    throw TableSerialization.CompressedWithoutProcessor(TableName);
                }
                return MessagePackSerializer.ConvertToJson(binaryData);
            }

            // a processor leaves a message that is not its own as it is, and says so: a table that was too small to compress
            if (!processor.TryDecode(binaryData, out var decoded))
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