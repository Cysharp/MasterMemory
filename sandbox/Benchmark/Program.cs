using BenchmarkDotNet.Attributes;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using TestPerfLiteDB;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Enyim.Caching.Memcached.Transcoders;
using RocksDbSharp;
using MessagePack;
using System.Text;

namespace Benchmark
{

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }

    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            // run quickly:)
            var baseConfig = Job.ShortRun.WithIterationCount(1).WithWarmupCount(1);

            // Add(baseConfig.With(Runtime.Clr).With(Jit.RyuJit).With(Platform.X64));
            Add(baseConfig.With(Runtime.Core).With(Jit.RyuJit).With(Platform.X64));
            // Add(baseConfig.With(InProcessEmitToolchain.Instance));

            Add(MarkdownExporter.GitHub);
            Add(CsvExporter.Default);
            Add(MemoryDiagnoser.Default);
        }
    }

    [Config(typeof(BenchmarkConfig))]
    public class SimpleRun
    {
        MemoryDatabase db;
        SQLite_Test sqliteMemory;
        SQLite_Test sqliteFile;

        LiteDB_Test defaultLiteDb;
        LiteDB_Test inmemoryLiteDb;

        LiteDB_Test2 liteDb2;

        MemcachedClient localMemcached;
        Dictionary<int, TestDoc> dictionary;

        RocksDb rocksDb;

        const int QueryId = 741;

        public SimpleRun()
        {
            var bin = new DatabaseBuilder().Append(MakeDoc(5000)).Build();
            db = new MemoryDatabase(bin);

            sqliteMemory = new SQLite_Test(5000, null, false, true);
            sqliteMemory.Prepare(); sqliteMemory.Insert(); sqliteMemory.CreateIndex();
            sqliteFile = new SQLite_Test(5000, null, false, false);
            sqliteFile.Prepare(); sqliteFile.Insert(); sqliteFile.CreateIndex();


            defaultLiteDb = new LiteDB_Test(5000, null, new LiteDB.FileOptions { Journal = true, FileMode = LiteDB.FileMode.Shared });
            defaultLiteDb.Prepare(); defaultLiteDb.Insert(); defaultLiteDb.CreateIndex();
            inmemoryLiteDb = new LiteDB_Test(5000);
            inmemoryLiteDb.Prepare(); inmemoryLiteDb.Insert(); inmemoryLiteDb.CreateIndex();

            liteDb2 = new LiteDB_Test2(5000);
            liteDb2.Prepare(); liteDb2.Insert(); liteDb2.CreateIndex();

            dictionary = new Dictionary<int, TestDoc>();
            foreach (var item in MakeDoc(5000))
            {
                dictionary.Add(item.id, item);

            }

            {
                var options = new DbOptions().SetCreateIfMissing(true);
                var tempPath = Guid.NewGuid() + ".bin";
                rocksDb = RocksDb.Open(options, tempPath);
                foreach (var item in MakeDoc(5000))
                {
                    rocksDb.Put(Encoding.UTF8.GetBytes("testdata." + item.id), MessagePackSerializer.Serialize(item));
                }
            }


            var config = new MemcachedClientConfiguration(new LoggerDummy(), new Dummy());
            localMemcached = new MemcachedClient(new LoggerDummy(), config);
            foreach (var item in MakeDoc(5000))
            {
                localMemcached.Add("testdoc2." + item.id, item, 9999);
            }

        }

        public IEnumerable<TestDoc> MakeDoc(int count)
        {
            foreach (var doc in Helper.GetDocs(count))
            {
                var v = new TestDoc
                {
                    id = (int)doc["_id"],
                    name = (string)doc["name"],
                    lorem = (string)doc["lorem"]
                };

                yield return v;
            }
        }

        [Benchmark(Baseline = true)]
        public TestDoc MasterMemoryQuery()
        {
            return db.TestDocTable.FindByid(QueryId);
        }

        [Benchmark]
        public TestDoc SQLiteInMemoryQuery()
        {
            using (var cmd = new SQLiteCommand("SELECT * FROM col WHERE id = @id", sqliteMemory._db))
            {
                cmd.Parameters.Add(new SQLiteParameter("id", DbType.Int32));
                cmd.Parameters["id"].Value = QueryId;

                using (var r = cmd.ExecuteReader())
                {
                    r.Read();
                    var id = r.GetInt32(0);
                    var name = r.GetString(1);
                    var lorem = r.GetString(2);
                    return new TestDoc { id = 1, name = name, lorem = lorem };
                }
            }
        }

        [Benchmark]
        public TestDoc SQLiteFileQuery()
        {
            using (var cmd = new SQLiteCommand("SELECT * FROM col WHERE id = @id", sqliteFile._db))
            {
                cmd.Parameters.Add(new SQLiteParameter("id", DbType.Int32));
                cmd.Parameters["id"].Value = QueryId;

                using (var r = cmd.ExecuteReader())
                {
                    r.Read();
                    var id = r.GetInt32(0);
                    var name = r.GetString(1);
                    var lorem = r.GetString(2);
                    return new TestDoc { id = 1, name = name, lorem = lorem };
                }
            }
        }



        [Benchmark]
        public BsonDocument LiteDbDefaultQuery()
        {
            return defaultLiteDb._db.FindOne("col", LiteDB.Query.EQ("_id", QueryId));
        }

        [Benchmark]
        public BsonDocument LiteDbInMemoryQuery()
        {
            return inmemoryLiteDb._db.FindOne("col", LiteDB.Query.EQ("_id", QueryId));
        }

        [Benchmark]
        public object LocalMemcachedQuery()
        {
            return localMemcached.Get("testdoc2." + QueryId);
        }

        //[Benchmark]
        //public TestDoc DictionaryQuery()
        //{
        //    return dictionary.TryGetValue(QueryId, out var r) ? r : null;
        //}

        [Benchmark]
        public TestDoc RocksDbQuery()
        {
            return MessagePackSerializer.Deserialize<TestDoc>(rocksDb.Get(Encoding.UTF8.GetBytes("testdata." + QueryId)));
        }
    }


    public class SQLite_Test
    {
        private string _filename;
        public SQLiteConnection _db;
        private int _count;

        public int Count { get { return _count; } }

        public SQLite_Test(int count, string password, bool journal, bool memory = false)
        {
            _count = count;
            _filename = "sqlite-" + Guid.NewGuid().ToString("n") + ".db";

            if (memory)
            {
                var cs = "Data Source=:memory:;New=True;";
                _db = new SQLiteConnection(cs);
            }
            else
            {
                var cs = "Data Source=" + _filename;
                if (password != null) cs += "; Password=" + password;
                if (journal == false) cs += "; Journal Mode=Off";
                _db = new SQLiteConnection(cs);
            }
        }

        public void Prepare()
        {
            _db.Open();

            var table = new SQLiteCommand("CREATE TABLE col (id INTEGER NOT NULL PRIMARY KEY, name TEXT, lorem TEXT)", _db);
            table.ExecuteNonQuery();

            var table2 = new SQLiteCommand("CREATE TABLE col_bulk (id INTEGER NOT NULL PRIMARY KEY, name TEXT, lorem TEXT)", _db);
            table2.ExecuteNonQuery();
        }

        public void Insert()
        {
            // standard insert is slow, mod same as Bulk
            using (var trans = _db.BeginTransaction())
            {
                var cmd = new SQLiteCommand("INSERT INTO col (id, name, lorem) VALUES (@id, @name, @lorem)", _db);

                cmd.Parameters.Add(new SQLiteParameter("id", DbType.Int32));
                cmd.Parameters.Add(new SQLiteParameter("name", DbType.String));
                cmd.Parameters.Add(new SQLiteParameter("lorem", DbType.String));

                foreach (var doc in Helper.GetDocs(_count))
                {
                    cmd.Parameters["id"].Value = (int)doc["_id"];
                    cmd.Parameters["name"].Value = (string)doc["name"];
                    cmd.Parameters["lorem"].Value = (string)doc["lorem"];

                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
            }
        }

        public void CreateIndex()
        {
            var cmd = new SQLiteCommand("CREATE INDEX idx1 ON col (name)", _db);

            cmd.ExecuteNonQuery();
        }

        public void Query()
        {
            var cmd = new SQLiteCommand("SELECT * FROM col WHERE id = @id", _db);

            cmd.Parameters.Add(new SQLiteParameter("id", DbType.Int32));

            for (var i = 0; i < _count; i++)
            {
                cmd.Parameters["id"].Value = i;

                var r = cmd.ExecuteReader();

                r.Read();

                var name = r.GetString(1);
                var lorem = r.GetString(2);

                r.Close();
            }
        }


        public void Dispose()
        {
            _db.Dispose();
        }
    }


    public class LiteDB_Test
    {
        private string _filename;
        public LiteEngine _db;
        private int _count;

        public int Count { get { return _count; } }

        public LiteDB_Test(int count, string password, LiteDB.FileOptions options)
        {
            _count = count;
            _filename = "dblite-" + Guid.NewGuid().ToString("n") + ".db";

            var disk = new FileDiskService(_filename, options);

            _db = new LiteEngine(disk, password);
        }

        public LiteDB_Test(int count)
        {
            _count = count;
            _filename = "dblite-" + Guid.NewGuid().ToString("n") + ".db";

            var ms = new MemoryStream();
            var disk = new LiteDB.StreamDiskService(ms);
            //var disk = new FileDiskService(_filename, options);

            _db = new LiteEngine(disk);
        }

        public void Prepare()
        {
        }

        public void Insert()
        {
            foreach (var doc in Helper.GetDocs(_count))
            {
                _db.Insert("col", doc);
            }
        }

        public void Bulk()
        {
            _db.Insert("col_bulk", Helper.GetDocs(_count));
        }

        public void Update()
        {
            foreach (var doc in Helper.GetDocs(_count))
            {
                _db.Update("col", doc);
            }
        }

        public void CreateIndex()
        {
            _db.EnsureIndex("col", "name", false);
        }

        public void Query()
        {
            for (var i = 0; i < _count; i++)
            {
                _db.Find("col", LiteDB.Query.EQ("_id", i)).Single();
            }
        }

        public void Delete()
        {
            _db.Delete("col", LiteDB.Query.All());
        }

        public void Drop()
        {
            _db.DropCollection("col_bulk");
        }

        public void Dispose()
        {
            _db.Dispose();
            File.Delete(_filename);
        }
    }


    public class LiteDB_Test2
    {
        private string _filename;
        public LiteDatabase _db;
        private int _count;
        public LiteCollection<TestDoc> _collection;

        public int Count { get { return _count; } }

        public LiteDB_Test2(int count, string password, LiteDB.FileOptions options)
        {
            _count = count;
            _filename = "dblite-" + Guid.NewGuid().ToString("n") + ".db";

            var disk = new FileDiskService(_filename, options);

            _db = new LiteDatabase(disk);

            _collection = _db.GetCollection<TestDoc>();
        }

        public LiteDB_Test2(int count)
        {
            _count = count;
            _filename = "dblite-" + Guid.NewGuid().ToString("n") + ".db";

            var ms = new MemoryStream();
            var disk = new LiteDB.StreamDiskService(ms);
            //var disk = new FileDiskService(_filename, options);

            _db = new LiteDatabase(disk);

            _collection = _db.GetCollection<TestDoc>();
        }

        public void Prepare()
        {
        }

        public void Insert()
        {
            foreach (var doc in Helper.GetDocs(_count))
            {
                _collection.Insert(new TestDoc
                {
                    //  id = (int)doc["_id"],
                    name = (string)doc["name"],
                    lorem = (string)doc["lorem"]
                });
            }
        }

        public void CreateIndex()
        {
            //_collection.EnsureIndex(x => x.id, true);
        }
    }


    class Dummy : IOptions<MemcachedClientOptions>
    {
        public MemcachedClientOptions Value => new MemcachedClientOptions
        {
            Servers = new List<Server> { new Server { Address = "127.0.0.1", Port = 11211 } },
            Protocol = MemcachedProtocol.Binary,
            // Transcoder = new BinaryFormatterTranscoder()
        };
    }

    class LoggerDummy : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new NullLogger();
        }

        public void Dispose()
        {

        }
        class NullLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
            {
                return new EmptyDisposable();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
            }

            class EmptyDisposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}
