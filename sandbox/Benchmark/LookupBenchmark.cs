using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Benchmark.Engines;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    /// <summary>
    /// A row by its primary key. Every engine holds the same rows and is asked for the same keys, and every method
    /// returns the row as a <see cref="TestDoc"/> (except the one that says "raw").
    ///
    /// MasterMemory and Dictionary hold the rows as objects and return the object they hold. The other engines hold
    /// bytes or columns and make the object for each lookup: that work is part of what they are measured for, since it
    /// is what a program that wants the row pays.
    /// </summary>
    [Config(typeof(BenchmarkConfig))]
    public class LookupBenchmark
    {
        // One invocation is this many lookups; the results are per lookup.
        const int Lookups = 1024;

        // The keys asked for: random ids of rows that exist. An invocation takes the next 1024 of them, so the sequence
        // comes round only after 64 invocations. A short sequence that repeats is learned by the branch predictor,
        // and one key asked for over and over is always in the cache: both flatter a search.
        const int QueryKeyCount = 1 << 16;

        // 100 rows fit in the L1 cache, 10,000 are a table of ordinary size, and the objects of 100,000 (about 70 MB)
        // do not fit in the L3 cache. Above 32,768 rows MasterMemory searches with conditional jumps.
        [Params(100, 10_000, 100_000)]
        public int Rows;

        TestDoc[] rows;
        int[] keys;
        int cursor;
        string directory;

        MemoryDatabase masterMemory;
        Dictionary<int, TestDoc> dictionary;
        DryDbEngine dryDb;
        RocksDbEngine rocksDb;
        SqliteEngine sqlite;
        LiteDbEngine liteDb;

        // ---- MasterMemory

        [GlobalSetup(Target = nameof(FindMasterMemory))]
        public void SetupMasterMemory()
        {
            Setup(dense: false, () =>
            {
                masterMemory = new MemoryDatabase(new DatabaseBuilder().Append(rows).Build());
                Verify(id => masterMemory.TestDocTable.FindById(id));
            });
        }

        // ids with gaps: the table is searched
        [Benchmark(Description = "MasterMemory", Baseline = true, OperationsPerInvoke = Lookups)]
        public long FindMasterMemory()
        {
            var table = masterMemory.TestDocTable;
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += table.FindById(key).Id;
            }
            return sum;
        }

        [GlobalSetup(Target = nameof(FindMasterMemoryDense))]
        public void SetupMasterMemoryDense()
        {
            Setup(dense: true, () =>
            {
                masterMemory = new MemoryDatabase(new DatabaseBuilder().Append(rows).Build());
                Verify(id => masterMemory.TestDocTable.FindById(id));
            });
        }

        // ids 1, 2, 3, ... without a gap: MasterMemory reads the row at its position and searches nothing.
        // The only method whose rows have other ids than the rest; Name and Lorem are the same.
        [Benchmark(Description = "MasterMemory (ids without a gap)", OperationsPerInvoke = Lookups)]
        public long FindMasterMemoryDense()
        {
            var table = masterMemory.TestDocTable;
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += table.FindById(key).Id;
            }
            return sum;
        }

        // ---- Dictionary<int, TestDoc>: what a lookup by a unique key costs with no database at all

        [GlobalSetup(Target = nameof(FindDictionary))]
        public void SetupDictionary()
        {
            Setup(dense: false, () =>
            {
                dictionary = rows.ToDictionary(x => x.Id);
                Verify(id => dictionary[id]);
            });
        }

        [Benchmark(Description = "Dictionary", OperationsPerInvoke = Lookups)]
        public long FindDictionary()
        {
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += dictionary[key].Id;
            }
            return sum;
        }

        // ---- DryDB

        [GlobalSetup(Targets = new[] { nameof(FindDryDb), nameof(FindDryDbRaw) })]
        public void SetupDryDb()
        {
            Setup(dense: false, () =>
            {
                var path = Path.Combine(directory, "bench.drydb");
                DryDbEngine.Build(path, rows);
                dryDb = new DryDbEngine(path);
                Verify(dryDb.Find);

                // every page into the page cache, so that the lookups measured are those of a database in use
                foreach (var row in rows)
                {
                    dryDb.FindRaw(row.Id);
                }
            });
        }

        [Benchmark(Description = "DryDB", OperationsPerInvoke = Lookups)]
        public long FindDryDb()
        {
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += dryDb.Find(key).Id;
            }
            return sum;
        }

        // the lookup without the deserialization: the bytes of the row, as DryDB itself returns them
        [Benchmark(Description = "DryDB (raw bytes)", OperationsPerInvoke = Lookups)]
        public long FindDryDbRaw()
        {
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += dryDb.FindRaw(key);
            }
            return sum;
        }

        // ---- RocksDB

        [GlobalSetup(Target = nameof(FindRocksDb))]
        public void SetupRocksDb()
        {
            Setup(dense: false, () =>
            {
                var path = Path.Combine(directory, "bench.rocksdb");
                RocksDbEngine.Build(path, rows);
                rocksDb = new RocksDbEngine(path);
                Verify(rocksDb.Find);
            });
        }

        [Benchmark(Description = "RocksDB", OperationsPerInvoke = Lookups)]
        public long FindRocksDb()
        {
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += rocksDb.Find(key).Id;
            }
            return sum;
        }

        // ---- SQLite

        [GlobalSetup(Targets = new[] { nameof(FindSqliteInMemory), nameof(FindSqliteInMemoryNaive) })]
        public void SetupSqliteInMemory()
        {
            Setup(dense: false, () =>
            {
                sqlite = SqliteEngine.CreateInMemory(rows);
                Verify(sqlite.Find);
                Verify(sqlite.FindNaive);
            });
        }

        [Benchmark(Description = "SQLite in memory (prepared)", OperationsPerInvoke = Lookups)]
        public long FindSqliteInMemory()
        {
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += sqlite.Find(key).Id;
            }
            return sum;
        }

        [Benchmark(Description = "SQLite in memory (command per query)", OperationsPerInvoke = Lookups)]
        public long FindSqliteInMemoryNaive()
        {
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += sqlite.FindNaive(key).Id;
            }
            return sum;
        }

        [GlobalSetup(Target = nameof(FindSqliteFile))]
        public void SetupSqliteFile()
        {
            Setup(dense: false, () =>
            {
                var path = Path.Combine(directory, "bench.sqlite");
                SqliteEngine.Build(path, rows);
                sqlite = SqliteEngine.OpenFile(path, immutable: true);
                Verify(sqlite.Find);
            });
        }

        // the most a file gives: nobody else writes it, and the statement is reused
        [Benchmark(Description = "SQLite file (prepared, immutable)", OperationsPerInvoke = Lookups)]
        public long FindSqliteFile()
        {
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += sqlite.Find(key).Id;
            }
            return sum;
        }

        [GlobalSetup(Target = nameof(FindSqliteFileNaive))]
        public void SetupSqliteFileNaive()
        {
            Setup(dense: false, () =>
            {
                var path = Path.Combine(directory, "bench.sqlite");
                SqliteEngine.Build(path, rows);
                sqlite = SqliteEngine.OpenFile(path, immutable: false);
                Verify(sqlite.FindNaive);
            });
        }

        // SQLite with nothing done for speed: the file opened as it is by default, and a command for every query
        [Benchmark(Description = "SQLite file (command per query)", OperationsPerInvoke = Lookups)]
        public long FindSqliteFileNaive()
        {
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += sqlite.FindNaive(key).Id;
            }
            return sum;
        }

        // ---- LiteDB

        [GlobalSetup(Target = nameof(FindLiteDbInMemory))]
        public void SetupLiteDbInMemory()
        {
            Setup(dense: false, () =>
            {
                liteDb = LiteDbEngine.CreateInMemory(rows);
                Verify(liteDb.Find);
            });
        }

        [Benchmark(Description = "LiteDB in memory", OperationsPerInvoke = Lookups)]
        public long FindLiteDbInMemory()
        {
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += liteDb.Find(key).Id;
            }
            return sum;
        }

        [GlobalSetup(Target = nameof(FindLiteDbFile))]
        public void SetupLiteDbFile()
        {
            Setup(dense: false, () =>
            {
                var path = Path.Combine(directory, "bench.litedb");
                LiteDbEngine.Build(path, rows);
                liteDb = LiteDbEngine.OpenFile(path);
                Verify(liteDb.Find);
            });
        }

        [Benchmark(Description = "LiteDB file", OperationsPerInvoke = Lookups)]
        public long FindLiteDbFile()
        {
            var sum = 0L;
            foreach (var key in NextKeys())
            {
                sum += liteDb.Find(key).Id;
            }
            return sum;
        }

        // ----

        [GlobalCleanup]
        public void Cleanup()
        {
            dryDb?.Dispose();
            rocksDb?.Dispose();
            sqlite?.Dispose();
            liteDb?.Dispose();
            TempDirectory.Delete(directory);
        }

        // The rows and the keys of this run, then the engine. BenchmarkDotNet does not call the cleanup after a setup
        // that threw, so a setup that fails takes its directory away itself.
        void Setup(bool dense, Action build)
        {
            rows = TestData.Create(Rows, dense);
            keys = TestData.CreateQueryKeys(rows, QueryKeyCount);
            cursor = 0;
            directory = TempDirectory.Create();
            try
            {
                build();
            }
            catch
            {
                Cleanup();
                throw;
            }
        }

        ReadOnlySpan<int> NextKeys()
        {
            var next = new ReadOnlySpan<int>(keys, cursor, Lookups);
            cursor = (cursor + Lookups) & (QueryKeyCount - 1);
            return next;
        }

        // An engine that returned another row, or the same row for every key, would still be fast.
        void Verify(Func<int, TestDoc> find)
        {
            TestData.Verify(rows, find);
        }
    }
}
