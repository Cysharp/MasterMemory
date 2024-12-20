using MasterMemory;
using MessagePack;
using GeneratorSandbox;
using System.Runtime.CompilerServices;

//[assembly: MasterMemoryGeneratorOptions(
//    Namespace = "Z",
//    IsReturnNullIfKeyNotFound = true,
//    PrefixClassName = "")]

// to create database, use DatabaseBuilder and Append method.
var builder = new DatabaseBuilder();
builder.Append(new Person[]
{
    new (){ PersonId = 0, Age = 13, Gender = Gender.Male,   Name = "Dana Terry" },
    new (){ PersonId = 1, Age = 17, Gender = Gender.Male,   Name = "Kirk Obrien" },
    new (){ PersonId = 2, Age = 31, Gender = Gender.Male,   Name = "Wm Banks" },
    new (){ PersonId = 3, Age = 44, Gender = Gender.Male,   Name = "Karl Benson" },
    new (){ PersonId = 4, Age = 23, Gender = Gender.Male,   Name = "Jared Holland" },
    new (){ PersonId = 5, Age = 27, Gender = Gender.Female, Name = "Jeanne Phelps" },
    new (){ PersonId = 6, Age = 25, Gender = Gender.Female, Name = "Willie Rose" },
    new (){ PersonId = 7, Age = 11, Gender = Gender.Female, Name = "Shari Gutierrez" },
    new (){ PersonId = 8, Age = 63, Gender = Gender.Female, Name = "Lori Wilson" },
    new (){ PersonId = 9, Age = 34, Gender = Gender.Female, Name = "Lena Ramsey" },
});

// build database binary(you can also use `WriteToStream` for save to file).
byte[] data = builder.Build();


var db = new MemoryDatabase(data);

// .PersonTable.FindByPersonId is fully typed by code-generation.
Person person = db.PersonTable.FindByPersonId(5);

// Multiple key is also typed(***And * **), Return value is multiple if key is marked with `NonUnique`.
RangeView<Person> result = db.PersonTable.FindByGenderAndAge((Gender.Female, 23));

// Get nearest value(choose lower(default) or higher).
RangeView<Person> age1 = db.PersonTable.FindClosestByAge(31);

// Get range(min-max inclusive).
RangeView<Person> age2 = db.PersonTable.FindRangeByAge(20, 29);


public enum Gender
{
    Male, Female, Unknown
}



// table definition marked by MemoryTableAttribute.
// database-table must be serializable by MessagePack-CSsharp
[MemoryTable("person"), MessagePackObject(true)]
public record Person
{
    // index definition by attributes.
    [PrimaryKey]
    public required int PersonId { get; init; }

    // secondary index can add multiple(discriminated by index-number).
    [SecondaryKey(0), NonUnique]
    [SecondaryKey(1, keyOrder: 1), NonUnique]
    public required int Age { get; init; }

    [SecondaryKey(2), NonUnique]
    [SecondaryKey(1, keyOrder: 0), NonUnique]
    public required Gender Gender { get; init; }

    public required string Name { get; init; }
}