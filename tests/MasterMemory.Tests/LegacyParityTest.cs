using MasterMemory.Internal;
using MasterMemory.Tests.Fuzz.Target;
using MasterMemory.Tests.Legacy;
using MasterMemory.Tests.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    // Every lookup used to go through a key selector delegate and an IComparer (Legacy/LegacySearch.cs keeps that code).
    // It now goes through a struct the generator declares per key. These tests ask both the same questions and require
    // the same answer: the same rows in the same order, or the same exception.
    //
    // Three answers of the old code were wrong and are deliberately not reproduced (FindSpecificationTest has what is
    // right, worked out without either implementation):
    //   - FindClosestBy of a unique key, past the last row, asking for the upper neighbour: IndexOutOfRangeException
    //     instead of null
    //   - FindRangeBy of a shared key: "min is after max" was decided with Comparer<TKey>.Default instead of the order
    //     of the index, so an ordinal range such as "B" .. "a" came back empty
    //   - UpperBoundClosest of an empty array: array.Length instead of -1
    public class LegacyParityTest
    {
        readonly struct IntKey : IComparable<int>
        {
            readonly int key;
            public IntKey(int key) => this.key = key;
            public int CompareTo(int other) => key.CompareTo(other);
        }

        // the searches themselves, on arrays with repeated values of every length up to 40
        [Fact]
        public void Searches()
        {
            var random = new Random(12345);
            Func<int, int> selector = x => x;
            var comparer = Comparer<int>.Default;

            for (var length = 0; length <= 40; length++)
            {
                for (var repeat = 0; repeat < 20; repeat++)
                {
                    var array = Enumerable.Range(0, length).Select(_ => random.Next(0, 18)).OrderBy(x => x).ToArray();

                    for (var key = -2; key <= 20; key++)
                    {
                        var context = $"length {length}, key {key}, [{string.Join(",", array)}]";

                        BinarySearch.Find(array, new IntKey(key)).ShouldBe(ToFindResult(LegacyBinarySearch.FindFirst(array, key, selector, comparer), array, key), context);
                        BinarySearch.FindClosest(array, new IntKey(key), true).ShouldBe(LegacyBinarySearch.FindClosest(array, 0, array.Length, key, selector, comparer, true), context);
                        BinarySearch.FindClosest(array, new IntKey(key), false).ShouldBe(LegacyBinarySearch.FindClosest(array, 0, array.Length, key, selector, comparer, false), context);
                        BinarySearch.LowerBound(array, new IntKey(key)).ShouldBe(LegacyBinarySearch.LowerBound(array, 0, array.Length, key, selector, comparer), context);
                        BinarySearch.UpperBound(array, new IntKey(key)).ShouldBe(LegacyBinarySearch.UpperBound(array, 0, array.Length, key, selector, comparer), context);
                        BinarySearch.LowerBoundClosest(array, new IntKey(key)).ShouldBe(LegacyBinarySearch.LowerBoundClosest(array, 0, array.Length, key, selector, comparer), context);
                        BinarySearch.UpperBoundClosest(array, new IntKey(key)).ShouldBe((array.Length == 0) ? -1 : LegacyBinarySearch.UpperBoundClosest(array, 0, array.Length, key, selector, comparer), context);
                    }
                }
            }
        }

        // Find reports a miss as the complement of the insertion point, FindFirst as -1; a hit is the same index
        static int ToFindResult(int legacyIndex, int[] array, int key)
        {
            return (legacyIndex >= 0) ? legacyIndex : Array.BinarySearch(array, key);
        }

        // ---- whole tables: every key of Sample, which has unique and shared keys of one member and of several ----

        static Sample[] CreateData()
        {
            return new[]
            {
                new Sample { Id = 5, Age = 19, FirstName = "aaa", LastName = "foo" },
                new Sample { Id = 6, Age = 29, FirstName = "bbb", LastName = "foo" },
                new Sample { Id = 7, Age = 39, FirstName = "ccc", LastName = "foo" },
                new Sample { Id = 8, Age = 49, FirstName = "ddd", LastName = "foo" },
                new Sample { Id = 1, Age = 59, FirstName = "eee", LastName = "foo" },
                new Sample { Id = 2, Age = 89, FirstName = "aaa", LastName = "bar" },
                new Sample { Id = 3, Age = 79, FirstName = "be", LastName = "de" },
                new Sample { Id = 4, Age = 89, FirstName = "aaa", LastName = "tako" },
                new Sample { Id = 9, Age = 99, FirstName = "aaa", LastName = "ika" },
                new Sample { Id = 10, Age = 9, FirstName = "eee", LastName = "baz" },
                new Sample { Id = 12, Age = 9, FirstName = "Eee", LastName = "Baz" },   // case: ordinal and culture order differ
                new Sample { Id = 14, Age = 39, FirstName = "a_a", LastName = "f_o" },  // punctuation: likewise
                new Sample { Id = 999, Age = 10, FirstName = null, LastName = "abcde" },
            };
        }

        static readonly int[] Ids = { int.MinValue, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 998, 999, 1000, int.MaxValue };
        static readonly int[] Ages = { int.MinValue, 8, 9, 10, 11, 19, 29, 39, 40, 49, 59, 79, 89, 99, 100, int.MaxValue };
        static readonly string[] FirstNames = { null, "", "A", "Eee", "a", "a_a", "aaa", "aab", "bbb", "be", "ccc", "ddd", "eee", "eef", "zzz" };
        static readonly string[] LastNames = { null, "", "Baz", "abcde", "bar", "baz", "de", "f_o", "foo", "ika", "tako", "zzz" };

        delegate bool TryFind<TKey>(TKey key, out Sample result);

        // what a lookup answered: the rows it returned (by reference), null, or the exception it threw
        static object Capture(Func<object> lookup)
        {
            try
            {
                var result = lookup();
                return (result is RangeView<Sample> range) ? range.ToArray() : result;
            }
            catch (Exception ex)
            {
                return ex.GetType();
            }
        }

        static void ShouldAnswerTheSame(object actual, object expected, string context)
        {
            if (expected is Sample[] expectedRows)
            {
                var actualRows = actual.ShouldBeOfType<Sample[]>(context);
                actualRows.Length.ShouldBe(expectedRows.Length, context);
                for (var i = 0; i < expectedRows.Length; i++)
                {
                    ReferenceEquals(actualRows[i], expectedRows[i]).ShouldBeTrue(context + " at " + i);
                }
            }
            else
            {
                ReferenceEquals(actual, expected).ShouldBeTrue($"{context}: {actual ?? "null"} / expected {expected ?? "null"}");
            }
        }

        static void VerifyUnique<TKey>(string name, Sample[] index, Func<Sample, TKey> selector, IComparer<TKey> comparer, TKey[] probes,
            Func<TKey, Sample> find, TryFind<TKey> tryFind, Func<TKey, bool, Sample> closest, Func<TKey, TKey, bool, RangeView<Sample>> range)
        {
            foreach (var key in probes)
            {
                var context = $"{name} {key}";

                ShouldAnswerTheSame(Capture(() => find(key)), Capture(() => LegacyTable<Sample>.FindUniqueCore(index, selector, comparer, key, true)), "Find " + context);

                var found = tryFind(key, out var row);
                var expectedFound = LegacyTable<Sample>.TryFindUniqueCore(index, selector, comparer, key, out var expectedRow);
                found.ShouldBe(expectedFound, "TryFind " + context);
                ReferenceEquals(row, expectedRow).ShouldBeTrue("TryFind " + context);

                foreach (var selectLower in new[] { true, false })
                {
                    var expected = Capture(() => LegacyTable<Sample>.FindUniqueClosestCore(index, selector, comparer, key, selectLower));
                    if (ReferenceEquals(expected, typeof(IndexOutOfRangeException))) expected = null; // the first of the three

                    ShouldAnswerTheSame(Capture(() => closest(key, selectLower)), expected, $"FindClosest {context} lower:{selectLower}");
                }
            }

            VerifyRanges(name, probes, range, (min, max, ascendant) => LegacyTable<Sample>.FindUniqueRangeCore(index, selector, comparer, min, max, ascendant));
        }

        static void VerifyNonUnique<TKey>(string name, Sample[] index, Func<Sample, TKey> selector, IComparer<TKey> comparer, TKey[] probes,
            Func<TKey, RangeView<Sample>> find, Func<TKey, bool, RangeView<Sample>> closest, Func<TKey, TKey, bool, RangeView<Sample>> range)
        {
            foreach (var key in probes)
            {
                var context = $"{name} {key}";

                ShouldAnswerTheSame(Capture(() => find(key)), Capture(() => LegacyTable<Sample>.FindManyCore(index, selector, comparer, key)), "Find " + context);

                foreach (var selectLower in new[] { true, false })
                {
                    ShouldAnswerTheSame(
                        Capture(() => closest(key, selectLower)),
                        Capture(() => LegacyTable<Sample>.FindManyClosestCore(index, selector, comparer, key, selectLower)),
                        $"FindClosest {context} lower:{selectLower}");
                }
            }

            // the second of the three: where the two orders disagree on min and max, the old answer is not the reference
            VerifyRanges(name, probes, range, (min, max, ascendant) => LegacyTable<Sample>.FindManyRangeCore(index, selector, comparer, min, max, ascendant),
                skip: (min, max) => (Comparer<TKey>.Default.Compare(min, max) > 0) != (comparer.Compare(min, max) > 0));
        }

        static void VerifyRanges<TKey>(string name, TKey[] probes, Func<TKey, TKey, bool, RangeView<Sample>> range, Func<TKey, TKey, bool, RangeView<Sample>> expected, Func<TKey, TKey, bool> skip = null)
        {
            // every pair of a spread of the probes, both ways round, so that min after max is asked too
            var step = Math.Max(1, probes.Length / 40);
            var sample = probes.Where((_, i) => i % step == 0).ToArray();
            foreach (var min in sample)
            {
                foreach (var max in sample)
                {
                    if (skip != null && skip(min, max)) continue;

                    foreach (var ascendant in new[] { true, false })
                    {
                        ShouldAnswerTheSame(
                            Capture(() => range(min, max, ascendant)),
                            Capture(() => expected(min, max, ascendant)),
                            $"FindRange {name} {min} .. {max} ascendant:{ascendant}");
                    }
                }
            }
        }

        [Fact]
        public void Tables()
        {
            var table = new MemoryDatabase(new DatabaseBuilder().Append(CreateData()).Build(), internString: false).SampleTable;

            // unique, one integer: the primary key, here with gaps and so searched
            VerifyUnique("Id", table.GetRawDataUnsafe(), x => x.Id, Comparer<int>.Default, Ids,
                table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);

            // shared, one integer
            VerifyNonUnique("Age", table.SortByAge.ToArray(), x => x.Age, Comparer<int>.Default, Ages,
                table.FindByAge, table.FindClosestByAge, table.FindRangeByAge);

            // shared, one string: ordinal
            VerifyNonUnique("FirstName", table.SortByFirstName.ToArray(), x => x.FirstName, StringComparer.Ordinal, FirstNames,
                table.FindByFirstName, table.FindClosestByFirstName, table.FindRangeByFirstName);

            // unique, two integers
            var idAndAge = (from id in Ids from age in Ages select (id, age)).ToArray();
            VerifyUnique("(Id, Age)", table.SortByIdAndAge.ToArray(), x => (x.Id, x.Age), Comparer<(int Id, int Age)>.Default, idAndAge,
                k => table.FindByIdAndAge(k.Item1, k.Item2),
                ((int, int) k, out Sample row) => table.TryFindByIdAndAge(k.Item1, k.Item2, out row),
                (k, lower) => table.FindClosestByIdAndAge(k.Item1, k.Item2, lower),
                (min, max, ascendant) => table.FindRangeByIdAndAge(min.Item1, min.Item2, max.Item1, max.Item2, ascendant));

            // unique, two strings: the tuple's default comparison, which is not ordinal
            var names = (from first in FirstNames from last in LastNames select (first, last)).ToArray();
            VerifyUnique("(FirstName, LastName)", table.SortByFirstNameAndLastName.ToArray(), x => (x.FirstName, x.LastName), OrdinalTupleComparer.Create<string, string>(), names,
                k => table.FindByFirstNameAndLastName(k.Item1, k.Item2),
                ((string, string) k, out Sample row) => table.TryFindByFirstNameAndLastName(k.Item1, k.Item2, out row),
                (k, lower) => table.FindClosestByFirstNameAndLastName(k.Item1, k.Item2, lower),
                (min, max, ascendant) => table.FindRangeByFirstNameAndLastName(min.Item1, min.Item2, max.Item1, max.Item2, ascendant));

            // shared, a string and an integer
            var nameAndAge = (from first in FirstNames from age in Ages select (first, age)).ToArray();
            VerifyNonUnique("(FirstName, Age)", table.SortByFirstNameAndAge.ToArray(), x => (x.FirstName, x.Age), OrdinalTupleComparer.Create<string, int>(), nameAndAge,
                k => table.FindByFirstNameAndAge(k.Item1, k.Item2),
                (k, lower) => table.FindClosestByFirstNameAndAge(k.Item1, k.Item2, lower),
                (min, max, ascendant) => table.FindRangeByFirstNameAndAge(min.Item1, min.Item2, max.Item1, max.Item2, ascendant));

            // unique, three and four members
            var three = (from id in Ids.Where((_, i) => i % 2 == 0) from age in Ages.Where((_, i) => i % 2 == 0) from first in FirstNames select (id, age, first)).ToArray();
            VerifyUnique("(Id, Age, FirstName)", table.SortByIdAndAgeAndFirstName.ToArray(), x => (x.Id, x.Age, x.FirstName), OrdinalTupleComparer.Create<int, int, string>(), three,
                k => table.FindByIdAndAgeAndFirstName(k.Item1, k.Item2, k.Item3),
                ((int, int, string) k, out Sample row) => table.TryFindByIdAndAgeAndFirstName(k.Item1, k.Item2, k.Item3, out row),
                (k, lower) => table.FindClosestByIdAndAgeAndFirstName(k.Item1, k.Item2, k.Item3, lower),
                (min, max, ascendant) => table.FindRangeByIdAndAgeAndFirstName(min.Item1, min.Item2, min.Item3, max.Item1, max.Item2, max.Item3, ascendant));

            var four = (from row in CreateData() from last in LastNames.Where((_, i) => i % 3 == 0).Append(row.LastName) select (row.Id, row.Age, row.FirstName, last)).ToArray();
            VerifyUnique("(Id, Age, FirstName, LastName)", table.SortByIdAndAgeAndFirstNameAndLastName.ToArray(), x => (x.Id, x.Age, x.FirstName, x.LastName), OrdinalTupleComparer.Create<int, int, string, string>(), four,
                k => table.FindByIdAndAgeAndFirstNameAndLastName(k.Item1, k.Item2, k.Item3, k.Item4),
                ((int, int, string, string) k, out Sample row) => table.TryFindByIdAndAgeAndFirstNameAndLastName(k.Item1, k.Item2, k.Item3, k.Item4, out row),
                (k, lower) => table.FindClosestByIdAndAgeAndFirstNameAndLastName(k.Item1, k.Item2, k.Item3, k.Item4, lower),
                (min, max, ascendant) => table.FindRangeByIdAndAgeAndFirstNameAndLastName(min.Item1, min.Item2, min.Item3, min.Item4, max.Item1, max.Item2, max.Item3, max.Item4, ascendant));
        }

        // an enum key and a string key with a comparison option, which Sample has none of
        [Fact]
        public void EnumAndStringOptionKeys()
        {
            var db = new MemoryDatabase(new DatabaseBuilder()
                .Append(new[]
                {
                    new FileScoped.FileScopedMaster { Id = 1, Element = FileScoped.Element.Wind, Code = "c" },
                    new FileScoped.FileScopedMaster { Id = 2, Element = FileScoped.Element.Fire, Code = "a" },
                    new FileScoped.FileScopedMaster { Id = 3, Element = FileScoped.Element.Wind, Code = "b" },
                    new FileScoped.FileScopedMaster { Id = 4, Element = FileScoped.Element.Fire, Code = "d" },
                })
                .Append(new[]
                {
                    new TestStructures.NullableStringKeyMaster { Code = "a_b", Alias = "Alpha", Value = 1 },
                    new TestStructures.NullableStringKeyMaster { Code = "a1", Alias = "beta", Value = 2 },
                    new TestStructures.NullableStringKeyMaster { Code = "B", Alias = "GAMMA", Value = 3 },
                })
                .Build());

            var elements = db.FileScopedMasterTable;
            elements.FindByElement(FileScoped.Element.Fire).Select(x => x.Id).OrderBy(x => x).ToArray().ShouldBe(new[] { 2, 4 });
            elements.FindByElement(FileScoped.Element.Water).Count.ShouldBe(0);
            elements.FindByElement(FileScoped.Element.Wind).Select(x => x.Id).OrderBy(x => x).ToArray().ShouldBe(new[] { 1, 3 });
            elements.FindClosestByElement(FileScoped.Element.Water, selectLower: true).Select(x => x.Id).OrderBy(x => x).ToArray().ShouldBe(new[] { 2, 4 });
            elements.FindClosestByElement(FileScoped.Element.Water, selectLower: false).Select(x => x.Id).OrderBy(x => x).ToArray().ShouldBe(new[] { 1, 3 });
            elements.FindRangeByElement(FileScoped.Element.Fire, FileScoped.Element.Water).Count.ShouldBe(2);
            elements.FindRangeByElement(FileScoped.Element.Fire, FileScoped.Element.Wind).Count.ShouldBe(4);

            var aliases = db.NullableStringKeyMasterTable;
            aliases.FindByAlias("ALPHA").Value.ShouldBe(1);
            aliases.FindByAlias("Gamma").Value.ShouldBe(3);
            aliases.TryFindByAlias("delta", out _).ShouldBeFalse();
            aliases.FindClosestByAlias("BETZ", selectLower: true).Value.ShouldBe(2);
            aliases.FindClosestByAlias("BETZ", selectLower: false).Value.ShouldBe(3);
            aliases.FindRangeByAlias("ALPHA", "BETA").Select(x => x.Value).ToArray().ShouldBe(new[] { 1, 2 });
        }
    }
}
