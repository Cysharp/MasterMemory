using System.IO;
using Benchmark.Engines;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    /// <summary>
    /// Opening a database that is in a file and reading the first row from it: what an application pays at startup.
    /// It is the other side of <see cref="LookupBenchmark"/>. MasterMemory reads the whole file and makes an object of
    /// every row before the first lookup; an engine that reads pages on demand opens at once and pays at each lookup.
    /// The operating system has the files in its cache, so this is the cost of the engines, not of the disk.
    /// </summary>
    [Config(typeof(BenchmarkConfig))]
    public class LoadBenchmark
    {
        [Params(100, 10_000, 100_000)]
        public int Rows;

        string directory;
        string masterMemoryPath;
        string dryDbPath;
        string rocksDbPath;
        string sqlitePath;
        string liteDbPath;
        int key;

        [GlobalSetup]
        public void Setup()
        {
            var rows = TestData.Create(Rows);
            key = rows[rows.Length / 2].Id;

            directory = TempDirectory.Create();
            masterMemoryPath = Path.Combine(directory, "bench.mastermemory");
            dryDbPath = Path.Combine(directory, "bench.drydb");
            rocksDbPath = Path.Combine(directory, "bench.rocksdb");
            sqlitePath = Path.Combine(directory, "bench.sqlite");
            liteDbPath = Path.Combine(directory, "bench.litedb");

            try
            {
                File.WriteAllBytes(masterMemoryPath, new DatabaseBuilder().Append(rows).Build());
                DryDbEngine.Build(dryDbPath, rows);
                RocksDbEngine.Build(rocksDbPath, rows);
                SqliteEngine.Build(sqlitePath, rows);
                LiteDbEngine.Build(liteDbPath, rows);

                var expected = rows[rows.Length / 2];
                TestData.VerifyRow(expected, LoadMasterMemory());
                TestData.VerifyRow(expected, LoadDryDb());
                TestData.VerifyRow(expected, LoadRocksDb());
                TestData.VerifyRow(expected, LoadSqlite());
                TestData.VerifyRow(expected, LoadLiteDb());
            }
            catch
            {
                // BenchmarkDotNet does not call the cleanup after a setup that threw
                Cleanup();
                throw;
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            TempDirectory.Delete(directory);
        }

        [Benchmark(Description = "MasterMemory", Baseline = true)]
        public TestDoc LoadMasterMemory()
        {
            var database = new MemoryDatabase(File.ReadAllBytes(masterMemoryPath));
            return database.TestDocTable.FindById(key);
        }

        [Benchmark(Description = "DryDB")]
        public TestDoc LoadDryDb()
        {
            using var engine = new DryDbEngine(dryDbPath);
            return engine.Find(key);
        }

        [Benchmark(Description = "RocksDB")]
        public TestDoc LoadRocksDb()
        {
            using var engine = new RocksDbEngine(rocksDbPath);
            return engine.Find(key);
        }

        [Benchmark(Description = "SQLite")]
        public TestDoc LoadSqlite()
        {
            using var engine = SqliteEngine.OpenFile(sqlitePath, immutable: true);
            return engine.Find(key);
        }

        [Benchmark(Description = "LiteDB")]
        public TestDoc LoadLiteDb()
        {
            using var engine = LiteDbEngine.OpenFile(liteDbPath);
            return engine.Find(key);
        }
    }
}
