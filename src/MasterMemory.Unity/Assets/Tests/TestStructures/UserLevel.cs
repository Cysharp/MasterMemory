using MessagePack;

namespace MasterMemory.Tests
{
    [MemoryTable("UserLevel"), MessagePackObject(true)]
    public class UserLevel
    {
        [PrimaryKey]
        public int Level { get; set; }
        [SecondaryKey(0)]
        public int Exp { get; set; }

        public UserLevel()
        {

        }

        public UserLevel(int Level, int Exp)
        {
            this.Level = Level;
            this.Exp = Exp;
        }

    }
}