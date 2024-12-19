// See https://aka.ms/new-console-template for more information
using MasterMemory;
using MessagePack;


[assembly: MasterMemoryGeneratorOptions(
    Namespace = "",
    IsReturnNullIfKeyNotFound = true,
    PrefixClassName = "foo")]

Console.WriteLine("Hello, World!");









public enum Gender
{
    Male, Female, Unknown
}

[MemoryTable("person"), MessagePackObject(true)]
public class Person
{
    [PrimaryKey(keyOrder: 1)]
    public int PersonId { get; set; }
    [SecondaryKey(0), NonUnique]
    [SecondaryKey(2, keyOrder: 1), NonUnique]
    public int Age { get; set; }
    [SecondaryKey(1), NonUnique]
    [SecondaryKey(2, keyOrder: 0), NonUnique]
    public Gender Gender { get; set; }
    public string Name { get; set; }

    public Person() // ?
    {
    }

    public Person(int PersonId, int Age, Gender Gender, string Name)
    {
        this.PersonId = PersonId;
        this.Age = Age;
        this.Gender = Gender;
        this.Name = Name;
    }

    public override string ToString()
    {
        return $"{PersonId} {Age} {Gender} {Name}";
    }
}

[MemoryTable("monster"), MessagePackObject(true)]
public partial class Monster
{
    [PrimaryKey]
    public int MonsterId { get; private set; }
    public string Name { get; private set; }
    public int MaxHp { get; private set; }

    public Monster(int MonsterId, string Name, int MaxHp)
    {
        this.MonsterId = MonsterId;
        this.Name = Name;
        this.MaxHp = MaxHp;
    }
}

[MemoryTable("enumkeytable"), MessagePackObject(true)]
public class EnumKeyTable
{
    [PrimaryKey]
    public Gender Gender { get; set; }
}