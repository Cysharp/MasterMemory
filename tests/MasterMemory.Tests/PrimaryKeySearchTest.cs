using MasterMemory.Tests.Tables;
using MasterMemory.Tests.TestStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    // A unique numeric primary key is looked up in one of three ways: at (key - first key) when the keys have no gap,
    // by a search that halves without jumping up to TableBase.BranchlessSearchMaxLength rows, by the jumping search
    // above that. Whatever the way, a lookup has to agree with the rows. The searches themselves are also tested on
    // their own, in BinarySearchTest.
    public class PrimaryKeySearchTest
    {
        const int BranchlessSearchMaxLength = 32768; // TableBase<T>.BranchlessSearchMaxLength, which is protected

        static IntKeyMasterTable Create(IEnumerable<int> ids)
        {
            var bin = new DatabaseBuilder().Append(ids.Select(x => new IntKeyMaster { Id = x, Value = unchecked(x * 31) })).Build();
            return new MemoryDatabase(bin).IntKeyMasterTable;
        }

        // every id is found, and every key in `misses` is not, through both Find and TryFind
        static void Verify(IntKeyMasterTable table, IEnumerable<int> ids, IEnumerable<int> misses)
        {
            foreach (var id in ids)
            {
                table.FindById(id).Id.ShouldBe(id);
                table.TryFindById(id, out var row).ShouldBeTrue();
                row.Value.ShouldBe(unchecked(id * 31));
            }
            foreach (var key in misses)
            {
                table.TryFindById(key, out _).ShouldBeFalse("key: " + key);
                Should.Throw<KeyNotFoundException>(() => table.FindById(key));
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(100)]
        [InlineData(1000)]
        public void WithoutGaps(int count)
        {
            var ids = Enumerable.Range(10, count).ToArray();
            Verify(Create(ids), ids, new[] { int.MinValue, -1, 0, 9, 10 + count, 10 + count + 1, int.MaxValue });
        }

        // every length around the powers of two, where a halving loop goes wrong first
        [Fact]
        public void WithGapsEveryLength()
        {
            for (var count = 1; count <= 70; count++)
            {
                var ids = Enumerable.Range(0, count).Select(x => x * 3 + 5).ToArray();
                var misses = Enumerable.Range(0, count * 3 + 10).Where(x => x < 5 || (x - 5) % 3 != 0 || (x - 5) / 3 >= count);
                Verify(Create(ids), ids, misses);
            }
        }

        [Fact]
        public void Empty()
        {
            var table = Create(Array.Empty<int>());
            Verify(table, Array.Empty<int>(), new[] { int.MinValue, -1, 0, 1, int.MaxValue });
        }

        [Fact]
        public void NegativeAndExtremeKeys()
        {
            // the distance from the first key to the last does not fit in an int
            var ids = new[] { int.MinValue, int.MinValue + 1, -5, 0, 7, int.MaxValue - 1, int.MaxValue };
            Verify(Create(ids), ids, new[] { int.MinValue + 2, -6, -4, -1, 1, 6, 8, int.MaxValue - 2 });

            var around = Enumerable.Range(-3, 7).ToArray(); // -3 .. 3, no gap
            Verify(Create(around), around, new[] { int.MinValue, -4, 4, int.MaxValue });
        }

        // Nothing stops a database from being built with a repeated primary key. Rows 1, 2, 2, 4 pass for a table
        // without gaps (last - first == count - 1), so the row a key lands on must be checked.
        [Fact]
        public void DuplicatedKeysThatLookGapless()
        {
            var table = Create(new[] { 1, 2, 2, 4 });

            table.FindById(1).Id.ShouldBe(1);
            table.FindById(2).Id.ShouldBe(2);
            table.FindById(4).Id.ShouldBe(4);
            table.TryFindById(3, out _).ShouldBeFalse();
            Should.Throw<KeyNotFoundException>(() => table.FindById(3));
            table.TryFindById(0, out _).ShouldBeFalse();
            table.TryFindById(5, out _).ShouldBeFalse();
        }

        // one row more than the halving search takes, so the jumping search runs
        [Theory]
        [InlineData(BranchlessSearchMaxLength)]
        [InlineData(BranchlessSearchMaxLength + 1)]
        public void AroundTheSwitch(int count)
        {
            var table = Create(Enumerable.Range(0, count).Select(x => x * 2));

            table.Count.ShouldBe(count);
            var random = new Random(1);
            for (var i = 0; i < 20000; i++)
            {
                var index = random.Next(count);
                table.FindById(index * 2).Id.ShouldBe(index * 2);
                table.TryFindById(index * 2 + 1, out _).ShouldBeFalse();
            }
            table.FindById(0).Id.ShouldBe(0);
            table.FindById((count - 1) * 2).Id.ShouldBe((count - 1) * 2);
            table.TryFindById(-2, out _).ShouldBeFalse();
            table.TryFindById(count * 2, out _).ShouldBeFalse();
        }

        [Fact]
        public void OtherKeyTypes()
        {
            var builder = new DatabaseBuilder()
                // unsigned 32 bit: above int.MaxValue
                .Append(new[] { 0u, 1u, 5u, 3000000000u, uint.MaxValue }.Select(x => new UIntKeyMaster { Id = x, Value = 1 }))
                // 64 bit: no halving search, and a distance wider than 32 bits
                .Append(new[] { long.MinValue, -10000000000L, -1L, 0L, 3L, 10000000000L, long.MaxValue }.Select(x => new LongKeyMaster { Id = x, Value = 2 }))
                .Append(new[] { 0ul, 2ul, 10000000000ul, ulong.MaxValue }.Select(x => new ULongKeyMaster { Id = x, Value = 3 }))
                .Append(Enumerable.Range(0, 256).Select(x => new ByteKeyMaster { Id = (byte)x, Value = x }))   // every byte: gapless
                .Append(new sbyte[] { -128, -1, 0, 1, 127 }.Select(x => new SByteKeyMaster { Id = x, Value = 5 }));
            var db = new MemoryDatabase(builder.Build());

            foreach (var id in new[] { 0u, 1u, 5u, 3000000000u, uint.MaxValue }) db.UIntKeyMasterTable.FindById(id).Id.ShouldBe(id);
            foreach (var id in new[] { 2u, 4u, 6u, 2999999999u, uint.MaxValue - 1 }) db.UIntKeyMasterTable.TryFindById(id, out _).ShouldBeFalse();

            foreach (var id in new[] { long.MinValue, -10000000000L, -1L, 0L, 3L, 10000000000L, long.MaxValue }) db.LongKeyMasterTable.FindById(id).Id.ShouldBe(id);
            foreach (var id in new[] { long.MinValue + 1, -2L, 1L, 2L, 4L, long.MaxValue - 1 }) db.LongKeyMasterTable.TryFindById(id, out _).ShouldBeFalse();

            foreach (var id in new[] { 0ul, 2ul, 10000000000ul, ulong.MaxValue }) db.ULongKeyMasterTable.FindById(id).Id.ShouldBe(id);
            foreach (var id in new[] { 1ul, 3ul, ulong.MaxValue - 1 }) db.ULongKeyMasterTable.TryFindById(id, out _).ShouldBeFalse();

            for (var i = 0; i < 256; i++) db.ByteKeyMasterTable.FindById((byte)i).Value.ShouldBe(i);

            foreach (var id in new sbyte[] { -128, -1, 0, 1, 127 }) db.SByteKeyMasterTable.FindById(id).Id.ShouldBe(id);
            foreach (var id in new sbyte[] { -127, -2, 2, 126 }) db.SByteKeyMasterTable.TryFindById(id, out _).ShouldBeFalse();
        }

        [Fact]
        public void GaplessLongKeys()
        {
            var ids = Enumerable.Range(0, 50).Select(x => 10000000000L + x).ToArray();
            var db = new MemoryDatabase(new DatabaseBuilder().Append(ids.Select(x => new LongKeyMaster { Id = x, Value = 1 })).Build());

            foreach (var id in ids) db.LongKeyMasterTable.FindById(id).Id.ShouldBe(id);
            foreach (var id in new[] { long.MinValue, 0L, 9999999999L, 10000000050L, long.MaxValue }) db.LongKeyMasterTable.TryFindById(id, out _).ShouldBeFalse();
        }
    }
}
