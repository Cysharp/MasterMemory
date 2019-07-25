using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace MasterMemory.Tests.TestStructures
{
    [MessagePackObject(true)]
    [MemoryTable(nameof(TestMaster))]
    public class TestMaster
    {
        [PrimaryKey, NonUnique]
        public int TestID { get; set; }
        public int Value { get; set; }

        public TestMaster(int TestID, int Value)
        {
            this.TestID = TestID;
            this.Value = Value;
        }
    }
}
