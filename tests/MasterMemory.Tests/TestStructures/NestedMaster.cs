using MessagePack;

namespace MasterMemory.Tests.TestStructures
{
    public class NestedOwner
    {
        // a table declared inside another type: generated code has to name it NestedOwner.NestedMaster
        [MemoryTable("nested"), MessagePackObject(true)]
        public class NestedMaster
        {
            [PrimaryKey]
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
