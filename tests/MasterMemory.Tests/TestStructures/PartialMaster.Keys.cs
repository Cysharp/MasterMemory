#nullable enable

namespace MasterMemory.Tests.TestStructures
{
    // the other part of PartialMaster (ShapeMasters.cs), in a file of its own
    public partial class PartialMaster
    {
        [PrimaryKey(0)]
        public int Category { get; set; }

        [PrimaryKey(1)]
        public int No { get; set; }
    }
}
