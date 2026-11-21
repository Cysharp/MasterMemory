using MessagePack;

namespace MasterMemory.Tests.TestStructures
{
    // one table per numeric type a unique primary key is looked up inline for

    [MemoryTable("int_key"), MessagePackObject(true)]
    public class IntKeyMaster
    {
        [PrimaryKey] public int Id { get; set; }
        public int Value { get; set; }
    }

    [MemoryTable("uint_key"), MessagePackObject(true)]
    public class UIntKeyMaster
    {
        [PrimaryKey] public uint Id { get; set; }
        public int Value { get; set; }
    }

    [MemoryTable("long_key"), MessagePackObject(true)]
    public class LongKeyMaster
    {
        [PrimaryKey] public long Id { get; set; }
        public int Value { get; set; }
    }

    [MemoryTable("ulong_key"), MessagePackObject(true)]
    public class ULongKeyMaster
    {
        [PrimaryKey] public ulong Id { get; set; }
        public int Value { get; set; }
    }

    [MemoryTable("byte_key"), MessagePackObject(true)]
    public class ByteKeyMaster
    {
        [PrimaryKey] public byte Id { get; set; }
        public int Value { get; set; }
    }

    [MemoryTable("sbyte_key"), MessagePackObject(true)]
    public class SByteKeyMaster
    {
        [PrimaryKey] public sbyte Id { get; set; }
        public int Value { get; set; }
    }
}
