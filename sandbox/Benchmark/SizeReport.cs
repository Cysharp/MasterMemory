using System;
using System.IO;
using Benchmark.Engines;
using MasterMemory;
using MessagePack;

namespace Benchmark
{
    /// <summary>
    /// The size of what each engine stores for the same rows, as a Markdown table.
    /// MasterMemory writes a table as it is unless it is given a processor: "MasterMemory" is that, the default, and
    /// "+ LZ4" is the same database built with the LZ4 processor of MessagePack.LZ4. The other engines are built with
    /// their defaults, DryDB and SQLite without compression. "MessagePack" is the rows serialized one after another:
    /// the size of the data itself, which a MasterMemory database adds a header to.
    /// </summary>
    static class SizeReport
    {
        public static void Run()
        {
            var lz4 = new MemoryDatabaseOptions { MessageProcessor = new Lz4BlockProcessor(int.MaxValue) };

            Console.WriteLine("| Rows | MessagePack | MasterMemory | + LZ4 | DryDB | SQLite | LiteDB | RocksDB |");
            Console.WriteLine("|-----:|------------:|-------------:|------:|------:|-------:|-------:|--------:|");

            foreach (var count in new[] { 100, 10_000, 100_000 })
            {
                var rows = TestData.Create(count);
                var directory = TempDirectory.Create();
                try
                {
                    var messagePack = 0L;
                    foreach (var row in rows)
                    {
                        messagePack += MessagePackSerializer.Serialize(row).Length;
                    }

                    var masterMemory = new DatabaseBuilder().Append(rows).Build().LongLength;
                    var masterMemoryLz4 = new DatabaseBuilder(lz4).Append(rows).Build().LongLength;

                    var dryDb = Path.Combine(directory, "bench.drydb");
                    DryDbEngine.Build(dryDb, rows);

                    var sqlite = Path.Combine(directory, "bench.sqlite");
                    SqliteEngine.Build(sqlite, rows);

                    // a directory of its own: LiteDB keeps a log file beside the database
                    var liteDb = Directory.CreateDirectory(Path.Combine(directory, "litedb")).FullName;
                    LiteDbEngine.Build(Path.Combine(liteDb, "bench.litedb"), rows);

                    var rocksDb = Path.Combine(directory, "bench.rocksdb");
                    RocksDbEngine.Build(rocksDb, rows);

                    Console.WriteLine($"| {count:N0} | {Kb(messagePack)} | {Kb(masterMemory)} | {Kb(masterMemoryLz4)} | {Kb(TempDirectory.SizeOf(dryDb))} | {Kb(TempDirectory.SizeOf(sqlite))} | {Kb(TempDirectory.SizeOf(liteDb))} | {Kb(TempDirectory.SizeOf(rocksDb))} |");
                }
                finally
                {
                    TempDirectory.Delete(directory);
                }
            }
        }

        static string Kb(long bytes)
        {
            return (bytes / 1024.0).ToString("N0") + " KB";
        }
    }
}
