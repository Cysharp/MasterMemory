using MessagePack;

namespace MasterMemory.Tests
{
    [MemoryTable("skillmaster"), MessagePackObject(true)]
    public class SkillMaster
    {
        [PrimaryKey]
        public int SkillId { get; set; }
        [PrimaryKey]
        public int SkillLevel { get; set; }
        public int AttackPower { get; set; }
        public string SkillName { get; set; }
        public string Description { get; set; }
    }
}