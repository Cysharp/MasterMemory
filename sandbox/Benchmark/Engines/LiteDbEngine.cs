using System;
using System.IO;
using LiteDB;

namespace Benchmark.Engines
{
    /// <summary>LiteDB with a typed collection: a lookup returns the row mapped to its class.</summary>
    public sealed class LiteDbEngine : IDisposable
    {
        const string CollectionName = "col";

        readonly LiteDatabase database;
        readonly ILiteCollection<TestDoc> collection;

        LiteDbEngine(LiteDatabase database)
        {
            this.database = database;
            collection = database.GetCollection<TestDoc>(CollectionName);
        }

        public static void Build(string path, TestDoc[] rows)
        {
            using var database = new LiteDatabase(new ConnectionString { Filename = path });
            database.GetCollection<TestDoc>(CollectionName).InsertBulk(rows);
        }

        public static LiteDbEngine OpenFile(string path)
        {
            return new LiteDbEngine(new LiteDatabase(new ConnectionString { Filename = path, ReadOnly = true }));
        }

        public static LiteDbEngine CreateInMemory(TestDoc[] rows)
        {
            var database = new LiteDatabase(new MemoryStream());
            database.GetCollection<TestDoc>(CollectionName).InsertBulk(rows);
            return new LiteDbEngine(database);
        }

        public TestDoc Find(int id)
        {
            return collection.FindById(id);
        }

        public void Dispose()
        {
            database.Dispose();
        }
    }
}
