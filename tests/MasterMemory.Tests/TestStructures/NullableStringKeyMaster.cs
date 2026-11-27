#nullable enable
using MessagePack;
using System;

namespace MasterMemory.Tests.TestStructures
{
    // keys declared "string?": compared like "string" keys, ordinal unless [StringComparisonOption] says otherwise
    [MemoryTable("nullable_string_key"), MessagePackObject(true)]
    public class NullableStringKeyMaster
    {
        [PrimaryKey]
        public string? Code { get; set; }

        [SecondaryKey(0), StringComparisonOption(StringComparison.OrdinalIgnoreCase)]
        public string? Alias { get; set; }

        public int Value { get; set; }
    }
}
