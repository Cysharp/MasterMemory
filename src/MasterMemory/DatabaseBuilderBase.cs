using MasterMemory.Internal;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;

namespace MasterMemory
{
    /// <summary>
    /// The tables of a database while they can still be changed. Rows go in, a table at a time; what comes out is the
    /// database binary (<see cref="Build"/>, <see cref="WriteToStream"/>) or, from the generated class, the database
    /// itself.
    /// </summary>
    /// <remarks>
    /// A table is held as its rows in the order of the primary key, and every change makes a new array: an array that
    /// was handed in, or that a database made from this builder holds, is never written to.
    /// </remarks>
    public abstract class DatabaseBuilderBase
    {
        readonly MessagePackSerializerOptions options;

        // in the order the tables were set first, which is the order they are written in
        readonly List<TableEntry> entries = new List<TableEntry>();

        /// <param name="tableFormatterFactory">The generated factory that serves the array of each table type.</param>
        /// <param name="formatterFactory">The formatters of the table types and their members; null is <see cref="MessagePackFormatterFactory.DefaultAot"/>.</param>
        protected DatabaseBuilderBase(MessagePackFormatterFactory tableFormatterFactory, MessagePackFormatterFactory? formatterFactory)
        {
            this.options = SerializerOptionsCache.Get(tableFormatterFactory, formatterFactory, internString: false);
        }

        /// <summary>One table of the builder. The generated class has a field of it per table, null until the table is set.</summary>
        protected abstract class TableEntry
        {
            internal readonly string TableName;

            internal TableEntry(string tableName)
            {
                TableName = tableName;
            }

            internal abstract void Serialize(ByteBufferWriter writer, MessagePackSerializerOptions options);
        }

        /// <inheritdoc cref="TableEntry"/>
        protected sealed class TableEntry<TRow, TTable> : TableEntry
            where TTable : TableBase<TRow>
        {
            // in the order of the primary key
            internal TRow[] Rows;

            // the table over Rows once there is one: the table of the database the builder was made from, or the one
            // made for a database. A change of the rows takes it away.
            internal TTable? Table;

            internal TableEntry(string tableName, TRow[] rows, TTable? table)
                : base(tableName)
            {
                Rows = rows;
                Table = table;
            }

            internal override void Serialize(ByteBufferWriter writer, MessagePackSerializerOptions options)
            {
                MessagePackSerializer.Serialize(writer, Rows, options);
            }
        }

        // ---- what the generated methods call ----

        /// <summary>The rows of a table that the builder does not have yet.</summary>
        protected void AppendCore<TRow, TTable, TKey>(ref TableEntry<TRow, TTable>? entry, string tableName, IEnumerable<TRow>? dataSource, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
            where TTable : TableBase<TRow>
        {
            if (entry != null)
            {
                throw new InvalidOperationException("TableName is already appended in builder, use ReplaceAll or AddOrReplace to change its rows. TableName: " + tableName + " Type:" + typeof(TRow).FullName);
            }

            if (dataSource == null) return;

            SetRows(ref entry, tableName, Sort(tableName, dataSource, keySelector, comparer, out _));
        }

        /// <summary>The rows of a table, in place of the ones it has.</summary>
        protected void ReplaceAllCore<TRow, TTable, TKey>(ref TableEntry<TRow, TTable>? entry, string tableName, IEnumerable<TRow> dataSource, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
            where TTable : TableBase<TRow>
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));

            SetRows(ref entry, tableName, Sort(tableName, dataSource, keySelector, comparer, out _));
        }

        /// <summary>
        /// Each row takes the place of the rows that have its primary key, or is added when there is none. Of two rows
        /// in <paramref name="dataSource"/> with one key the later one stays, as if they had been given one by one.
        /// </summary>
        protected void AddOrReplaceCore<TRow, TTable, TKey>(ref TableEntry<TRow, TTable>? entry, string tableName, IEnumerable<TRow> dataSource, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
            where TTable : TableBase<TRow>
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));

            var incoming = SortLastOfEachKey(tableName, dataSource, keySelector, comparer, out var incomingKeys);
            if (incoming.Length == 0) return;

            if (entry == null || entry.Rows.Length == 0)
            {
                SetRows(ref entry, tableName, incoming);
                return;
            }

            // both are in the order of the key: one pass over the two
            var existing = entry.Rows;
            var merged = new TRow[existing.Length + incoming.Length];
            var count = 0;
            var i = 0;
            var j = 0;
            while (i < existing.Length && j < incoming.Length)
            {
                var order = comparer.Compare(keySelector(existing[i]), incomingKeys[j]);
                if (order < 0)
                {
                    merged[count++] = existing[i++];
                }
                else if (order > 0)
                {
                    merged[count++] = incoming[j++];
                }
                else
                {
                    i++; // replaced: the incoming row is taken once the existing rows are past its key
                }
            }
            while (i < existing.Length) merged[count++] = existing[i++];
            while (j < incoming.Length) merged[count++] = incoming[j++];

            if (count != merged.Length)
            {
                Array.Resize(ref merged, count);
            }
            SetRows(ref entry, tableName, merged);
        }

        /// <summary>Takes out the rows that have one of the primary keys. A key that no row has is passed over.</summary>
        protected void RemoveCore<TRow, TTable, TKey>(ref TableEntry<TRow, TTable>? entry, TKey[] keys, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
            where TTable : TableBase<TRow>
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (entry == null || entry.Rows.Length == 0 || keys.Length == 0) return;

            var sortedKeys = (TKey[])keys.Clone();
            Array.Sort(sortedKeys, 0, sortedKeys.Length, comparer);

            // both are in the order of the key: one pass over the two. Nothing is copied before the first row to go.
            var existing = entry.Rows;
            TRow[]? remaining = null;
            var count = 0;
            var j = 0;
            for (var i = 0; i < existing.Length; i++)
            {
                var key = keySelector(existing[i]);
                while (j < sortedKeys.Length && comparer.Compare(sortedKeys[j], key) < 0)
                {
                    j++;
                }

                if (j < sortedKeys.Length && comparer.Compare(sortedKeys[j], key) == 0)
                {
                    if (remaining == null)
                    {
                        remaining = new TRow[existing.Length - 1];
                        Array.Copy(existing, 0, remaining, 0, i);
                        count = i;
                    }
                }
                else if (remaining != null)
                {
                    remaining[count++] = existing[i];
                }
            }

            if (remaining == null) return; // no row had any of the keys: the rows stay, and so does a table over them

            if (count != remaining.Length)
            {
                Array.Resize(ref remaining, count);
            }
            entry.Rows = remaining;
            entry.Table = null;
        }

        /// <summary>A table of a database: its rows as they are, and the table itself for a database made from the builder.</summary>
        protected void SetTable<TRow, TTable>(ref TableEntry<TRow, TTable>? entry, string tableName, TTable table)
            where TTable : TableBase<TRow>
        {
            SetRows(ref entry, tableName, table.GetRawDataUnsafe());
            entry!.Table = table;
        }

        /// <summary>
        /// The table of an entry for a database: the one it has, or one made now and kept until the rows change. An
        /// entry that was never set gives an empty table, like a table that a database binary does not contain.
        /// </summary>
        protected static TTable GetTable<TRow, TTable>(TableEntry<TRow, TTable>? entry, Func<TRow[], TTable> createTable)
            where TTable : TableBase<TRow>
        {
            if (entry == null)
            {
                return createTable(Array.Empty<TRow>());
            }

            return entry.Table ??= createTable(entry.Rows);
        }

        void SetRows<TRow, TTable>(ref TableEntry<TRow, TTable>? entry, string tableName, TRow[] rows)
            where TTable : TableBase<TRow>
        {
            if (entry == null)
            {
                entry = new TableEntry<TRow, TTable>(tableName, rows, null);
                entries.Add(entry);
            }
            else
            {
                entry.Rows = rows;
                entry.Table = null;
            }
        }

        // ---- sorting ----

        // the rows in a new array, in the order of the key, and their keys beside them
        static TRow[] Sort<TRow, TKey>(string tableName, IEnumerable<TRow> dataSource, Func<TRow, TKey> keySelector, IComparer<TKey> comparer, out TKey[] keys)
        {
            var rows = ToNewArray(dataSource);
            keys = KeysOf(tableName, rows, keySelector);
            Array.Sort(keys, rows, 0, rows.Length, comparer);
            return rows;
        }

        // The same, with one row for each key: of the rows that share one, the last in dataSource. The sort does not
        // keep rows of one key in the order they came in, so it is the positions that are sorted, and the highest
        // position of each key that is taken.
        static TRow[] SortLastOfEachKey<TRow, TKey>(string tableName, IEnumerable<TRow> dataSource, Func<TRow, TKey> keySelector, IComparer<TKey> comparer, out TKey[] keys)
        {
            var source = ToNewArray(dataSource);
            var sourceKeys = KeysOf(tableName, source, keySelector);
            var positions = new int[source.Length];
            for (var i = 0; i < positions.Length; i++)
            {
                positions[i] = i;
            }
            Array.Sort(sourceKeys, positions, 0, positions.Length, comparer);

            var rows = new TRow[source.Length];
            keys = new TKey[source.Length];
            var count = 0;
            for (var i = 0; i < positions.Length; i++)
            {
                if (count != 0 && comparer.Compare(keys[count - 1], sourceKeys[i]) == 0)
                {
                    if (positions[i] > positions[i - 1])
                    {
                        rows[count - 1] = source[positions[i]];
                    }
                    else
                    {
                        positions[i] = positions[i - 1]; // the latest of this key so far, for the next row to be held against
                    }
                    continue;
                }

                rows[count] = source[positions[i]];
                keys[count] = sourceKeys[i];
                count++;
            }

            if (count != rows.Length)
            {
                Array.Resize(ref rows, count);
                Array.Resize(ref keys, count);
            }
            return rows;
        }

        static TRow[] ToNewArray<TRow>(IEnumerable<TRow> dataSource)
        {
            if (dataSource is ICollection<TRow> collection)
            {
                var array = new TRow[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }

            return new List<TRow>(dataSource).ToArray();
        }

        static TKey[] KeysOf<TRow, TKey>(string tableName, TRow[] rows, Func<TRow, TKey> keySelector)
        {
            var keys = new TKey[rows.Length];
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i] == null)
                {
                    throw new ArgumentException("A row is null. TableName: " + tableName + " Type:" + typeof(TRow).FullName + " Index:" + i);
                }
                keys[i] = keySelector(rows[i]);
            }
            return keys;
        }

        // ---- the database binary ----

        /// <summary>The database binary: what the constructor of the generated MemoryDatabase reads.</summary>
        public byte[] Build()
        {
            var tables = SerializeTables(out var header);
            using (var headerStream = new MemoryStream())
            {
                DatabaseHeader.Write(headerStream, header);
                var headerLength = checked((int)headerStream.Length);

                var binary = new byte[checked(headerLength + tables.CurrentOffset)];
                headerStream.Position = 0;
                headerStream.Read(binary, 0, headerLength);
                tables.WrittenSpan.CopyTo(binary.AsSpan(headerLength));
                return binary;
            }
        }

        /// <summary>Writes what <see cref="Build"/> returns to a stream.</summary>
        public void WriteToStream(Stream stream)
        {
            var tables = SerializeTables(out var header);
            DatabaseHeader.Write(stream, header);
            var written = tables.WrittenSegment;
            stream.Write(written.Array!, written.Offset, written.Count);
        }

        // every table that was set, one after another; the header says where each of them is
        ByteBufferWriter SerializeTables(out Dictionary<string, (int offset, int count)> header)
        {
            var writer = new ByteBufferWriter();
            header = new Dictionary<string, (int offset, int count)>(entries.Count);
            foreach (var entry in entries)
            {
                var offset = writer.CurrentOffset;
                entry.Serialize(writer, options);
                header.Add(entry.TableName, (offset, writer.CurrentOffset - offset));
            }
            return writer;
        }
    }
}
