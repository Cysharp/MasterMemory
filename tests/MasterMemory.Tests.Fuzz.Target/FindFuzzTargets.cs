using MasterMemory.Internal;
using MasterMemory.Tests.Fuzz.Target.Tables;
using System.Numerics;

namespace MasterMemory.Tests.Fuzz.Target;

/// <summary>A lookup answered something else than a pass over all the rows says it should.</summary>
public sealed class FindMismatchException(string message) : Exception(message);

/// <summary>
/// The fuzz targets of the lookups. Each turns its input into a table and a list of keys, asks the table every kind of
/// lookup for every key, and compares each answer with what a linear pass over the rows gives (the same definitions
/// as FindSpecificationTest in MasterMemory.Tests). A wrong answer throws <see cref="FindMismatchException"/>; so the
/// oracle is "the answer is right", not only "nothing crashed".
/// The targets are shared by the libFuzzer harness (MasterMemory.Tests.Fuzz, coverage-guided) and by FindFuzzTest in
/// MasterMemory.Tests, which runs them on the seeds and on random inputs as an ordinary test.
/// </summary>
public static class FindFuzzTargets
{
    const int MaxRows = 32;
    const int MaxProbes = 48;

    public static readonly (string Name, Action<byte[]> Run)[] Targets =
    [
        ("table/all-keys", AllKeys),
        ("search/int", Searches),
        ("pk/long", data => NumericPrimaryKey<long, LongKeyRow>("long", data, (id, no) => new LongKeyRow { Id = id, No = no }, x => x.Id, rows =>
        {
            var table = new LongKeyRowTable(rows);
            return new(table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);
        })),
        ("pk/uint", data => NumericPrimaryKey<uint, UIntKeyRow>("uint", data, (id, no) => new UIntKeyRow { Id = id, No = no }, x => x.Id, rows =>
        {
            var table = new UIntKeyRowTable(rows);
            return new(table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);
        })),
        ("pk/byte", data => NumericPrimaryKey<byte, ByteKeyRow>("byte", data, (id, no) => new ByteKeyRow { Id = id, No = no }, x => x.Id, rows =>
        {
            var table = new ByteKeyRowTable(rows);
            return new(table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);
        })),
        ("pk/sbyte", data => NumericPrimaryKey<sbyte, SByteKeyRow>("sbyte", data, (id, no) => new SByteKeyRow { Id = id, No = no }, x => x.Id, rows =>
        {
            var table = new SByteKeyRowTable(rows);
            return new(table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);
        })),
        // added after the first campaign, at the end: the seeds of an existing corpus (selector = index) keep their target
        ("pk/short", data => NumericPrimaryKey<short, ShortKeyRow>("short", data, (id, no) => new ShortKeyRow { Id = id, No = no }, x => x.Id, rows =>
        {
            var table = new ShortKeyRowTable(rows);
            return new(table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);
        })),
        ("pk/ushort", data => NumericPrimaryKey<ushort, UShortKeyRow>("ushort", data, (id, no) => new UShortKeyRow { Id = id, No = no }, x => x.Id, rows =>
        {
            var table = new UShortKeyRowTable(rows);
            return new(table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);
        })),
        ("pk/char", data => NumericPrimaryKey<char, CharKeyRow>("char", data, (id, no) => new CharKeyRow { Id = id, No = no }, x => x.Id, rows =>
        {
            var table = new CharKeyRowTable(rows);
            return new(table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);
        })),
    ];

    /// <summary>Inputs to start a campaign from: tables of several sizes, with and without gaps in their ids, and a few keys each.</summary>
    public static byte[][] SeedCorpus()
    {
        var random = new Random(20261121);
        var seeds = new List<byte[]> { Array.Empty<byte>(), new byte[] { 0 }, new byte[] { 1, 0, 0, 0, 0, 0 } };

        foreach (var rows in new[] { 0, 1, 2, 3, 8, 16, MaxRows })
        {
            foreach (var gapless in new[] { true, false })
            {
                var bytes = new byte[2 + (rows * 4) + (12 * 4)];
                random.NextBytes(bytes);
                bytes[0] = (byte)rows;
                if (gapless)
                {
                    for (var i = 0; i < rows; i++) bytes[2 + (i * 4)] &= 0xFC; // every id one above the last
                }
                seeds.Add(bytes);
            }
        }

        return seeds.ToArray();
    }

    // an input is read byte by byte; past its end it reads as zero, so every input is a valid table and key list
    sealed class Input(byte[] data)
    {
        int position;

        public bool HasMore => position < data.Length;

        public byte Next() => (position < data.Length) ? data[position++] : (byte)0;
    }

    // ---- FuzzRow: every kind of key at once ----

    static readonly int[] FirstIds = [int.MinValue, -5, 0, 1, 1000, int.MaxValue - 200];
    static readonly string?[] Names = [null, "", "a", "A", "aa", "a_", "ab", "b", "B", "é", "~"];
    static readonly string?[] ProbeNames = [.. Names, "0", "a ", "ac", "c", "Z"];
    static readonly double[] Scores = [double.NaN, double.NegativeInfinity, -1.5, -0.0, 0.0, 0.5, 1, 2.5, double.PositiveInfinity];
    static readonly double[] ProbeScores = [.. Scores, double.MinValue, -2, 0.25, 3, double.MaxValue];

    // a different serial for each of up to 37 rows, in an order unrelated to the ids, wider than 32 bits and on both sides of zero
    static long SerialOf(int index) => (((index * 7) % 37) * 5_000_000_000L) - 40_000_000_000L;

    static void AllKeys(byte[] data)
    {
        var input = new Input(data);
        var count = input.Next() % (MaxRows + 1);
        var firstId = FirstIds[input.Next() % FirstIds.Length];

        // four bytes a row: the step to the next id (1 to 4; all 1 gives a table without gaps) and the case of the
        // code, the group, the name and the kind, the score
        var rows = new FuzzRow[count];
        var id = firstId;
        for (var i = 0; i < count; i++)
        {
            byte b0 = input.Next(), b1 = input.Next(), b2 = input.Next(), b3 = input.Next();
            id += (i == 0) ? (b0 & 3) : 1 + (b0 & 3);
            rows[i] = new FuzzRow
            {
                Id = id,
                Serial = SerialOf(i),
                Group = (b1 % 6) - 1,
                Seq = i,
                Name = Names[(b2 & 15) % Names.Length],
                Kind = (FuzzKind)((b2 >> 4) & 3),
                Code = (((b0 & 4) != 0) ? "C" : "c") + i.ToString("D2"),
                Score = Scores[b3 % Scores.Length],
                Bucket = ((b3 >> 4) % 5) - 1,
            };
        }

        var table = new FuzzRowTable(rows); // the rows are in id order, which is what a table is given

        // four bytes a key, one key for every index out of them
        var ids = new List<int>();
        var serials = new List<long>();
        var groups = new List<int>();
        var pairs = new List<(int Group, int Seq)>();
        var names = new List<string?>();
        var nameAndKinds = new List<(string? Name, FuzzKind Kind)>();
        var kinds = new List<FuzzKind>();
        var codes = new List<string?>();
        var scores = new List<double>();
        var buckets = new List<int>();
        var bucketAndKinds = new List<(int Bucket, FuzzKind Kind)>();
        var bucketAndKindAndSeqs = new List<(int Bucket, FuzzKind Kind, int Seq)>();
        do
        {
            byte p0 = input.Next(), p1 = input.Next(), p2 = input.Next(), p3 = input.Next();

            ids.Add(p3 switch { 255 => int.MaxValue, 254 => int.MinValue, _ => unchecked(firstId - 3 + p0) });
            serials.Add(p3 switch { 255 => long.MaxValue, 254 => long.MinValue, _ => (((p1 % 40) - 1) * 5_000_000_000L) - 40_000_000_000L + (p2 % 3) - 1 });
            groups.Add((p1 % 9) - 2);
            pairs.Add(((p1 % 9) - 2, (p2 % (MaxRows + 4)) - 2));
            names.Add(ProbeNames[p2 % ProbeNames.Length]);
            kinds.Add((FuzzKind)(p3 % 6)); // two values the enum does not declare
            nameAndKinds.Add((ProbeNames[p2 % ProbeNames.Length], (FuzzKind)(p3 % 6)));
            buckets.Add((p0 % 8) - 2);
            bucketAndKinds.Add(((p0 % 8) - 2, (FuzzKind)(p3 % 6)));
            bucketAndKindAndSeqs.Add(((p0 % 8) - 2, (FuzzKind)(p3 % 6), (p2 % (MaxRows + 4)) - 2));
            codes.Add((p3 % 8) switch
            {
                4 => null,
                5 => "",
                6 => "c",
                7 => "d00",
                var x => (((x & 1) == 0) ? "c" : "C") + (p0 % (MaxRows + 4)).ToString("D2"),
            });
            scores.Add(ProbeScores[p3 % ProbeScores.Length]);
        }
        while (input.HasMore && ids.Count < MaxProbes);

        VerifyUnique("Id", rows, x => x.Id, Comparer<int>.Default, ids,
            table.FindById, table.TryFindById, table.FindClosestById, table.FindRangeById);
        VerifyUnique("Serial", Rows(table.SortBySerial), x => x.Serial, Comparer<long>.Default, serials,
            table.FindBySerial, table.TryFindBySerial, table.FindClosestBySerial, table.FindRangeBySerial);
        VerifyShared("Group", Rows(table.SortByGroup), x => x.Group, Comparer<int>.Default, groups,
            table.FindByGroup, table.FindClosestByGroup, table.FindRangeByGroup);
        VerifyUnique("(Group, Seq)", Rows(table.SortByGroupAndSeq), x => (x.Group, x.Seq), Comparer<(int Group, int Seq)>.Default, pairs,
            table.FindByGroupAndSeq, table.TryFindByGroupAndSeq, table.FindClosestByGroupAndSeq, table.FindRangeByGroupAndSeq);
        VerifyShared("Name", Rows(table.SortByName), x => x.Name, StringComparer.Ordinal, names,
            table.FindByName, table.FindClosestByName, table.FindRangeByName);
        VerifyShared("(Name, Kind)", Rows(table.SortByNameAndKind), x => (x.Name, x.Kind), OrdinalTupleComparer.Create<string?, FuzzKind>(), nameAndKinds,
            table.FindByNameAndKind, table.FindClosestByNameAndKind, table.FindRangeByNameAndKind);
        VerifyShared("Kind", Rows(table.SortByKind), x => x.Kind, Comparer<FuzzKind>.Default, kinds,
            table.FindByKind, table.FindClosestByKind, table.FindRangeByKind);
        VerifyUnique("Code", Rows(table.SortByCode), x => x.Code, StringComparer.OrdinalIgnoreCase, codes,
            table.FindByCode, table.TryFindByCode, table.FindClosestByCode, table.FindRangeByCode);
        VerifyShared("Score", Rows(table.SortByScore), x => x.Score, Comparer<double>.Default, scores,
            table.FindByScore, table.FindClosestByScore, table.FindRangeByScore);

        // the key of three, and its two heads, all in the one index
        var bucketIndex = Rows(table.SortByBucketAndKindAndSeq);
        VerifyUnique("(Bucket, Kind, Seq)", bucketIndex, x => (x.Bucket, x.Kind, x.Seq), Comparer<(int Bucket, FuzzKind Kind, int Seq)>.Default, bucketAndKindAndSeqs,
            table.FindByBucketAndKindAndSeq, table.TryFindByBucketAndKindAndSeq, table.FindClosestByBucketAndKindAndSeq, table.FindRangeByBucketAndKindAndSeq);
        VerifyHead("Bucket", bucketIndex, x => x.Bucket, Comparer<int>.Default, buckets,
            table.FindByBucket, table.FindRangeByBucket);
        VerifyHead("(Bucket, Kind)", bucketIndex, x => (x.Bucket, x.Kind), Comparer<(int Bucket, FuzzKind Kind)>.Default, bucketAndKinds,
            table.FindByBucketAndKind, table.FindRangeByBucketAndKind);
    }

    // the head of a composite key: every row whose first members equal the key, in the order of the index of the whole key
    static void VerifyHead<TRow, TKey>(string name, TRow[] index, Func<TRow, TKey> selector, IComparer<TKey> comparer, List<TKey> keys,
        Func<TKey, RangeView<TRow>> find, Func<TKey, TKey, bool, RangeView<TRow>> range)
        where TRow : class
    {
        for (var k = 0; k < keys.Count; k++)
        {
            var key = keys[k];
            var exact = new List<TRow>();
            foreach (var row in index)
            {
                if (comparer.Compare(selector(row), key) == 0) exact.Add(row);
            }

            VerifyView($"{name} FindBy({Show(key)})", find(key), exact, false, selector);
            VerifyRange(name, index, selector, comparer, key, keys[(k + 1) % keys.Count], range);
        }
    }

    // ---- unique numeric primary keys of the other widths ----

    readonly record struct Lookups<TKey, TRow>(Func<TKey, TRow> Find, TryFind<TKey, TRow> TryFind, Func<TKey, bool, TRow?> Closest, Func<TKey, TKey, bool, RangeView<TRow>> Range);

    static void NumericPrimaryKey<TKey, TRow>(string name, byte[] data, Func<TKey, int, TRow> create, Func<TRow, TKey> key, Func<TRow[], Lookups<TKey, TRow>> build)
        where TKey : struct, INumber<TKey>, IMinMaxValue<TKey>
        where TRow : class
    {
        var input = new Input(data);
        var count = input.Next() % (MaxRows + 1);
        var first = (input.Next() % 4) switch
        {
            0 => TKey.MinValue,
            1 => TKey.Zero,
            2 => TKey.MaxValue - TKey.CreateChecked(100),
            _ => TKey.CreateChecked(10),
        };

        // ids going up by 1 to 4, for as many rows as fit below the maximum of the type
        var rows = new List<TRow>(count);
        var id = first;
        for (var i = 0; i < count; i++)
        {
            var step = TKey.CreateChecked((i == 0) ? (input.Next() & 3) : 1 + (input.Next() & 3));
            if (TKey.MaxValue - id < step) break;
            id += step;
            rows.Add(create(id, i));
        }

        var keys = new List<TKey>();
        do
        {
            byte p0 = input.Next(), p1 = input.Next();
            keys.Add(p1 switch
            {
                255 => TKey.MaxValue,
                254 => TKey.MinValue,
                _ => TKey.CreateTruncating(Int128.CreateChecked(first) - 3 + p0), // around the rows, wrapping at the ends of the type
            });
        }
        while (input.HasMore && keys.Count < MaxProbes);

        var index = rows.ToArray();
        var lookups = build(index);
        VerifyUnique(name, index, key, Comparer<TKey>.Default, keys, lookups.Find, lookups.TryFind, lookups.Closest, lookups.Range);
    }

    // ---- the searches themselves, on an array with repeated values ----

    readonly struct IntKey(int key) : IBranchlessComparable<int>
    {
        public int CompareTo(int other) => key.CompareTo(other);

        public int AtOrAfterMask(int other) => (int)(((long)other - key - 1) >> 63);
    }

    static void Searches(byte[] data)
    {
        var input = new Input(data);
        var count = input.Next() % 41;

        // sorted, steps of 0 to 3: a step of 0 repeats a value
        var array = new int[count];
        var value = 10;
        for (var i = 0; i < count; i++)
        {
            value += input.Next() & 3;
            array[i] = value;
        }

        do
        {
            var key = 7 + input.Next();
            var comparable = new IntKey(key);

            // what a pass over the array says
            int before = 0, equal = 0;
            foreach (var item in array)
            {
                if (item < key) before++;
                else if (item == key) equal++;
            }
            var firstEqual = (equal != 0) ? before : -1;
            var lastEqual = (equal != 0) ? before + equal - 1 : -1;

            void Expect(string operation, int actual, int expected)
            {
                if (actual != expected) Fail($"search/int {operation}({key}) in [{string.Join(",", array)}]", actual.ToString(), expected.ToString());
            }

            void ExpectEqualElement(string operation, int actual, int whenMissing)
            {
                if (equal != 0)
                {
                    if (actual < firstEqual || actual > lastEqual) Fail($"search/int {operation}({key}) in [{string.Join(",", array)}]", actual.ToString(), $"{firstEqual}..{lastEqual}");
                }
                else
                {
                    Expect(operation, actual, whenMissing);
                }
            }

            ExpectEqualElement("Find", BinarySearch.Find(array, comparable), ~before);
            ExpectEqualElement("FindBranchless", BinarySearch.FindBranchless(array, comparable), -1);
            ExpectEqualElement("FindClosest(lower)", BinarySearch.FindClosest(array, comparable, true), before - 1);
            ExpectEqualElement("FindClosest(upper)", BinarySearch.FindClosest(array, comparable, false), (count == 0) ? -1 : before);
            Expect("LowerBound", BinarySearch.LowerBound(array, comparable), firstEqual);
            Expect("UpperBound", BinarySearch.UpperBound(array, comparable), lastEqual);
            Expect("LowerBoundClosest", BinarySearch.LowerBoundClosest(array, comparable), before);
            Expect("UpperBoundClosest", BinarySearch.UpperBoundClosest(array, comparable), before + equal - 1);
        }
        while (input.HasMore);
    }

    // ---- the oracle ----

    public delegate bool TryFind<TKey, TRow>(TKey key, out TRow result);

    static TRow[] Rows<TRow>(RangeView<TRow> view)
    {
        var rows = new TRow[view.Count];
        view.CopyTo(rows, 0);
        return rows;
    }

    static void Fail(string question, string actual, string expected)
    {
        throw new FindMismatchException($"{question} -> {actual}, expected {expected}");
    }

    static string Show<T>(T value) => (value == null) ? "null" : (value is string text) ? "\"" + text + "\"" : value.ToString()!;

    static string Show<TRow, TKey>(TRow? row, Func<TRow, TKey> selector) where TRow : class => (row == null) ? "no row" : "the row of " + Show(selector(row));

    // one row per key
    static void VerifyUnique<TRow, TKey>(string name, TRow[] index, Func<TRow, TKey> selector, IComparer<TKey> comparer, List<TKey> keys,
        Func<TKey, TRow> find, TryFind<TKey, TRow> tryFind, Func<TKey, bool, TRow?> closest, Func<TKey, TKey, bool, RangeView<TRow>> range)
        where TRow : class
    {
        for (var k = 0; k < keys.Count; k++)
        {
            var key = keys[k];

            // the row at the key, the last one before it, the first one after it
            TRow? exact = null, lower = null, upper = null;
            foreach (var row in index)
            {
                var c = comparer.Compare(selector(row), key);
                if (c == 0)
                {
                    if (exact != null) throw new InvalidOperationException($"fuzz target: {name} is not unique in the generated rows");
                    exact = row;
                }
                else if (c < 0)
                {
                    lower = row;
                }
                else if (upper == null)
                {
                    upper = row;
                }
            }

            TRow? found;
            var threw = false;
            try
            {
                found = find(key);
            }
            catch (KeyNotFoundException)
            {
                found = null;
                threw = true;
            }
            if (!ReferenceEquals(found, exact) || threw != (exact == null))
            {
                Fail($"{name} FindBy({Show(key)})", threw ? "KeyNotFoundException" : Show(found, selector), (exact == null) ? "KeyNotFoundException" : Show(exact, selector));
            }

            var tried = tryFind(key, out var result);
            if (tried != (exact != null) || !ReferenceEquals(result, exact))
            {
                Fail($"{name} TryFindBy({Show(key)})", $"{tried}, {Show(result, selector)}", $"{exact != null}, {Show(exact, selector)}");
            }

            var below = closest(key, true);
            if (!ReferenceEquals(below, exact ?? lower)) Fail($"{name} FindClosestBy({Show(key)}, lower)", Show(below, selector), Show(exact ?? lower, selector));

            var above = closest(key, false);
            if (!ReferenceEquals(above, exact ?? upper)) Fail($"{name} FindClosestBy({Show(key)}, upper)", Show(above, selector), Show(exact ?? upper, selector));

            VerifyRange(name, index, selector, comparer, key, keys[(k + 1) % keys.Count], range);
        }
    }

    // several rows per key
    static void VerifyShared<TRow, TKey>(string name, TRow[] index, Func<TRow, TKey> selector, IComparer<TKey> comparer, List<TKey> keys,
        Func<TKey, RangeView<TRow>> find, Func<TKey, bool, RangeView<TRow>> closest, Func<TKey, TKey, bool, RangeView<TRow>> range)
        where TRow : class
    {
        List<TRow> RowsOf(TKey key)
        {
            var rows = new List<TRow>();
            foreach (var row in index)
            {
                if (comparer.Compare(selector(row), key) == 0) rows.Add(row);
            }
            return rows;
        }

        for (var k = 0; k < keys.Count; k++)
        {
            var key = keys[k];

            TRow? lower = null, upper = null;
            foreach (var row in index)
            {
                var c = comparer.Compare(selector(row), key);
                if (c < 0) lower = row;
                else if (c > 0 && upper == null) upper = row;
            }

            var exact = RowsOf(key);
            VerifyView($"{name} FindBy({Show(key)})", find(key), exact, false, selector);

            // the rows of the key itself, else all the rows of the nearest key on the side asked for
            var below = (exact.Count != 0) ? exact : (lower == null) ? [] : RowsOf(selector(lower));
            var above = (exact.Count != 0) ? exact : (upper == null) ? [] : RowsOf(selector(upper));
            VerifyView($"{name} FindClosestBy({Show(key)}, lower)", closest(key, true), below, false, selector);
            VerifyView($"{name} FindClosestBy({Show(key)}, upper)", closest(key, false), above, false, selector);

            VerifyRange(name, index, selector, comparer, key, keys[(k + 1) % keys.Count], range);
        }
    }

    // every row from min to max, both included, in index order or reversed; empty when min is after max
    static void VerifyRange<TRow, TKey>(string name, TRow[] index, Func<TRow, TKey> selector, IComparer<TKey> comparer, TKey min, TKey max, Func<TKey, TKey, bool, RangeView<TRow>> range)
        where TRow : class
    {
        var expected = new List<TRow>();
        foreach (var row in index)
        {
            var rowKey = selector(row);
            if (comparer.Compare(rowKey, min) >= 0 && comparer.Compare(rowKey, max) <= 0) expected.Add(row);
        }

        VerifyView($"{name} FindRangeBy({Show(min)}, {Show(max)}, ascendant)", range(min, max, true), expected, false, selector);
        VerifyView($"{name} FindRangeBy({Show(min)}, {Show(max)}, descendant)", range(min, max, false), expected, true, selector);
    }

    // a RangeView against the rows it should hold, through each way of reading it
    static void VerifyView<TRow, TKey>(string question, RangeView<TRow> view, List<TRow> expected, bool reversed, Func<TRow, TKey> selector)
        where TRow : class
    {
        string Keys(IEnumerable<TRow> rows) => "[" + string.Join(" ", rows.Select(x => Show(selector(x)))) + "]";

        var count = expected.Count;
        TRow Expected(int i) => expected[reversed ? count - 1 - i : i];

        if (view.Count != count) Fail(question, $"{view.Count} rows {Keys(view)}", $"{count} rows {Keys(reversed ? Enumerable.Reverse(expected) : expected)}");

        var position = 0;
        foreach (var row in view)
        {
            if (position >= count || !ReferenceEquals(row, Expected(position))) Fail(question, Keys(view), Keys(reversed ? Enumerable.Reverse(expected) : expected));
            position++;
        }
        if (position != count) Fail(question + " (enumeration)", position + " rows", count + " rows");

        var copied = new TRow[count];
        view.CopyTo(copied, 0);
        var backwards = view.Reverse;
        if (backwards.Count != count) Fail(question + " (Reverse)", backwards.Count + " rows", count + " rows");
        for (var i = 0; i < count; i++)
        {
            if (!ReferenceEquals(view[i], Expected(i))) Fail(question + $" (indexer {i})", Show(selector(view[i])), Show(selector(Expected(i))));
            if (!ReferenceEquals(copied[i], Expected(i))) Fail(question + $" (CopyTo {i})", Show(selector(copied[i])), Show(selector(Expected(i))));
            if (!ReferenceEquals(backwards[i], Expected(count - 1 - i))) Fail(question + $" (Reverse {i})", Show(selector(backwards[i])), Show(selector(Expected(count - 1 - i))));
        }

        if (count != 0)
        {
            if (!ReferenceEquals(view.First, Expected(0))) Fail(question + " (First)", Show(selector(view.First)), Show(selector(Expected(0))));
            if (!ReferenceEquals(view.Last, Expected(count - 1))) Fail(question + " (Last)", Show(selector(view.Last)), Show(selector(Expected(count - 1))));
        }
    }
}
