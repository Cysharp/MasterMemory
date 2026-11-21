#nullable enable
using MessagePack;
using System;

namespace MasterMemory.Tests.TestStructures
{
    // The tables of Golden/v3-string-keys.bin: the three shapes of primary key that MasterMemory v3 stored in another
    // order than v4 does. V3StringKeyOrderTest reads that file.

    // a composite key with a string member: v3 compared it by the current culture, v4 ordinally
    [MemoryTable("v3_composite"), MessagePackObject(true)]
    public class V3CompositeMaster
    {
        [PrimaryKey(0)]
        public string Code { get; set; } = "";

        [PrimaryKey(1)]
        public int No { get; set; }

        public int Value { get; set; }
    }

    // "string?": v3 compared it by the current culture, v4 ordinally
    [MemoryTable("v3_nullable"), MessagePackObject(true)]
    public class V3NullableMaster
    {
        [PrimaryKey]
        public string? Code { get; set; }

        public int Value { get; set; }
    }

    // the option in an attribute list of its own: v3 did not apply it and kept the key ordinal, v4 applies it
    [MemoryTable("v3_option"), MessagePackObject(true)]
    public class V3OptionMaster
    {
        [PrimaryKey]
        [StringComparisonOption(StringComparison.OrdinalIgnoreCase)]
        public string Code { get; set; } = "";

        public int Value { get; set; }
    }
}
