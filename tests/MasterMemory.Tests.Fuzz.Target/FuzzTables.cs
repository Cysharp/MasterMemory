using MessagePack;

namespace MasterMemory.Tests.Fuzz.Target;

public enum FuzzKind : byte
{
    A, B, C, D
}

// One table with a key of every kind a lookup is generated differently for. The fuzz target fills it so that the keys
// declared unique are unique by construction (see FindFuzzTargets.AllKeys).
[MemoryTable("fuzz_row"), MessagePackObject(true)]
public class FuzzRow
{
    // unique integer: read at its position when the ids have no gap, else searched without jumps or with them
    [PrimaryKey]
    public int Id { get; set; }

    // unique, 64 bits: the jumping search only
    [SecondaryKey(0)]
    public long Serial { get; set; }

    // shared integer, and the first member of a unique pair
    [SecondaryKey(1), NonUnique]
    [SecondaryKey(3, 0)]
    public int Group { get; set; }

    [SecondaryKey(3, 1)]
    [SecondaryKey(8, 2)]
    public int Seq { get; set; }

    // the first member of a unique key of three, (Bucket, Kind, Seq). Neither Bucket nor (Bucket, Kind) is a key of its
    // own, so both are looked up as heads of this one, in its index
    [SecondaryKey(8, 0)]
    public int Bucket { get; set; }

    // shared string (ordinal), and the first member of a shared pair (compared member by member, the string ordinally)
    [SecondaryKey(2), NonUnique]
    [SecondaryKey(4, 0), NonUnique]
    public string? Name { get; set; }

    // shared enum, compared by its underlying value
    [SecondaryKey(5), NonUnique]
    [SecondaryKey(4, 1), NonUnique]
    [SecondaryKey(8, 1)]
    public FuzzKind Kind { get; set; }

    // unique string with a comparison option
    [SecondaryKey(6), StringComparisonOption(StringComparison.OrdinalIgnoreCase)]
    public string? Code { get; set; }

    // shared, through Comparer<double>.Default: NaN and the infinities have a place in the order
    [SecondaryKey(7), NonUnique]
    public double Score { get; set; }
}

// unique numeric primary keys of the other widths, for the lookup that reads a gapless table at (key - first key)

[MemoryTable("fuzz_long"), MessagePackObject(true)]
public class LongKeyRow
{
    [PrimaryKey] public long Id { get; set; }
    public int No { get; set; }
}

[MemoryTable("fuzz_uint"), MessagePackObject(true)]
public class UIntKeyRow
{
    [PrimaryKey] public uint Id { get; set; }
    public int No { get; set; }
}

[MemoryTable("fuzz_byte"), MessagePackObject(true)]
public class ByteKeyRow
{
    [PrimaryKey] public byte Id { get; set; }
    public int No { get; set; }
}

[MemoryTable("fuzz_sbyte"), MessagePackObject(true)]
public class SByteKeyRow
{
    [PrimaryKey] public sbyte Id { get; set; }
    public int No { get; set; }
}

[MemoryTable("fuzz_short"), MessagePackObject(true)]
public class ShortKeyRow
{
    [PrimaryKey] public short Id { get; set; }
    public int No { get; set; }
}

[MemoryTable("fuzz_ushort"), MessagePackObject(true)]
public class UShortKeyRow
{
    [PrimaryKey] public ushort Id { get; set; }
    public int No { get; set; }
}

[MemoryTable("fuzz_char"), MessagePackObject(true)]
public class CharKeyRow
{
    [PrimaryKey] public char Id { get; set; }
    public int No { get; set; }
}