#nullable enable
using MessagePack;
using System;

namespace MasterMemory.Tests.TestStructures
{
    // Composite keys whose heads have no key of their own, so each head gets its lookups from the index of the whole key:
    // FindByCategory, FindByCategoryAndSub, FindByKind, FindByName (and FindRangeBy of each).
    [MemoryTable("prefix_master"), MessagePackObject(true)]
    public class PrefixMaster
    {
        // unique, three members, the middle one ignoring case
        [PrimaryKey(0)]
        public int Category { get; set; }

        [PrimaryKey(1), StringComparisonOption(StringComparison.OrdinalIgnoreCase)]
        public string Sub { get; set; } = "";

        [PrimaryKey(2)]
        public int No { get; set; }

        // shared: an enum, then a nullable integer
        [SecondaryKey(0, 0), NonUnique]
        public SmallKind Kind { get; set; }

        [SecondaryKey(0, 1), NonUnique]
        public int? Level { get; set; }

        // unique: an ordinal string, then 64 bits
        [SecondaryKey(1, 0)]
        public string Name { get; set; } = "";

        [SecondaryKey(1, 1)]
        public long Serial { get; set; }
    }
}
