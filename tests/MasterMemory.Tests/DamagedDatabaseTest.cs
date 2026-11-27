using MasterMemory.Tests.Fuzz.Target;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FuzzDatabase = MasterMemory.Tests.Fuzz.Target.MemoryDatabase;

namespace MasterMemory.Tests
{
    // A database binary that is not what was built: cut short, damaged, or another file altogether. Loading it ends in
    // an InvalidOperationException (the header, where a table is, what a table is) or a MessagePackSerializationException
    // (the bytes of a table), and never in an exception from somewhere inside, nor in an allocation of the size a
    // damaged length field asks for.
    //
    // The first four cases are what fuzzing the load found (tests/MasterMemory.Tests.Fuzz, target db/load); each used to
    // surface as ArgumentOutOfRangeException, NullReferenceException or ArgumentException.
    public class DamagedDatabaseTest
    {
        // a database of one table, by hand: the header {name: [offset, count]} and then the bytes of the table
        static byte[] Database(string tableName, int offset, int count, params byte[] table)
        {
            var bytes = new List<byte> { 0x81 };
            bytes.AddRange(Entry(tableName, offset, count));
            bytes.AddRange(table);
            return bytes.ToArray();
        }

        static IEnumerable<byte> Entry(string tableName, int offset, int count)
        {
            var name = System.Text.Encoding.UTF8.GetBytes(tableName);
            var bytes = new List<byte> { (byte)(0xA0 | name.Length) };
            bytes.AddRange(name);
            bytes.Add(0x92);
            bytes.AddRange(Int32(offset));
            bytes.AddRange(Int32(count));
            return bytes;
        }

        static byte[] Int32(int value) => new byte[] { 0xD2, (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };

        static InvalidOperationException Rejected(byte[] database)
        {
            return Should.Throw<InvalidOperationException>(() => new FuzzDatabase(database));
        }

        [Fact]
        public void TableOutsideTheData()
        {
            // a table of 80 bytes, in a binary that ends after 3
            Rejected(Database("fuzz_long", 0, 80, 0x90, 0x90, 0x90)).Message.ShouldContain("cut short or damaged");
            Rejected(Database("fuzz_long", 5, 1, 0x90)).Message.ShouldContain("fuzz_long");
            Rejected(Database("fuzz_long", -1, 1, 0x90));
            Rejected(Database("fuzz_long", 0, -1, 0x90));
            Rejected(Database("fuzz_long", int.MaxValue, int.MaxValue, 0x90));

            // the same check where only the layout is read
            Should.Throw<InvalidOperationException>(() => MemoryDatabaseBase.GetTableInfo(Database("fuzz_long", 0, 80, 0x90)));

            // and a table that is where the header says loads
            new FuzzDatabase(Database("fuzz_long", 0, 1, 0x90)).LongKeyRowTable.Count.ShouldBe(0);
        }

        [Fact]
        public void NilWhereTheRowsShouldBe()
        {
            Rejected(Database("fuzz_long", 0, 1, 0xC0)).Message.ShouldContain("is nil where its rows should be");
        }

        [Fact]
        public void NilWhereARowShouldBe()
        {
            // [ {Id: 1, No: 2}, nil ]
            var table = new byte[] { 0x92, 0x82, 0xA2, (byte)'I', (byte)'d', 0x01, 0xA2, (byte)'N', (byte)'o', 0x02, 0xC0 };
            Rejected(Database("fuzz_long", 0, table.Length, table)).Message.ShouldContain("has nil where its row 1 should be");
        }

        [Fact]
        public void TableListedTwice()
        {
            var bytes = new List<byte> { 0x82 };
            bytes.AddRange(Entry("fuzz_long", 0, 1));
            bytes.AddRange(Entry("fuzz_long", 0, 1));
            bytes.Add(0x90);

            Rejected(bytes.ToArray()).Message.ShouldContain("listed twice");
        }

        // ---- lengths that are not what they say ----

        static long Allocated(Action action)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            action();
            return GC.GetAllocatedBytesForCurrentThread() - before;
        }

        [Fact]
        public void HeaderCountIsNotAnAllocation()
        {
            // a map of 2,147,483,647 tables, and then nothing
            var database = new byte[] { 0xDF, 0x7F, 0xFF, 0xFF, 0xFF };
            Allocated(() => Should.Throw<Exception>(() => new FuzzDatabase(database)).ShouldSatisfy(DatabaseFuzzTargets.IsRejection)).ShouldBeLessThan(1_000_000);
        }

        [Fact]
        public void DeclaredLengthOfACompressedTableIsNotAnAllocation()
        {
            // Compression is what the application plugs in: here the LZ4 processor of MessagePack.LZ4, with its default
            // cap of 64 MB on what a message may say it decompresses to.
            var lz4 = new MemoryDatabaseOptions { MessageProcessor = new Lz4BlockProcessor() };

            // 32 rows, written as an Lz4Block: ext 99 holding the length of the rows as an int32 and then the block
            var database = new FuzzDatabase(DatabaseFuzzTargets.SeedCorpus().Last()).ToDatabaseBuilder(lz4).Build();
            var at = -1;
            for (var i = 0; i + 5 < database.Length; i++)
            {
                if (database[i] == 99 && database[i + 1] == 0xD2) { at = i + 2; break; }
            }
            at.ShouldBeGreaterThan(0, "the seed should hold a compressed table");

            foreach (var length in new[] { int.MaxValue, 0x7FFFFFF0, 0x40000000, 1 << 24, 0, -1, int.MinValue })
            {
                var damaged = (byte[])database.Clone();
                damaged[at] = (byte)(length >> 24); damaged[at + 1] = (byte)(length >> 16); damaged[at + 2] = (byte)(length >> 8); damaged[at + 3] = (byte)length;

                Exception? thrown = null;
                var allocated = Allocated(() => thrown = Record.Exception(() => new FuzzDatabase(damaged, lz4)));
                thrown.ShouldNotBeNull($"a table that says it is {length} bytes when it is not");
                DatabaseFuzzTargets.IsRejection(thrown!).ShouldBeTrue($"{length}: {thrown!.GetType().Name}: {thrown.Message}");
                allocated.ShouldBeLessThan(64_000_000, $"a declared length of {length}");
            }
        }

        // ---- cut short, at every length ----

        [Fact]
        public void CutShortAnywhere()
        {
            foreach (var database in DatabaseFuzzTargets.SeedCorpus())
            {
                for (var length = 0; length < database.Length; length++)
                {
                    var cut = database.AsSpan(0, length).ToArray();
                    var thrown = Record.Exception(() => new FuzzDatabase(cut));
                    if (thrown != null)
                    {
                        DatabaseFuzzTargets.IsRejection(thrown).ShouldBeTrue($"cut at {length} of {database.Length}: {thrown.GetType().Name}: {thrown.Message}");
                    }
                }

                // whole, it loads
                new FuzzDatabase(database);
            }
        }
    }

    static class ExceptionAssertions
    {
        public static void ShouldSatisfy(this Exception exception, Func<Exception, bool> predicate)
        {
            predicate(exception).ShouldBeTrue($"{exception.GetType().Name}: {exception.Message}");
        }
    }
}
