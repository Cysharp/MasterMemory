#nullable enable
using MessagePack;
using System;
using MyId = System.Int32;

namespace MasterMemory.Tests.TestStructures
{
    // Tables declared in the ways that were not read, or not read right, while the generator went by the text of a
    // declaration. TableShapeTest builds, loads and searches each of them.

    // a positional record: the keys are on the properties its parameters declare
    [MemoryTable("record_master"), MessagePackObject(true)]
    public record RecordMaster([property: PrimaryKey] int Id, [property: SecondaryKey(0), NonUnique] string Group, int Level);

    // keys declared in a base class
    public abstract class EntityBase
    {
        [PrimaryKey]
        public int Id { get; set; }

        [SecondaryKey(0), NonUnique]
        public int Tenant { get; set; }
    }

    [MemoryTable("derived_master"), MessagePackObject(true)]
    public class DerivedMaster : EntityBase
    {
        public string Name { get; set; } = "";
    }

    // an internal table in a database that is otherwise public
    [MemoryTable("internal_master"), MessagePackObject(true)]
    internal class InternalMaster
    {
        [PrimaryKey]
        public int Id { get; set; }

        [SecondaryKey(0), NonUnique]
        public string Group { get; set; } = "";
    }

    // Composite keys with strings: compared member by member, a string ordinally unless its own
    // [StringComparisonOption] says otherwise.
    [MemoryTable("composite_name"), MessagePackObject(true)]
    public class CompositeNameMaster
    {
        [PrimaryKey(0)]
        public string Code { get; set; } = "";

        [PrimaryKey(1)]
        public int No { get; set; }

        [SecondaryKey(0, 0), NonUnique, StringComparisonOption(StringComparison.OrdinalIgnoreCase)]
        public string Label { get; set; } = "";

        [SecondaryKey(0, 1), NonUnique]
        public int Rank { get; set; }

        public int Value { get; set; }
    }

    public static class SpelledIndexes
    {
        public const int Class = 0;
        public const int Name = 1;
    }

    // attributes and types spelled the long way round: the suffix form, an alias of int, an index from a constant,
    // the option in front of the key it belongs to, a keyword as a property name
    [MemoryTable("spelled_master"), MessagePackObject(true)]
    public class SpelledMaster
    {
        [PrimaryKeyAttribute]
        public MyId Id { get; set; }

        [SecondaryKey(SpelledIndexes.Class)]
        public short @class { get; set; }

        [StringComparisonOption(StringComparison.OrdinalIgnoreCase), SecondaryKey(SpelledIndexes.Name)]
        public string Name { get; set; } = "";

        [SecondaryKey(SpelledIndexes.Name + 1)]
        public char Letter { get; set; }
    }

    // a partial class: the columns here, the keys in PartialMaster.Keys.cs
    [MemoryTable("partial_master"), MessagePackObject(true)]
    public partial class PartialMaster
    {
        public string Name { get; set; } = "";
    }
}
