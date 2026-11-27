using MasterMemory.Tests.FileScoped;
using MasterMemory.Tests.Fuzz.Target;
using MasterMemory.Tests.TestStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    // What every lookup has to answer, worked out the slow way: a pass over all the rows of the index with the comparer
    // the index is sorted by. No binary search, and none of the library's code, is involved in the expected answer.
    //
    //   FindBy (unique)         the row whose key equals the key; KeyNotFoundException when there is none
    //   TryFindBy               the same row and true, or false
    //   FindBy (shared)         every row whose key equals the key, in index order; empty when there is none
    //   FindClosestBy (unique)  the row at the key; else the nearest row before it (selectLower) or after it; null when
    //                           there is no row on that side
    //   FindClosestBy (shared)  every row of the key found that way; empty when there is no row on that side
    //   FindRangeBy             every row with min <= key <= max, in index order or reversed; empty when min is after max
    public class FindSpecificationTest
    {
        readonly Xunit.Abstractions.ITestOutputHelper output;

        public FindSpecificationTest(Xunit.Abstractions.ITestOutputHelper output)
        {
            this.output = output;
        }

        delegate bool TryFind<TRow, TKey>(TKey key, out TRow result);

        sealed class Failures
        {
            readonly List<string> list = new List<string>();
            readonly Dictionary<string, int> counts = new Dictionary<string, int>();
            public int Checked;

            public void Add(string operation, string detail)
            {
                counts[operation] = counts.TryGetValue(operation, out var count) ? count + 1 : 1;
                if (counts[operation] <= 4) list.Add(operation + ": " + detail); // a few of each kind are enough to read
            }

            public void ShouldBeNone(Xunit.Abstractions.ITestOutputHelper output)
            {
                output.WriteLine($"{Checked} answers checked");
                foreach (var item in counts) output.WriteLine($"{item.Value} wrong: {item.Key}");
                foreach (var item in list) output.WriteLine(item);
                counts.Sum(x => x.Value).ShouldBe(0, string.Join(Environment.NewLine, counts.Select(x => $"{x.Value} wrong: {x.Key}").Concat(list)));
            }
        }

        // the rows a lookup returned (by reference), the single row, null, or the type of the exception it threw
        static object Capture<TRow>(Func<object> lookup)
        {
            try
            {
                var result = lookup();
                return (result is RangeView<TRow> range) ? range.ToArray()
                    : (result is ReverseRangeView<TRow> reverse) ? reverse.ToArray()
                    : result;
            }
            catch (Exception ex)
            {
                return ex.GetType();
            }
        }

        static string Describe<TRow>(object answer, Func<TRow, string> describe)
        {
            if (answer == null) return "null";
            if (answer is Type exception) return exception.Name;
            if (answer is TRow[] rows) return "[" + string.Join(" ", rows.Select(describe)) + "]";
            return describe((TRow)answer);
        }

        static bool Same<TRow>(object actual, object expected)
        {
            if (expected is TRow[] expectedRows)
            {
                return actual is TRow[] actualRows
                    && actualRows.Length == expectedRows.Length
                    && actualRows.Zip(expectedRows, (x, y) => ReferenceEquals(x, y)).All(x => x);
            }
            return ReferenceEquals(actual, expected);
        }

        static void Check<TRow>(Failures failures, string operation, string question, object actual, object expected, Func<TRow, string> describe)
        {
            failures.Checked++;
            if (!Same<TRow>(actual, expected))
            {
                failures.Add(operation, $"{question} -> {Describe(actual, describe)}, expected {Describe(expected, describe)}");
            }
        }

        static string Show<TKey>(TKey key) => (key == null) ? "null" : (key is string s) ? "\"" + s + "\"" : key.ToString();

        // the pairs FindRangeBy is asked: a spread of the probes against itself, so min after max is asked as well
        static IEnumerable<(TKey min, TKey max)> RangePairs<TKey>(TKey[] probes)
        {
            var step = Math.Max(1, probes.Length / 32);
            var spread = probes.Where((_, i) => i % step == 0).ToArray();
            return from min in spread from max in spread select (min, max);
        }

        static void VerifyUnique<TRow, TKey>(Failures failures, string name, TRow[] index, Func<TRow, TKey> selector, IComparer<TKey> comparer, TKey[] probes, Func<TRow, string> describe,
            Func<TKey, TRow> find, TryFind<TRow, TKey> tryFind, Func<TKey, bool, TRow> closest, Func<TKey, TKey, RangeView<TRow>> range)
            where TRow : class
        {
            int Compare(TRow row, TKey key) => comparer.Compare(selector(row), key);

            foreach (var key in probes)
            {
                var exact = index.SingleOrDefault(x => Compare(x, key) == 0);

                Check(failures, $"{name} FindBy", Show(key), Capture<TRow>(() => find(key)), (object)exact ?? typeof(KeyNotFoundException), describe);

                var found = tryFind(key, out var row);
                failures.Checked++;
                if (found != (exact != null) || !ReferenceEquals(row, exact))
                {
                    failures.Add($"{name} TryFindBy", $"{Show(key)} -> {found} {Describe<TRow>(row, describe)}, expected {exact != null} {Describe<TRow>(exact, describe)}");
                }

                var lower = exact ?? index.LastOrDefault(x => Compare(x, key) < 0);
                var upper = exact ?? index.FirstOrDefault(x => Compare(x, key) > 0);
                Check(failures, $"{name} FindClosestBy(lower)", Show(key), Capture<TRow>(() => closest(key, true)), lower, describe);
                Check(failures, $"{name} FindClosestBy(upper)", Show(key), Capture<TRow>(() => closest(key, false)), upper, describe);
            }

            VerifyRange(failures, name, index, selector, comparer, probes, describe, range);
        }

        static void VerifyShared<TRow, TKey>(Failures failures, string name, TRow[] index, Func<TRow, TKey> selector, IComparer<TKey> comparer, TKey[] probes, Func<TRow, string> describe,
            Func<TKey, RangeView<TRow>> find, Func<TKey, bool, RangeView<TRow>> closest, Func<TKey, TKey, RangeView<TRow>> range)
            where TRow : class
        {
            int Compare(TRow row, TKey key) => comparer.Compare(selector(row), key);
            TRow[] RowsOf(TKey key) => index.Where(x => Compare(x, key) == 0).ToArray();
            TRow[] RowsOfRow(TRow row) => (row == null) ? Array.Empty<TRow>() : RowsOf(selector(row));

            foreach (var key in probes)
            {
                var exact = RowsOf(key);
                Check(failures, $"{name} FindBy", Show(key), Capture<TRow>(() => find(key)), exact, describe);

                var lower = (exact.Length != 0) ? exact : RowsOfRow(index.LastOrDefault(x => Compare(x, key) < 0));
                var upper = (exact.Length != 0) ? exact : RowsOfRow(index.FirstOrDefault(x => Compare(x, key) > 0));
                Check(failures, $"{name} FindClosestBy(lower)", Show(key), Capture<TRow>(() => closest(key, true)), lower, describe);
                Check(failures, $"{name} FindClosestBy(upper)", Show(key), Capture<TRow>(() => closest(key, false)), upper, describe);
            }

            VerifyRange(failures, name, index, selector, comparer, probes, describe, range);
        }

        static void VerifyRange<TRow, TKey>(Failures failures, string name, TRow[] index, Func<TRow, TKey> selector, IComparer<TKey> comparer, TKey[] probes, Func<TRow, string> describe,
            Func<TKey, TKey, RangeView<TRow>> range)
            where TRow : class
        {
            foreach (var (min, max) in RangePairs(probes))
            {
                var expected = index.Where(x => comparer.Compare(selector(x), min) >= 0 && comparer.Compare(selector(x), max) <= 0).ToArray();
                Check(failures, $"{name} FindRangeBy", $"{Show(min)} .. {Show(max)}", Capture<TRow>(() => range(min, max)), expected, describe);
                Check(failures, $"{name} FindRangeBy, Reverse", $"{Show(min)} .. {Show(max)}", Capture<TRow>(() => range(min, max).Reverse), expected.Reverse().ToArray(), describe);
            }
        }

        // ---- Sample: one member and several, unique and shared, integers and strings ----

        static Sample[] SampleRows()
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

        static string Describe(Sample x) => $"#{x.Id}";

        static void VerifySample(Failures failures, string prefix, Sample[] rows)
        {
            VerifySampleTable(failures, prefix, new MemoryDatabase(new DatabaseBuilder().Append(rows).Build(), new MemoryDatabaseOptions { InternString = false }).SampleTable);
        }

        static void VerifySampleTable(Failures failures, string prefix, Tables.SampleTable table)
        {
            VerifyUnique(failures, prefix + "Id", table.All.ToArray(), x => x.Id, Comparer<int>.Default, Ids, Describe,
                table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);

            VerifyShared(failures, prefix + "Age", table.SortByAge.ToArray(), x => x.Age, Comparer<int>.Default, Ages, Describe,
                table.FindByAge, table.FindClosestByAge, table.FindRangeByAge);

            VerifyShared(failures, prefix + "FirstName", table.SortByFirstName.ToArray(), x => x.FirstName, StringComparer.Ordinal, FirstNames, Describe,
                table.FindByFirstName, table.FindClosestByFirstName, table.FindRangeByFirstName);

            var idAndAge = (from id in Ids from age in Ages select (id, age)).ToArray();
            VerifyUnique(failures, prefix + "(Id, Age)", table.SortByIdAndAge.ToArray(), x => (x.Id, x.Age), Comparer<(int, int)>.Default, idAndAge, Describe,
                k => table.FindByIdAndAge(k.Item1, k.Item2),
                ((int, int) k, out Sample row) => table.TryFindByIdAndAge(k.Item1, k.Item2, out row),
                (k, lower) => table.FindClosestByIdAndAge(k.Item1, k.Item2, lower),
                (min, max) => table.FindRangeByIdAndAge(min.Item1, min.Item2, max.Item1, max.Item2));

            var names = (from first in FirstNames from last in LastNames select (first, last)).ToArray();
            VerifyUnique(failures, prefix + "(FirstName, LastName)", table.SortByFirstNameAndLastName.ToArray(), x => (x.FirstName, x.LastName), OrdinalTupleComparer.Create<string, string>(), names, Describe,
                k => table.FindByFirstNameAndLastName(k.Item1, k.Item2),
                ((string, string) k, out Sample row) => table.TryFindByFirstNameAndLastName(k.Item1, k.Item2, out row),
                (k, lower) => table.FindClosestByFirstNameAndLastName(k.Item1, k.Item2, lower),
                (min, max) => table.FindRangeByFirstNameAndLastName(min.Item1, min.Item2, max.Item1, max.Item2));

            var nameAndAge = (from first in FirstNames from age in Ages select (first, age)).ToArray();
            VerifyShared(failures, prefix + "(FirstName, Age)", table.SortByFirstNameAndAge.ToArray(), x => (x.FirstName, x.Age), OrdinalTupleComparer.Create<string, int>(), nameAndAge, Describe,
                k => table.FindByFirstNameAndAge(k.Item1, k.Item2),
                (k, lower) => table.FindClosestByFirstNameAndAge(k.Item1, k.Item2, lower),
                (min, max) => table.FindRangeByFirstNameAndAge(min.Item1, min.Item2, max.Item1, max.Item2));

            var three = (from id in Ids.Where((_, i) => i % 2 == 0) from age in Ages.Where((_, i) => i % 2 == 0) from first in FirstNames select (id, age, first)).ToArray();
            VerifyUnique(failures, prefix + "(Id, Age, FirstName)", table.SortByIdAndAgeAndFirstName.ToArray(), x => (x.Id, x.Age, x.FirstName), OrdinalTupleComparer.Create<int, int, string>(), three, Describe,
                k => table.FindByIdAndAgeAndFirstName(k.Item1, k.Item2, k.Item3),
                ((int, int, string) k, out Sample row) => table.TryFindByIdAndAgeAndFirstName(k.Item1, k.Item2, k.Item3, out row),
                (k, lower) => table.FindClosestByIdAndAgeAndFirstName(k.Item1, k.Item2, k.Item3, lower),
                (min, max) => table.FindRangeByIdAndAgeAndFirstName(min.Item1, min.Item2, min.Item3, max.Item1, max.Item2, max.Item3));

            var four = (from row in SampleRows() from last in LastNames.Where((_, i) => i % 3 == 0).Append(row.LastName) select (row.Id, row.Age, row.FirstName, last)).ToArray();
            VerifyUnique(failures, prefix + "(Id, Age, FirstName, LastName)", table.SortByIdAndAgeAndFirstNameAndLastName.ToArray(), x => (x.Id, x.Age, x.FirstName, x.LastName), OrdinalTupleComparer.Create<int, int, string, string>(), four, Describe,
                k => table.FindByIdAndAgeAndFirstNameAndLastName(k.Item1, k.Item2, k.Item3, k.Item4),
                ((int, int, string, string) k, out Sample row) => table.TryFindByIdAndAgeAndFirstNameAndLastName(k.Item1, k.Item2, k.Item3, k.Item4, out row),
                (k, lower) => table.FindClosestByIdAndAgeAndFirstNameAndLastName(k.Item1, k.Item2, k.Item3, k.Item4, lower),
                (min, max) => table.FindRangeByIdAndAgeAndFirstNameAndLastName(min.Item1, min.Item2, min.Item3, min.Item4, max.Item1, max.Item2, max.Item3, max.Item4));
        }

        [Fact]
        public void EveryKeyOfSample()
        {
            var failures = new Failures();
            VerifySample(failures, "", SampleRows());
            failures.ShouldBeNone(output);
        }

        // a table with no row, with one, with two: where an off-by-one in a bound shows first
        [Fact]
        public void EmptyAndTinyTables()
        {
            var failures = new Failures();
            VerifySample(failures, "empty: ", Array.Empty<Sample>());
            VerifySample(failures, "one row: ", SampleRows().Take(1).ToArray());
            VerifySample(failures, "two rows: ", SampleRows().Skip(4).Take(2).ToArray());
            VerifySample(failures, "three rows: ", SampleRows().Skip(9).Take(3).ToArray());
            failures.ShouldBeNone(output);
        }

        // ---- integer primary keys: without gaps (read at their position), with gaps, 64 bits, every table size up to 40 ----

        [Fact]
        public void IntegerPrimaryKeys()
        {
            var failures = new Failures();

            for (var count = 0; count <= 40; count++)
            {
                foreach (var gap in new[] { 1, 3 })
                {
                    var ids = Enumerable.Range(0, count).Select(x => x * gap + 10).ToArray();
                    var probes = Enumerable.Range(5, count * gap + 12).Concat(new[] { int.MinValue, int.MaxValue }).ToArray();

                    var table = new MemoryDatabase(new DatabaseBuilder().Append(ids.Select(x => new IntKeyMaster { Id = x })).Build()).IntKeyMasterTable;
                    VerifyUnique(failures, $"int x{gap}", table.All.ToArray(), x => x.Id, Comparer<int>.Default, probes, x => "#" + x.Id,
                        table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);

                    var longIds = ids.Select(x => 10000000000L + x).ToArray();
                    var longProbes = probes.Take(probes.Length - 2).Select(x => 10000000000L + x).Concat(new[] { long.MinValue, 0L, long.MaxValue }).ToArray();
                    var longTable = new MemoryDatabase(new DatabaseBuilder().Append(longIds.Select(x => new LongKeyMaster { Id = x })).Build()).LongKeyMasterTable;
                    VerifyUnique(failures, $"long x{gap}", longTable.All.ToArray(), x => x.Id, Comparer<long>.Default, longProbes, x => "#" + x.Id,
                        longTable.FindById, longTable.TryFindById, longTable.FindClosestById, longTable.FindRangeById);
                }
            }

            failures.ShouldBeNone(output);
        }

        // ---- string keys: ordinal (a nullable one), with a comparison option, and an enum ----

        [Fact]
        public void StringAndEnumKeys()
        {
            var failures = new Failures();

            var rows = new[]
            {
                new NullableStringKeyMaster { Code = "a_b", Alias = "Alpha", Value = 1 },
                new NullableStringKeyMaster { Code = "a1", Alias = "beta", Value = 2 },
                new NullableStringKeyMaster { Code = "B", Alias = "GAMMA", Value = 3 },
                new NullableStringKeyMaster { Code = "a", Alias = "delta", Value = 4 },
                new NullableStringKeyMaster { Code = "b", Alias = "Epsilon", Value = 5 },
                new NullableStringKeyMaster { Code = "", Alias = "zeta", Value = 6 },
            };
            var probes = new[] { null, "", " ", "0", "A", "B", "C", "Z", "_", "a", "a1", "a2", "aZ", "a_", "a_b", "a_c", "aa", "b", "c", "~" };
            var aliasProbes = new[] { null, "", "ALPHA", "Alpha", "alpha", "alphb", "BETA", "beta", "betz", "DELTA", "delta", "EPSILON", "f", "GAMMA", "gamma", "h", "ZETA", "zeta", "zz" };

            var table = new MemoryDatabase(new DatabaseBuilder().Append(rows).Build(), new MemoryDatabaseOptions { InternString = false }).NullableStringKeyMasterTable;

            VerifyUnique(failures, "Code (ordinal)", table.All.ToArray(), x => x.Code, StringComparer.Ordinal, probes, x => "#" + x.Value,
                table.FindByCode, table.TryFindByCode, table.FindClosestByCode, table.FindRangeByCode);

            VerifyUnique(failures, "Alias (ordinal, ignore case)", table.SortByAlias.ToArray(), x => x.Alias, StringComparer.OrdinalIgnoreCase, aliasProbes, x => "#" + x.Value,
                table.FindByAlias, table.TryFindByAlias, table.FindClosestByAlias, table.FindRangeByAlias);

            var elementRows = new[]
            {
                new FileScopedMaster { Id = 1, Element = Element.Wind, Code = "c" },
                new FileScopedMaster { Id = 2, Element = Element.Fire, Code = "a" },
                new FileScopedMaster { Id = 3, Element = Element.Wind, Code = "b" },
                new FileScopedMaster { Id = 4, Element = Element.Fire, Code = "d" },
            };
            var elements = new[] { (Element)(-1), Element.Fire, Element.Water, Element.Wind, (Element)3, (Element)int.MaxValue };
            var elementTable = new MemoryDatabase(new DatabaseBuilder().Append(elementRows).Build(), new MemoryDatabaseOptions { InternString = false }).FileScopedMasterTable;

            VerifyShared(failures, "Element", elementTable.SortByElement.ToArray(), x => x.Element, Comparer<Element>.Default, elements, x => "#" + x.Id,
                elementTable.FindByElement, elementTable.FindClosestByElement, elementTable.FindRangeByElement);

            failures.ShouldBeNone(output);
        }

        // ---- every other key type: compared through Comparer<T>.Default, or, for an enum, by its underlying value ----

        [Fact]
        public void OtherKeyTypes()
        {
            var failures = new Failures();

            var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var scores = new[] { double.NaN, double.NegativeInfinity, -1.5, 0.0, 1.5, 2.5, 100.25, double.PositiveInfinity };
            var levels = new int?[] { null, null, 1, 1, 2, 5, 5, 9 };
            var ranks = new short[] { -3, -3, 0, 0, 7, 7, 7, short.MaxValue };
            var letters = new[] { 'a', 'B', 'z', '0', '~', 'é', 'A', 'b' };
            var prices = new[] { decimal.MinValue, -1.5m, 0m, 0.1m, 1m, 1.10m, 100m, decimal.MaxValue };
            var kinds = new[] { SmallKind.C, SmallKind.A, SmallKind.B, SmallKind.A, SmallKind.C, SmallKind.C, SmallKind.A, SmallKind.B };

            var rows = Enumerable.Range(0, 8).Select(i => new OtherKeyTypesMaster
            {
                Id = new Guid(i * 37 % 8 + 1, (short)i, 3, 4, 5, 6, 7, 8, 9, 10, 11),
                Score = scores[i],
                Level = levels[i],
                At = start.AddDays(i * 3).AddTicks(i),
                Rank = ranks[i],
                Flag = i % 3 == 0,
                Letter = letters[i],
                Price = prices[i],
                Kind = kinds[i],
                MaybeKind = (i % 3 == 1) ? null : kinds[(i + 3) % 8],
                No = i,
            }).ToArray();

            var table = new MemoryDatabase(new DatabaseBuilder().Append(rows).Build(), new MemoryDatabaseOptions { InternString = false }).OtherKeyTypesMasterTable;
            Func<OtherKeyTypesMaster, string> describe = x => "#" + x.No;

            var ids = rows.Select(x => x.Id).Concat(new[] { Guid.Empty, new Guid(4, 99, 3, 4, 5, 6, 7, 8, 9, 10, 11), new Guid(int.MaxValue, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) }).ToArray();
            VerifyUnique(failures, "Guid", table.All.ToArray(), x => x.Id, Comparer<Guid>.Default, ids, describe,
                table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);

            var scoreProbes = new[] { double.NaN, double.NegativeInfinity, double.MinValue, -2, -1.5, -0.0, 0, 1, 1.5, 2, 2.5, 50, 100.25, 1e300, double.MaxValue, double.PositiveInfinity };
            VerifyUnique(failures, "double", table.SortByScore.ToArray(), x => x.Score, Comparer<double>.Default, scoreProbes, describe,
                table.FindByScore, table.TryFindByScore, table.FindClosestByScore, table.FindRangeByScore);

            var levelProbes = new int?[] { null, int.MinValue, 0, 1, 2, 3, 5, 9, 10, int.MaxValue };
            VerifyShared(failures, "int?", table.SortByLevel.ToArray(), x => x.Level, Comparer<int?>.Default, levelProbes, describe,
                table.FindByLevel, table.FindClosestByLevel, table.FindRangeByLevel);

            var times = rows.Select(x => x.At).SelectMany(x => new[] { x.AddTicks(-1), x, x.AddTicks(1) }).Concat(new[] { DateTime.MinValue, DateTime.MaxValue }).ToArray();
            VerifyUnique(failures, "DateTime", table.SortByAt.ToArray(), x => x.At, Comparer<DateTime>.Default, times, describe,
                table.FindByAt, table.TryFindByAt, table.FindClosestByAt, table.FindRangeByAt);

            var rankProbes = new short[] { short.MinValue, -4, -3, -1, 0, 1, 7, 8, short.MaxValue };
            VerifyShared(failures, "short", table.SortByRank.ToArray(), x => x.Rank, Comparer<short>.Default, rankProbes, describe,
                table.FindByRank, table.FindClosestByRank, table.FindRangeByRank);

            VerifyShared(failures, "bool", table.SortByFlag.ToArray(), x => x.Flag, Comparer<bool>.Default, new[] { false, true }, describe,
                table.FindByFlag, table.FindClosestByFlag, table.FindRangeByFlag);

            var letterProbes = new[] { '\0', '0', '1', 'A', 'B', 'C', 'a', 'b', 'c', 'z', '~', 'é', 'ÿ', char.MaxValue };
            VerifyUnique(failures, "char", table.SortByLetter.ToArray(), x => x.Letter, Comparer<char>.Default, letterProbes, describe,
                table.FindByLetter, table.TryFindByLetter, table.FindClosestByLetter, table.FindRangeByLetter);

            var priceProbes = new[] { decimal.MinValue, -2m, -1.5m, 0m, 0.05m, 0.1m, 1m, 1.1m, 1.100m, 2m, 100m, 101m, decimal.MaxValue };
            VerifyUnique(failures, "decimal", table.SortByPrice.ToArray(), x => x.Price, Comparer<decimal>.Default, priceProbes, describe,
                table.FindByPrice, table.TryFindByPrice, table.FindClosestByPrice, table.FindRangeByPrice);

            var kindProbes = new[] { SmallKind.A, SmallKind.B, SmallKind.C, (SmallKind)3, (SmallKind)255 };
            VerifyShared(failures, "enum : byte", table.SortByKind.ToArray(), x => x.Kind, Comparer<SmallKind>.Default, kindProbes, describe,
                table.FindByKind, table.FindClosestByKind, table.FindRangeByKind);

            var kindAndRank = (from kind in kindProbes from rank in rankProbes select (kind, rank)).ToArray();
            VerifyShared(failures, "(enum, short)", table.SortByKindAndRank.ToArray(), x => (x.Kind, x.Rank), Comparer<(SmallKind, short)>.Default, kindAndRank, describe,
                k => table.FindByKindAndRank(k.Item1, k.Item2),
                (k, lower) => table.FindClosestByKindAndRank(k.Item1, k.Item2, lower),
                (min, max) => table.FindRangeByKindAndRank(min.Item1, min.Item2, max.Item1, max.Item2));

            var maybeKindProbes = new SmallKind?[] { null, SmallKind.A, SmallKind.B, SmallKind.C, (SmallKind)3, (SmallKind)255 };
            VerifyShared(failures, "enum?", table.SortByMaybeKind.ToArray(), x => x.MaybeKind, Comparer<SmallKind?>.Default, maybeKindProbes, describe,
                table.FindByMaybeKind, table.FindClosestByMaybeKind, table.FindRangeByMaybeKind);

            var maybeKindAndRank = (from kind in maybeKindProbes from rank in rankProbes select (kind, rank)).ToArray();
            VerifyShared(failures, "(enum?, short)", table.SortByMaybeKindAndRank.ToArray(), x => (x.MaybeKind, x.Rank), Comparer<(SmallKind?, short)>.Default, maybeKindAndRank, describe,
                k => table.FindByMaybeKindAndRank(k.Item1, k.Item2),
                (k, lower) => table.FindClosestByMaybeKindAndRank(k.Item1, k.Item2, lower),
                (min, max) => table.FindRangeByMaybeKindAndRank(min.Item1, min.Item2, max.Item1, max.Item2));

            failures.ShouldBeNone(output);
        }

        // ---- the head of a composite key: every row whose first members equal the key, in the order of the whole key ----

        static void VerifyHead<TRow, TKey>(Failures failures, string name, TRow[] index, Func<TRow, TKey> selector, IComparer<TKey> comparer, TKey[] probes, Func<TRow, string> describe,
            Func<TKey, RangeView<TRow>> find, Func<TKey, TKey, RangeView<TRow>> range)
            where TRow : class
        {
            foreach (var key in probes)
            {
                var expected = index.Where(x => comparer.Compare(selector(x), key) == 0).ToArray();
                Check(failures, $"{name} FindBy", Show(key), Capture<TRow>(() => find(key)), expected, describe);
            }

            VerifyRange(failures, name, index, selector, comparer, probes, describe, range);
        }

        static PrefixMaster[] PrefixRows(int count)
        {
            var subs = new[] { "a", "B", "b", "A", "", "c" };
            var names = new[] { "x", "X", "y", "", "xy" };
            return Enumerable.Range(0, count).Select(i => new PrefixMaster
            {
                Category = (i * 5 % 7 % 4) - 1,                 // -1 .. 2, several rows each
                Sub = subs[i % subs.Length],
                No = i,
                Kind = (SmallKind)(i % 3),
                Level = (i % 4 == 0) ? null : (i * 3 % 5),
                Name = names[i * 2 % names.Length],
                Serial = (i % 2 == 0) ? i * 10_000_000_000L : -i,
            }).ToArray();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(23)]
        public void HeadsOfCompositeKeys(int count)
        {
            var failures = new Failures();
            var rows = PrefixRows(count);
            var table = new MemoryDatabase(new DatabaseBuilder().Append(rows).Build(), new MemoryDatabaseOptions { InternString = false }).PrefixMasterTable;
            Func<PrefixMaster, string> describe = x => "#" + x.No;

            // the primary key (Category, Sub ignoring case, No)
            var categories = new[] { int.MinValue, -2, -1, 0, 1, 2, 3, int.MaxValue };
            VerifyHead(failures, $"[{count}] Category", table.All.ToArray(), x => x.Category, Comparer<int>.Default, categories, describe,
                table.FindByCategory, table.FindRangeByCategory);

            var categoryAndSub = Comparer<(int, string)>.Create((x, y) =>
            {
                var c = x.Item1.CompareTo(y.Item1);
                return (c != 0) ? c : StringComparer.OrdinalIgnoreCase.Compare(x.Item2, y.Item2);
            });
            var subProbes = (from category in new[] { -2, -1, 0, 2, 3 } from sub in new[] { "", "A", "a", "b", "C", "d" } select (category, sub)).ToArray();
            VerifyHead(failures, $"[{count}] (Category, Sub)", table.All.ToArray(), x => (x.Category, x.Sub), categoryAndSub, subProbes, describe,
                k => table.FindByCategoryAndSub(k.Item1, k.Item2),
                (min, max) => table.FindRangeByCategoryAndSub(min.Item1, min.Item2, max.Item1, max.Item2));

            // the shared key (Kind, Level)
            var kinds = new[] { SmallKind.A, SmallKind.B, SmallKind.C, (SmallKind)3, (SmallKind)255 };
            VerifyHead(failures, $"[{count}] Kind", table.SortByKindAndLevel.ToArray(), x => x.Kind, Comparer<SmallKind>.Default, kinds, describe,
                table.FindByKind, table.FindRangeByKind);

            // the unique key (Name, Serial): its head is still several rows
            var names = new[] { "", "X", "Xy", "x", "xy", "y", "z" };
            VerifyHead(failures, $"[{count}] Name", table.SortByNameAndSerial.ToArray(), x => x.Name, StringComparer.Ordinal, names, describe,
                table.FindByName, table.FindRangeByName);

            failures.ShouldBeNone(output);

            // what a head is for: the rows of one value, already in the order of the other members
            if (count == 23)
            {
                table.FindByKind(SmallKind.B).AsReadOnlyList().Select(x => x.Level).ToArray().ShouldBe(table.FindByKind(SmallKind.B).AsReadOnlyList().Select(x => x.Level).OrderBy(x => x).ToArray());
                table.FindByKind(SmallKind.B).Count.ShouldBe(rows.Count(x => x.Kind == SmallKind.B));
                table.FindByCategory(1).AsReadOnlyList().Select(x => x.No).OrderBy(x => x).ToArray().ShouldBe(rows.Where(x => x.Category == 1).Select(x => x.No).ToArray());
            }
        }

        // ---- a builder made from a database: the table it leaves is a table like any other, with every index rebuilt ----

        [Fact]
        public void TablesChangedThroughTheBuilder()
        {
            var failures = new Failures();
            var original = new MemoryDatabase(new DatabaseBuilder().Append(SampleRows()).Append(new[] { new UserLevel(1, 100), new UserLevel(2, 200) }).Build(), new MemoryDatabaseOptions { InternString = false });
            var originalIds = original.SampleTable.All.AsReadOnlyList().Select(x => x.Id).ToArray();

            MemoryDatabase Apply(Action<DatabaseBuilder> change)
            {
                var builder = original.ToDatabaseBuilder();
                change(builder);
                return builder.ToMemoryDatabase();
            }

            void ShouldHoldIds(MemoryDatabase database, string name, IEnumerable<int> expectedIds)
            {
                database.SampleTable.All.AsReadOnlyList().Select(x => x.Id).ToArray().ShouldBe(expectedIds.OrderBy(x => x).ToArray(), name);
                VerifySampleTable(failures, name + ": ", database.SampleTable);

                // the table that was not touched is shared, the database that was changed from is as it was
                ReferenceEquals(database.UserLevelTable, original.UserLevelTable).ShouldBeTrue(name);
                original.SampleTable.All.AsReadOnlyList().Select(x => x.Id).ToArray().ShouldBe(originalIds, name);
            }

            // Remove: rows that exist, a key that does not, a key given twice, no key at all, every key
            ShouldHoldIds(Apply(x => x.RemoveSample(new[] { 5, 999 })), "remove two", originalIds.Except(new[] { 5, 999 }));
            ShouldHoldIds(Apply(x => x.RemoveSample(new[] { 1 })), "remove the first", originalIds.Except(new[] { 1 }));
            ShouldHoldIds(Apply(x => x.RemoveSample(new[] { 12345, 7, -1 })), "remove with missing keys", originalIds.Except(new[] { 7 }));
            ShouldHoldIds(Apply(x => x.RemoveSample(new[] { 7, 7, 7 })), "remove one key three times", originalIds.Except(new[] { 7 }));
            ShouldHoldIds(Apply(x => x.RemoveSample(Array.Empty<int>())), "remove nothing", originalIds);
            ShouldHoldIds(Apply(x => x.RemoveSample(originalIds)), "remove everything", Array.Empty<int>());

            // AddOrReplace: a new row, a replaced row, both, none
            var added = new Sample { Id = 11, Age = 49, FirstName = "new", LastName = "row" };
            var replaced = new Sample { Id = 7, Age = 1, FirstName = "zzz", LastName = "changed" };

            var afterAdd = Apply(x => x.AddOrReplace(new[] { added }));
            ShouldHoldIds(afterAdd, "add or replace: add", originalIds.Append(11));
            afterAdd.SampleTable.FindById(11).FirstName.ShouldBe("new");

            var afterReplace = Apply(x => x.AddOrReplace(new[] { replaced }));
            ShouldHoldIds(afterReplace, "add or replace: replace", originalIds);
            afterReplace.SampleTable.FindById(7).LastName.ShouldBe("changed");
            afterReplace.SampleTable.FindByAge(39).AsReadOnlyList().Select(x => x.Id).ToArray().ShouldBe(new[] { 14 }); // 7 left its old age
            afterReplace.SampleTable.FindByAge(1).AsReadOnlyList().Select(x => x.Id).ToArray().ShouldBe(new[] { 7 });
            original.SampleTable.FindById(7).LastName.ShouldBe("foo");

            ShouldHoldIds(Apply(x => x.AddOrReplace(new[] { replaced, added })), "add or replace: replace and add", originalIds.Append(11));
            ShouldHoldIds(Apply(x => x.AddOrReplace(Array.Empty<Sample>())), "add or replace: nothing", originalIds);

            // ReplaceAll, and several changes on one builder
            ShouldHoldIds(Apply(x => x.ReplaceAll(new List<Sample> { added, replaced })), "replace all", new[] { 7, 11 });
            ShouldHoldIds(Apply(x => x.ReplaceAll(new List<Sample>())), "replace all with nothing", Array.Empty<int>());
            ShouldHoldIds(Apply(x => { x.RemoveSample(new[] { 1, 2, 3 }); x.AddOrReplace(new[] { added }); x.RemoveSample(new[] { 11, 4 }); x.AddOrReplace(new[] { replaced }); }),
                "remove, add, remove, replace", originalIds.Except(new[] { 1, 2, 3, 4 }));

            failures.ShouldBeNone(output);
        }    }
}
