using System.Linq;
using MasterMemory.Tests.TestStructures;

namespace MasterMemory.Tests;

public class RelationTest
{
    private MemoryDatabase PrepareDatabase()
    {
        var builder = new DatabaseBuilder();
        builder.Append(new PersonRelation[]
        {
            new (){ PersonId = 0, Name = "Dana Terry" },
            new (){ PersonId = 1, Name = "Kirk Obrien" },
            new (){ PersonId = 2, Name = "Wm Banks" },
            new (){ PersonId = 3, Name = "Karl Benson" },
            new (){ PersonId = 4, Name = "Jared Holland" },
            new (){ PersonId = 5, Name = "Jeanne Phelps" },
            new (){ PersonId = 6, Name = "Willie Rose" },
            new (){ PersonId = 7, Name = "Shari Gutierrez" },
            new (){ PersonId = 8, Name = "Lori Wilson" },
            new (){ PersonId = 9, Name = "Lena Ramsey" },
        });
        builder.Append(new RelationItem[]
        {
            new (){ ItemId = 1, Name = "Sword" },
            new (){ ItemId = 2, Name = "Shield" },
            new (){ ItemId = 3, Name = "Armor" },
            new (){ ItemId = 4, Name = "Spear" },
            new (){ ItemId = 5, Name = "Bow" },
            new (){ ItemId = 6, Name = "Axe" },
            new (){ ItemId = 7, Name = "Hammer" },
            new (){ ItemId = 8, Name = "Spear" },
        });
        builder.Append(new PersonRelationItem[]
        {
            new (){ PersonId = 1, ItemId = 4 },
            new (){ PersonId = 1, ItemId = 5 },
            new (){ PersonId = 1, ItemId = 6 },
            new (){ PersonId = 4, ItemId = 5 },
            new (){ PersonId = 4, ItemId = 6 },
        });

        var build = builder.Build();

        return new MemoryDatabase(build);
    }

    [Fact]
    public void GetRelation()
    {
        var db = PrepareDatabase();
        var items = db.PersonRelationTable.GetItemsFromPerson(1).ToArray();

        items.Count().ShouldBe(3);
        items.Select(x => x.Name).ShouldBe(new[] { "Spear", "Bow", "Axe" });
    }
}
