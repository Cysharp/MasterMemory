using MessagePack;

[assembly: MasterMemory.MasterMemoryGeneratorOptions(Namespace = "MasterMemory.AotTests")]

namespace MasterMemory.AotTests
{
    public enum Element
    {
        Fire = 1,
        Water = 2,
        Wind = 3,
    }

    // Every kind of key the generator has code for: a primary key with gaps (searched), a unique string key that
    // ignores case, an enum key that several rows share, and a key of two members that several rows share.
    [MemoryTable("monster"), MessagePackObject(true)]
    public class Monster : IValidatable<Monster>
    {
        [PrimaryKey]
        public int MonsterId { get; set; }

        [SecondaryKey(0), StringComparisonOption(StringComparison.OrdinalIgnoreCase)]
        public string Name { get; set; } = "";

        [SecondaryKey(1), NonUnique]
        public Element Element { get; set; }

        [SecondaryKey(2, keyOrder: 0), NonUnique]
        public int Rank { get; set; }

        [SecondaryKey(2, keyOrder: 1), NonUnique]
        public int Level { get; set; }

        public int MaxHp { get; set; }
        public int DropItemId { get; set; }
        public string? Note { get; set; }

        // each kind of validation: a reference to another table, expressions (compiled by the interpreter under
        // Native AOT, and dumped member by member when they fail), delegates, and the checks over the whole table
        public void Validate(IValidator<Monster> validator)
        {
            validator.GetReferenceSet<Item>().Exists(x => x.DropItemId, x => x.ItemId);

            validator.Validate(x => x.MaxHp > 0);
            validator.Validate(x => x.Level >= 1, "the level starts at 1");
            validator.ValidateAction(() => this.Rank <= 9);
            validator.ValidateAction(() => this.Name.Length != 0, "a monster has a name");

            if (validator.CallOnce())
            {
                var monsters = validator.GetTableSet();
                monsters.Unique(x => x.Name);
                monsters.Where(x => x.Element == Element.Wind).Unique(x => x.Level, "one wind monster per level");
            }
        }
    }

    // a primary key without a gap (1, 2, 3, ...): read at its position, not searched
    [MemoryTable("item"), MessagePackObject(true)]
    public class Item : IValidatable<Item>
    {
        [PrimaryKey]
        public int ItemId { get; set; }
        public string Name { get; set; } = "";
        public long Price { get; set; }

        public void Validate(IValidator<Item> validator)
        {
            if (validator.CallOnce())
            {
                validator.GetTableSet().Sequential(x => x.ItemId);
            }
        }
    }

    // a primary key of two members
    [MemoryTable("stage"), MessagePackObject(true)]
    public class Stage
    {
        [PrimaryKey(keyOrder: 0)]
        public int Chapter { get; set; }

        [PrimaryKey(keyOrder: 1)]
        public int Number { get; set; }

        public string Title { get; set; } = "";
    }

    // a primary key that is a string
    [MemoryTable("message"), MessagePackObject(true)]
    public class Message
    {
        [PrimaryKey]
        public string Key { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
