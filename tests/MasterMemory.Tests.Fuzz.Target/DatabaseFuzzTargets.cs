using MessagePack;

namespace MasterMemory.Tests.Fuzz.Target;

/// <summary>A database that is not one was not turned away the way it should be, or one that loaded misbehaved.</summary>
public sealed class DatabaseLoadException(string message, Exception inner) : Exception(message, inner);

/// <summary>
/// The fuzz target of loading: the input is handed to <c>new MemoryDatabase(bytes)</c> as a database binary.
/// </summary>
/// <remarks>
/// A database binary is the application's own data, but what arrives is not always what was built: a download cut
/// short, a file damaged on disk, a file of another format under the same name. What is required of loading such
/// bytes:
///   - it ends, and it ends in one of two ways: a database, or an exception of the kinds a caller can be told to
///     expect (<see cref="IsRejection"/>). Not an IndexOutOfRangeException or a NullReferenceException from somewhere
///     inside, and not an attempt to allocate what a damaged length field asks for;
///   - a database that did load can be used: every lookup that does not throw for a missing key returns, whatever
///     order and whatever keys the rows turned out to have.
/// That the rows found are the right ones is not checked here. Bytes that decode to rows out of order, or to two rows
/// with one unique key, are a database whose content is wrong, not one that is malformed.
/// </remarks>
public static class DatabaseFuzzTargets
{
    public const string Name = "db/load";

    /// <summary>
    /// What loading a malformed database throws: InvalidOperationException from MasterMemory's own checks (the header,
    /// the location of a table), MessagePackSerializationException from reading a table.
    /// </summary>
    public static bool IsRejection(Exception ex)
    {
        return ex is InvalidOperationException || ex is MessagePackSerializationException;
    }

    public static void Load(byte[] data)
    {
        MemoryDatabase database;
        try
        {
            database = new MemoryDatabase(data, internString: false);
        }
        catch (Exception ex) when (IsRejection(ex))
        {
            return;
        }
        catch (Exception ex)
        {
            throw new DatabaseLoadException($"loading threw {ex.GetType().Name}, which is not how a malformed database is turned away: {ex.Message}", ex);
        }

        try
        {
            Use(database);
        }
        catch (Exception ex)
        {
            throw new DatabaseLoadException($"a database that loaded threw {ex.GetType().Name} when used: {ex.Message}", ex);
        }
    }

    // every kind of lookup that returns for a missing key, by the keys of the rows themselves and by a few others
    static void Use(MemoryDatabase database)
    {
        var count = 0;

        var rows = database.FuzzRowTable;
        foreach (var row in rows.All)
        {
            rows.TryFindById(row.Id, out _);
            rows.FindClosestById(row.Id + 1, selectLower: false);
            rows.TryFindBySerial(row.Serial, out _);
            count += rows.FindByGroup(row.Group).Count;
            rows.TryFindByGroupAndSeq(row.Group, row.Seq, out _);
            foreach (var found in rows.FindByName(row.Name)) count += found.Id & 1;
            count += rows.FindClosestByNameAndKind(row.Name, row.Kind).Count;
            count += rows.FindByKind(row.Kind).Count;
            if (row.Code != null) rows.TryFindByCode(row.Code, out _);
            count += rows.FindClosestByScore(row.Score, selectLower: false).Count;
            rows.TryFindByBucketAndKindAndSeq(row.Bucket, row.Kind, row.Seq, out _);
            count += rows.FindByBucket(row.Bucket).Count;
            count += rows.FindByBucketAndKind(row.Bucket, row.Kind).Count;
        }
        rows.TryFindById(int.MinValue, out _);
        rows.TryFindById(int.MaxValue, out _);
        rows.TryFindByCode("", out _);
        foreach (var found in rows.FindRangeById(int.MinValue, int.MaxValue, ascendant: false)) count += found.Id & 1;
        foreach (var found in rows.FindRangeByName(null, "~")) count += found.Id & 1;
        count += rows.FindRangeByGroupAndSeq(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue).Count;
        count += rows.FindRangeByBucket(-1, 3).Count;
        count += rows.SortByScore.Count + rows.SortByCode.Count;

        var longs = database.LongKeyRowTable;
        foreach (var row in longs.All) longs.TryFindById(row.Id, out _);
        count += longs.FindRangeById(long.MinValue, long.MaxValue).Count;

        var bytes = database.ByteKeyRowTable;
        foreach (var row in bytes.All) bytes.TryFindById(row.Id, out _);
        bytes.FindClosestById(128);

        var chars = database.CharKeyRowTable;
        foreach (var row in chars.All) chars.TryFindById(row.Id, out _);
        count += chars.FindRangeById('\0', char.MaxValue).Count;

        GC.KeepAlive(count);
    }

    /// <summary>Databases as they are built: none, one row, tables below and above the size that is stored compressed.</summary>
    public static byte[][] SeedCorpus()
    {
        static FuzzRow Row(int i) => new FuzzRow
        {
            Id = (i * 3) + 1,
            Serial = i * 5_000_000_000L,
            Group = i % 4,
            Seq = i,
            Name = (i % 5 == 0) ? null : "n" + (i % 3),
            Kind = (FuzzKind)(i % 4),
            Code = "c" + i.ToString("D2"),
            Score = (i % 7 == 0) ? double.NaN : i / 2.0,
            Bucket = i % 3,
        };

        static FuzzRow[] Rows(int count) => Enumerable.Range(0, count).Select(Row).ToArray();

        return
        [
            new DatabaseBuilder().Build(),
            new DatabaseBuilder().Append(Rows(1)).Build(),
            new DatabaseBuilder().Append(Rows(3)).Append([new LongKeyRow { Id = long.MinValue, No = 1 }, new LongKeyRow { Id = 7, No = 2 }]).Build(),
            new DatabaseBuilder().Append(Rows(12)).Append([new ByteKeyRow { Id = 1 }, new ByteKeyRow { Id = 2 }, new ByteKeyRow { Id = 3 }]).Append([new CharKeyRow { Id = 'a' }, new CharKeyRow { Id = 'z' }]).Build(),
            new DatabaseBuilder().Append(Rows(32)).Build(),
        ];
    }
}
