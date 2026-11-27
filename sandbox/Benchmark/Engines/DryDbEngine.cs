using System;
using MessagePack;

namespace Benchmark.Engines
{
    /// <summary>
    /// DryDB: a read-only B+Tree in a file, read through a page cache. The value of a row is its MessagePack binary,
    /// the same bytes MasterMemory stores for it.
    /// </summary>
    public sealed class DryDbEngine : IDisposable
    {
        const string TableName = "testdoc";

        readonly DryDB.ReadOnlyDatabase database;
        readonly DryDB.ReadOnlyTable table;

        public static void Build(string path, TestDoc[] rows)
        {
            using var builder = new DryDB.DatabaseBuilder { PageSize = 4096 };
            var tableBuilder = builder.CreateTable(TableName, DryDB.KeyEncoding.Int64LittleEndian);
            foreach (var row in rows)
            {
                tableBuilder.Append((long)row.Id, MessagePackSerializer.Serialize(row));
            }
            builder.BuildToFileAsync(path).AsTask().GetAwaiter().GetResult();
        }

        // the load options are the defaults, as in DryDB's own benchmarks
        public DryDbEngine(string path)
        {
            database = DryDB.ReadOnlyDatabase.OpenFileAsync(path).AsTask().GetAwaiter().GetResult();
            table = database.GetTable(TableName);
        }

        /// <summary>The row as an object: the lookup and the deserialization of what it found.</summary>
        public TestDoc Find(int id)
        {
            using var result = table.Get((long)id);
            return MessagePackSerializer.Deserialize<TestDoc>(result.Value.Span);
        }

        /// <summary>The lookup alone: the bytes of the row in the cached page, not deserialized.</summary>
        public int FindRaw(int id)
        {
            using var result = table.Get((long)id);
            return result.Value.Span.Length;
        }

        public void Dispose()
        {
            database.Dispose();
        }
    }
}
