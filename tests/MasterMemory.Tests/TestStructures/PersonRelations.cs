using System.Collections.Generic;
using MasterMemory.Tests.TestStructures;
using MessagePack;

namespace MasterMemory.Tests.TestStructures
{

    [MemoryTable("person_rel"), MessagePackObject(true)]
    public record PersonRelation
    {
        // index definition by attributes.
        [PrimaryKey] public int PersonId { get; set; }

        public string Name { get; set; }
    }

    [MemoryTable("person_rel_item"), MessagePackObject(true)]
    public record PersonRelationItem
    {
        [PrimaryKey, NonUnique]
        [SecondaryKey(0), NonUnique]
        public int PersonId { get; set; }

        [PrimaryKey, NonUnique]
        [SecondaryKey(1), NonUnique]
        public int ItemId { get; set; }
    }

    [MemoryTable("relation_item"), MessagePackObject(true)]
    public record RelationItem
    {
        [PrimaryKey] public int ItemId { get; set; }

        public string Name { get; set; }
    }
}

namespace MasterMemory.Tests.Tables
{

    public sealed partial class PersonRelationTable
    {
        private MemoryDatabase _database;

        partial void OnAfterConstruct(MemoryDatabaseBase db)
        {
            _database = (MemoryDatabase)db;
        }

        public IEnumerable<RelationItem> GetItemsFromPerson(int personId)
        {
            foreach (var item in _database.PersonRelationItemTable.FindByPersonId(personId))
            {
                yield return _database.RelationItemTable.FindByItemId(item.ItemId);
            }
        }
    }
}
