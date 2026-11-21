using MasterMemory.Tests.TestStructures;
using System;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    // A lookup allocates nothing: no delegate call that boxes, no comparer object, no tuple on the heap, and the range
    // it returns is a struct. Measured here on .NET.
    // The allocation this was written after is one .NET does not have: under Mono and IL2CPP (Unity) the default
    // comparer of an enum boxes both values on every comparison. That cannot be measured here; what stands in for it
    // is TableReadingTest.EnumKeysDoNotUseTheDefaultComparer in the generator's tests, which requires that generated
    // code never asks for that comparer.
    public class LookupAllocationTest
    {
        static OtherKeyTypesMaster[] CreateData()
        {
            return Enumerable.Range(0, 64).Select(i => new OtherKeyTypesMaster
            {
                Id = new Guid(i + 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
                Score = i,
                Level = (i % 5 == 0) ? null : i % 7,
                At = new DateTime(2020, 1, 1).AddDays(i),
                Rank = (short)(i % 9),
                Flag = i % 2 == 0,
                Letter = (char)('0' + i),
                Price = i,
                Kind = (SmallKind)(i % 3),
                MaybeKind = (i % 4 == 0) ? null : (SmallKind)(i % 3),
                No = i,
            }).ToArray();
        }

        static long Measure(Action action)
        {
            for (var i = 0; i < 200; i++) action(); // past the first calls, which the runtime may allocate for
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 1000; i++) action();
            return GC.GetAllocatedBytesForCurrentThread() - before;
        }

        [Fact]
        public void EnumKeys()
        {
            var table = new MemoryDatabase(new DatabaseBuilder().Append(CreateData()).Build()).OtherKeyTypesMasterTable;
            var sum = 0;

            Measure(() => sum += table.FindByKind(SmallKind.B).Count).ShouldBe(0);
            Measure(() => sum += table.FindClosestByKind((SmallKind)9, selectLower: true).Count).ShouldBe(0);
            Measure(() => sum += table.FindRangeByKind(SmallKind.A, SmallKind.B).Count).ShouldBe(0);

            // the lookup of the article: a composite key that starts with an enum, by range
            Measure(() => sum += table.FindRangeByKindAndRank((SmallKind.B, short.MinValue), (SmallKind.B, short.MaxValue)).Count).ShouldBe(0);
            Measure(() => sum += table.FindByKindAndRank((SmallKind.C, 2)).Count).ShouldBe(0);

            Measure(() => sum += table.FindByMaybeKind(null).Count).ShouldBe(0);
            Measure(() => sum += table.FindByMaybeKind(SmallKind.A).Count).ShouldBe(0);
            Measure(() => sum += table.FindRangeByMaybeKindAndRank((null, 0), (SmallKind.C, 8)).Count).ShouldBe(0);

            sum.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void OtherKeys()
        {
            var table = new MemoryDatabase(new DatabaseBuilder().Append(CreateData()).Build()).OtherKeyTypesMasterTable;
            var id = new Guid(10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var at = new DateTime(2020, 1, 20);
            var sum = 0;

            Measure(() => sum += table.FindById(id).No).ShouldBe(0);
            Measure(() => sum += table.TryFindByScore(12, out var row) ? row.No : 0).ShouldBe(0);
            Measure(() => sum += table.FindByLevel(3).Count).ShouldBe(0);
            Measure(() => sum += table.FindClosestByAt(at)!.No).ShouldBe(0);
            Measure(() => sum += table.FindRangeByRank(2, 5).Count).ShouldBe(0);
            Measure(() => sum += table.FindByLetter('5').No).ShouldBe(0);
            Measure(() => sum += table.FindByPrice(7).No).ShouldBe(0);

            // enumerating a range does not allocate either
            Measure(() => { foreach (var row in table.FindRangeByRank(2, 5)) sum += row.No; }).ShouldBe(0);

            sum.ShouldBeGreaterThan(0);
        }
    }
}
