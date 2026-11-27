using MasterMemory.Internal;
using MasterMemory.Tests.Fuzz.Target;
using MasterMemory.Tests.Fuzz.Target.Tables;
using MasterMemory.Tests.TestStructures;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FuzzBuilder = MasterMemory.Tests.Fuzz.Target.DatabaseBuilder;
using FuzzDatabase = MasterMemory.Tests.Fuzz.Target.MemoryDatabase;

namespace MasterMemory.Tests
{
    // The order of rows that share a key. The key says nothing about it, so the sort decides: rows that share the key
    // of a secondary index are in the order of the primary key, and rows that share a primary key are in the order
    // they were given to the builder. Array.Sort, which made these orders before, left both to chance.
    // The oracle is Enumerable.OrderBy, which keeps elements of one key in the order they came in.
    public class IndexOrderTest
    {
        // ---- the sort ----

        struct IntOrder : IComparer<int>
        {
            public int Compare(int x, int y) => (x < y) ? -1 : (x > y) ? 1 : 0;
        }

        sealed class Contradictory : IComparer<int>
        {
            readonly Random random;
            public Contradictory(Random random) => this.random = random;
            public int Compare(int x, int y) => random.Next(-1, 2);
        }

        // the positions as an array of their own; null when the sort found the keys in order
        static int[] Positions<TComparer>(int[] keys, TComparer comparer, ref int[] buffer)
            where TComparer : IComparer<int>
        {
            var first = StableSort.Positions(keys, comparer, ref buffer);
            if (first < 0) return null;

            buffer.Length.ShouldBeGreaterThanOrEqualTo(keys.Length * 2);
            return buffer.Skip(first).Take(keys.Length).ToArray();
        }

        // One buffer for every sort of a test, as the indexes of a table have one: it comes to each sort with what the
        // one before left in it, and longer than needed after a longer sort.
        int[] sharedBuffer;

        void CheckPositions(int[] keys, string context)
        {
            var identity = Enumerable.Range(0, keys.Length).ToArray();
            var expected = identity.OrderBy(i => keys[i]).ToArray();
            var inOrder = expected.SequenceEqual(identity);

            // a comparer that is a struct, and one that is an object: the same code, compiled twice
            int[] ownBuffer = null;
            var before = sharedBuffer;
            var byStruct = Positions(keys, default(IntOrder), ref sharedBuffer);
            var byObject = Positions(keys, (IComparer<int>)Comparer<int>.Default, ref ownBuffer);
            if (inOrder)
            {
                byStruct.ShouldBeNull(context);
                byObject.ShouldBeNull(context);
                sharedBuffer.ShouldBeSameAs(before, context); // nothing to sort: no buffer made
                ownBuffer.ShouldBeNull(context);
            }
            else
            {
                byStruct.ShouldBe(expected, context);
                byObject.ShouldBe(expected, context);
                ownBuffer.Length.ShouldBe(keys.Length * 2, context);
                if (before != null && before.Length >= keys.Length * 2) sharedBuffer.ShouldBeSameAs(before, context); // long enough: used again
            }

            var rows = keys.Select((key, i) => (key, i)).ToArray();
            var sorted = StableSort.Sort(rows, keys, default(IntOrder), ref sharedBuffer);
            sorted.ShouldBe(expected.Select(i => rows[i]).ToArray(), context);
            if (rows.Length != 0) sorted.ShouldNotBeSameAs(rows, context);
        }

        // every length around the sizes the sort changes its way at (a run is 16 keys, merged two by two), with keys
        // from all equal to all different
        [Fact]
        public void PositionsAreThoseOfAStableSort()
        {
            var random = new Random(20261122);
            var lengths = Enumerable.Range(0, 70).Concat(new[] { 95, 96, 97, 127, 128, 129, 255, 256, 257, 1000, 4097 });
            foreach (var length in lengths)
            {
                foreach (var distinct in new[] { 1, 2, 3, Math.Max(1, length / 4), Math.Max(1, length), int.MaxValue })
                {
                    var keys = new int[length];
                    for (var i = 0; i < keys.Length; i++) keys[i] = random.Next(distinct);
                    CheckPositions(keys, $"random, length {length}, {distinct} distinct");
                }

                var ascending = Enumerable.Range(0, length).ToArray();
                CheckPositions(ascending, $"ascending, length {length}");
                CheckPositions(ascending.Reverse().ToArray(), $"descending, length {length}");
                CheckPositions(ascending.Select(x => x / 3).Reverse().ToArray(), $"descending in threes, length {length}");
                CheckPositions(ascending.Select(x => x % 16).ToArray(), $"runs in order, length {length}");
                CheckPositions(ascending.Select(x => x % 40).ToArray(), $"saw of 40, length {length}");
                CheckPositions(ascending.Select(x => (x == length - 1) ? -1 : x).ToArray(), $"the last one first, length {length}");
                CheckPositions(ascending.Select(x => (x == 0) ? int.MaxValue : x).ToArray(), $"the first one last, length {length}");
            }
        }

        // A comparison that contradicts itself (a NaN under a hand-written comparer, a culture that changed between two
        // calls) gives the keys in some order: every position once, and no exception.
        [Fact]
        public void AContradictoryComparison()
        {
            var random = new Random(3);
            foreach (var length in new[] { 0, 1, 2, 16, 17, 100, 1000 })
            {
                for (var round = 0; round < 20; round++)
                {
                    var keys = new int[length];
                    int[] buffer = null;
                    var positions = Positions(keys, new Contradictory(random), ref buffer) ?? Enumerable.Range(0, length).ToArray();
                    positions.OrderBy(x => x).ToArray().ShouldBe(Enumerable.Range(0, length).ToArray());
                }
            }
        }

        // ---- a secondary index ----

        static readonly string[] Names = { null, "", "a", "A", "b", "ab", "name" };
        static readonly double[] Scores = { double.NaN, double.NegativeInfinity, -1.5, -0.0, 0.0, 2.25, double.PositiveInfinity };

        static FuzzRow[] FuzzRows(int count, Random random)
        {
            var rows = new FuzzRow[count];
            for (var i = 0; i < rows.Length; i++)
            {
                rows[i] = new FuzzRow
                {
                    Id = i * 3,
                    Serial = (count - i) * 1_000_003L, // the other way round from Id
                    Group = random.Next(0, 4),
                    Seq = i, // in the order of Id
                    Bucket = random.Next(0, 3),
                    Name = Names[random.Next(Names.Length)],
                    Kind = (FuzzKind)random.Next(0, 4),
                    Code = "Code" + random.Next(0, Math.Max(1, count / 2)), // shared, though the key is declared unique
                    Score = Scores[random.Next(Scores.Length)],
                };
            }

            // given to the builder in no order
            for (var i = rows.Length - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (rows[i], rows[j]) = (rows[j], rows[i]);
            }
            return rows;
        }

        static void CheckIndex<TKey>(string name, FuzzRowTable table, RangeView<FuzzRow> index, Func<FuzzRow, TKey> key, IComparer<TKey> comparer)
        {
            var expected = table.All.ToArray().OrderBy(key, comparer).Select(x => x.Id).ToArray();
            index.ToArray().Select(x => x.Id).ShouldBe(expected, name);
        }

        static void CheckAscending(RangeView<FuzzRow> found, string name)
        {
            for (var i = 1; i < found.Count; i++)
            {
                found[i - 1].Id.ShouldBeLessThan(found[i].Id, name);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(33)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(5000)]
        public void RowsThatShareAKeyAreInTheOrderOfThePrimaryKey(int count)
        {
            var rows = FuzzRows(count, new Random(count));
            var builder = new FuzzBuilder().Append(rows);

            // a database made of the rows, and one read from the database binary
            foreach (var database in new[] { builder.ToMemoryDatabase(), new FuzzDatabase(builder.Build()) })
            {
                var table = database.FuzzRowTable;
                table.All.AsReadOnlyList().Select(x => x.Id).ToArray().ShouldBe(Enumerable.Range(0, count).Select(i => i * 3).ToArray());

                CheckIndex("Serial", table, table.SortBySerial, x => x.Serial, Comparer<long>.Default);
                CheckIndex("Group", table, table.SortByGroup, x => x.Group, Comparer<int>.Default);
                CheckIndex("(Group, Seq)", table, table.SortByGroupAndSeq, x => (x.Group, x.Seq), Comparer<(int, int)>.Default);
                CheckIndex("Name", table, table.SortByName, x => x.Name, StringComparer.Ordinal);
                CheckIndex("(Name, Kind)", table, table.SortByNameAndKind, x => (x.Name, x.Kind), OrdinalTupleComparer.Create<string, FuzzKind>());
                CheckIndex("Kind", table, table.SortByKind, x => x.Kind, Comparer<FuzzKind>.Default);
                CheckIndex("Code", table, table.SortByCode, x => x.Code, StringComparer.OrdinalIgnoreCase);
                CheckIndex("Score", table, table.SortByScore, x => x.Score, Comparer<double>.Default);
                CheckIndex("(Bucket, Kind, Seq)", table, table.SortByBucketAndKindAndSeq, x => (x.Bucket, x.Kind, x.Seq), Comparer<(int, FuzzKind, int)>.Default);

                // and so what a lookup of a shared key returns
                for (var group = 0; group < 4; group++)
                {
                    CheckAscending(table.FindByGroup(group), "Group " + group);
                    CheckAscending(table.FindByKind((FuzzKind)group), "Kind " + group);
                    foreach (var name in Names)
                    {
                        CheckAscending(table.FindByNameAndKind(name, (FuzzKind)group), $"({name}, {group})");
                    }
                }
                foreach (var name in Names) CheckAscending(table.FindByName(name), "Name " + name);
                // 0.0 and -0.0 are one key
                foreach (var score in Scores) CheckAscending(table.FindByScore(score), "Score " + score);
                if (count >= 100) table.FindByScore(0.0).AsReadOnlyList().Select(x => x.Score).Distinct().Count().ShouldBe(1); // equal, whatever the sign
                if (count >= 100) table.FindByScore(0.0).AsReadOnlyList().Count(x => double.IsNegative(x.Score)).ShouldBeGreaterThan(0);

                // a range goes key by key, and within a key by the primary key
                var range = table.FindRangeByGroup(1, 2).ToArray();
                range.Select(x => x.Id).ToArray().ShouldBe(range.OrderBy(x => x.Group).ThenBy(x => x.Id).Select(x => x.Id).ToArray());
            }
        }

        // ---- a primary key that rows share ----

        static void CheckGivenOrder(MemoryDatabase database, TestMaster[] given, string context)
        {
            var table = database.TestMasterTable;
            table.All.AsReadOnlyList().Select(x => (x.TestID, x.Value)).ToArray().ShouldBe(given.OrderBy(x => x.TestID).Select(x => (x.TestID, x.Value)).ToArray(), context);

            foreach (var id in given.Select(x => x.TestID).Distinct())
            {
                table.FindByTestID(id).AsReadOnlyList().Select(x => x.Value).ToArray().ShouldBe(given.Where(x => x.TestID == id).Select(x => x.Value).ToArray(), context);
            }
        }

        [Theory]
        [InlineData(2)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(300)]
        [InlineData(3000)]
        public void RowsThatShareAPrimaryKeyAreInTheOrderTheyWereGiven(int count)
        {
            var random = new Random(count);
            var rows = Enumerable.Range(0, count).Select(i => new TestMaster(random.Next(0, 1 + count / 8), i)).ToArray();

            // an array, and a sequence that is not a collection
            foreach (var source in new IEnumerable<TestMaster>[] { rows, rows.Where(x => true) })
            {
                var builder = new DatabaseBuilder().Append(source);
                CheckGivenOrder(builder.ToMemoryDatabase(), rows, "Append");
                CheckGivenOrder(new MemoryDatabase(builder.Build()), rows, "Append, read back");

                var reversed = rows.Reverse().ToArray();
                builder.ReplaceAll(reversed);
                CheckGivenOrder(builder.ToMemoryDatabase(), reversed, "ReplaceAll");
                CheckGivenOrder(new MemoryDatabase(builder.Build()), reversed, "ReplaceAll, read back");
            }

            // rows that come in the order of the key already are kept as they are, in an array of the builder's own
            var inOrder = rows.OrderBy(x => x.TestID).ToArray();
            var table = new DatabaseBuilder().Append(inOrder).ToMemoryDatabase().TestMasterTable;
            table.All.ToArray().ShouldBe(inOrder);
            table.GetRawDataUnsafe().ShouldNotBeSameAs(inOrder);
        }
    }
}
