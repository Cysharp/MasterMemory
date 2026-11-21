using MessagePack;
using System;

namespace MasterMemory.Tests.TestStructures
{
    public enum SmallKind : byte
    {
        A, B, C
    }

    // keys of the types a lookup compares through Comparer<T>.Default, or, for the enum, through its underlying value
    [MemoryTable("other_key_types"), MessagePackObject(true)]
    public class OtherKeyTypesMaster
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        [SecondaryKey(0)]
        public double Score { get; set; }

        [SecondaryKey(1), NonUnique]
        public int? Level { get; set; }

        [SecondaryKey(2)]
        public DateTime At { get; set; }

        [SecondaryKey(3), NonUnique]
        [SecondaryKey(8, 1), NonUnique]
        [SecondaryKey(10, 1), NonUnique]
        public short Rank { get; set; }

        [SecondaryKey(4), NonUnique]
        public bool Flag { get; set; }

        [SecondaryKey(5)]
        public char Letter { get; set; }

        [SecondaryKey(6)]
        public decimal Price { get; set; }

        [SecondaryKey(7), NonUnique]
        [SecondaryKey(8, 0), NonUnique]
        public SmallKind Kind { get; set; }

        // a nullable enum: null first, then by the underlying value
        [SecondaryKey(9), NonUnique]
        [SecondaryKey(10, 0), NonUnique]
        public SmallKind? MaybeKind { get; set; }

        public int No { get; set; }
    }
}
