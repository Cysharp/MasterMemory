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

        public SkillMaster()
        {

        }

        public SkillMaster(int SkillId, int SkillLevel, int AttackPower, string SkillName, string Description)
        {
            this.SkillId = SkillId;
            this.SkillLevel = SkillLevel;
            this.AttackPower = AttackPower;
            this.SkillName = SkillName;
            this.Description = Description;
        }

    }
}