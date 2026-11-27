using System;
using System.Buffers.Binary;
using MessagePack;
using RocksDbSharp;

namespace Benchmark.Engines
{
    /// <summary>RocksDB through its C# binding. The value of a row is its MessagePack binary.</summary>
    public sealed class RocksDbEngine : IDisposable
    {
        readonly RocksDb db;
        readonly byte[] keyBuffer = new byte[sizeof(int)]; // one lookup at a time, as the benchmarks do

        public static void Build(string path, TestDoc[] rows)
        {
            using var db = RocksDb.Open(new DbOptions().SetCreateIfMissing(true), path);
            var key = new byte[sizeof(int)];
            foreach (var row in rows)
            {
                // big-endian, so that the bytewise order of the keys is their numeric order
                BinaryPrimitives.WriteInt32BigEndian(key, row.Id);
                db.Put(key, MessagePackSerializer.Serialize(row));
            }
            // out of the memtable and into the table files, which is where a database that is only read has its rows
            db.Flush(new FlushOptions().SetWaitForFlush(true));
        }

        public RocksDbEngine(string path)
        {
            db = RocksDb.OpenReadOnly(new DbOptions(), path, false);
        }

        public TestDoc Find(int id)
        {
            BinaryPrimitives.WriteInt32BigEndian(keyBuffer, id);
            return MessagePackSerializer.Deserialize<TestDoc>(db.Get(keyBuffer));
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
