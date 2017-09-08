MasterMemory
===
Embedded Readonly In-Memory Document Database for .NET, .NET Core and Unity

Work in progress.

Concept
---
MasterMemory's objective has two areas.

* **memory efficient**, Do not create index in PrimaryKey search, only use underlying data memory.
* **performance**, MasterMemory adopts [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp) as an internal data structure.

These features are suitable for master data management(read-heavy and less-write) on application embedded especially role-playing game. MasterMemory has better performance than any other in-memory database(300x faster than filebase SQLite and 30x faster than inmemory SQLite).

Similar concepts, [linkedin/PalDb](https://github.com/linkedin/PalDB) - an embeddable write-once key-value store written in Java.

Install
---
for .NET, .NET Core

* PM> Install-Package [MasterMemory](https://www.nuget.org/packages/MasterMemory)

for Unity, Unity packages exists on [MasterMemory/Releases](https://github.com/neuecc/MasterMemory/releases) as well. More details, please see the [Unity-Supports](https://github.com/neuecc/MasterMemory#unity-supports) section.

Features
---

* O(log n) index key search
* allows multikey index
* zero index memory space for primary key
* dynamic secondary key index
* closest search
* lightweight range-view
* ILookup/IDictionary view

Quick Start
---
MasterMemory usually uses two classes, `Memory<TKey, TElement>` represents collection holder of document like `IDictionary<TKey, TValue>`, `ILookup<TKey, TElement>`. `Database` represents collection of Memory.

```csharp
public enum Gender
{
    Male, Female
}

// Document class must be MessagePackObject.
[MessagePackObject]
public class Person
{
    [Key(0)]
    public virtual int Id { get; set; }
    [Key(1)]
    public virtual int Age { get; set; }
    [Key(2)]
    public virtual Gender Gender { get; set; }
    [Key(3)]
    public virtual string Name { get; set; }
}
```

Memory is readonly-collection(represents database table), can instantiate from `IEnumerable<T>` and primary key selector.

```csharp
var sampleData = new[]
{
    new Person { Id = 0, Age = 13, Gender = Gender.Male,   Name = "Dana Terry" },
    new Person { Id = 1, Age = 17, Gender = Gender.Male,   Name = "Kirk Obrien" },
    new Person { Id = 2, Age = 31, Gender = Gender.Male,   Name = "Wm Banks" },
    new Person { Id = 3, Age = 44, Gender = Gender.Male,   Name = "Karl Benson" },
    new Person { Id = 4, Age = 23, Gender = Gender.Male,   Name = "Jared Holland" },
    new Person { Id = 5, Age = 27, Gender = Gender.Female, Name = "Jeanne Phelps" },
    new Person { Id = 6, Age = 25, Gender = Gender.Female, Name = "Willie Rose" },
    new Person { Id = 7, Age = 11, Gender = Gender.Female, Name = "Shari Gutierrez" },
    new Person { Id = 8, Age = 63, Gender = Gender.Female, Name = "Lori Wilson" },
    new Person { Id = 9, Age = 34, Gender = Gender.Female, Name = "Lena Ramsey" },
};

// Find(query unique key)
{
    // Memory is like Dictionary<TKey, TValue>
    var byId = new Memory<int, Person>(sampleData, x => x.Id);

    var id5 = byId.Find(5);
    Console.WriteLine(id5.Name); // Jeanne Phelps
}

// FindMany(query index key)
{
    // Memory is also like ILookup<TKey, TElement>
    var byGender = new Memory<Gender, Person>(sampleData, x => x.Gender);

    var females = byGender.FindMany(Gender.Female);
    foreach (var item in females) Console.WriteLine(item.Id); // 5, 6, 7, 8, 9(order is not guranteed).
}

// Multi key index
{
    var byGenderAndAge = new Memory<MemoryKey<Gender, int>, Person>(sampleData, x => MemoryKey.Create(x.Gender, x.Age));

    var maleNearAge30 = byGenderAndAge.FindClosest(Gender.Male, 35, selectLower: true);
    Console.WriteLine(maleNearAge30.Name + ":" + maleNearAge30.Age); // Wm Banks:31

    var males = byGenderAndAge.UseIndex1().FindMany(Gender.Male, ascendant: false); // use index1 only(Gender)
    foreach (var item in males) Console.WriteLine(item.Age); // 44, 31, 23, 17, 13(order is guranteed).
}
```

When handling as a database, Memory is normally not used standalone. It can create from `DatabaseBuilder` and get, save from `Database`.

```csharp
// use Database
{
    // Database is a collection of memories, which can be created from DatabaseBuilder
    var databaseBuilder = new DatabaseBuilder();

    // Add key string with data + primary key(for create memory)
    databaseBuilder.Add("person", sampleData, x => x.Id);

    // build database.
    var database = databaseBuilder.Build();

    // load memory
    var byId = database.GetMemory("person", (Person x) => x.Id);

    var id9 = byId.Find(9);
    Console.WriteLine(id9.Name); // Lena Ramsey

    // create secondary index
    var byGenderAndAge = byId.SecondaryIndex("Gender.Age", x => MemoryKey.Create(x.Gender, x.Age));

    // Typed FindMany as ILookup(or Typed Find as IDictionary by ToDictionaryView)
    var byGender = byGenderAndAge.UseIndex1().ToLookupView();
    foreach (var female in byGender[Gender.Female]) Console.WriteLine(female.Age); // 11, 25, 27, 34, 63(order is ascendant)

    // dump database, the binary can save to storage or transport on network.
    var binary = database.Save();
    File.WriteAllBytes("sampledb.db", binary);
}

// Open database from saved binary
{
    var database = Database.Open(File.ReadAllBytes("sampledb.db"));

    // and read memories...
    var memory = database.GetMemory("person", (Person x) => x.Id);

    // re-build
    var databaseBuilder = database.ToBuilder();

    databaseBuilder.Replace("person", memory.FindAll().Where(x => x.Age <= 50), x => x.Id);

    database = databaseBuilder.Build();
    File.WriteAllBytes("sampledb.db", database.Save());
}
```

Performance
---

![image](https://cloud.githubusercontent.com/assets/46207/25784661/1e0e801c-33ac-11e7-8d8a-716ddf21c38b.png)

![image](https://cloud.githubusercontent.com/assets/46207/25784668/2ae77514-33ac-11e7-84ef-56e0b6d69e97.png)

TODO....

Architecture
---

TODO...

Tips: TypedSchema
---

```csharp
// repretents schema and query of database table.
public class PersonSchema
{
    const string Key = "Person"; // unique per application.

    // in Unity, can not use IReadOnlyDictionary so use IDictionary instead.
    public readonly IReadOnlyDictionary<int, Person> ById;
    public readonly ILookup<MemoryKey<Gender, int>, Person> ByGenderAndAge;
    public readonly ILookup<Gender, Person> ByGender;

    // build database.
    public static void Build(DatabaseBuilder builder, IEnumerable<Person> persons)
    {
        builder.Add(Key, persons, x => x.Id);
    }

    // create typedschema.
    public PersonSchema(Database database)
    {
        var memory = database.GetMemory(Key, (Person x) => x.Id);
        var secondaryIndex = memory.SecondaryIndex("ByGenderAndAge", x => MemoryKey.Create(x.Gender, x.Age));

        ById = memory.ToDictionaryView();
        ByGenderAndAge = secondaryIndex.ToLookupView();
        ByGender = secondaryIndex.UseIndex1().ToLookupView();
    }
}

// wrapper of typed schema.
public class TypedDatabase
{
    // any other schemas
    public readonly PersonSchema Person;

    // datasources or datasource factory...
    public static Database Build(IEnumerable<Person> persons)
    {
        var builder = new DatabaseBuilder();
        PersonSchema.Build(builder, persons);
        return builder.Build();
    }

    public TypedDatabase(Database database)
    {
        Person = new PersonSchema(database);
    }
}
```



Unity Supports
---
MasterMemory requires [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp) as dependencies.

MasterMemory.Unity works on all platforms(PC, Android, iOS, etc...). But it can 'not' use dynamic keytuple index generation due to IL2CPP issue. But pre code generate helps it. Code Generator is located in `packages\MasterMemory.*.*.*\tools\MasterMemory.CodeGenerator.exe`, which is using [Roslyn](https://github.com/dotnet/roslyn) so analyze source code, pass the target `csproj`. 

```
arguments help:
  -i, --input=VALUE             [required]Input path of analyze csproj
  -o, --output=VALUE            [required]Output path
  -u, --unuseunityattr          [optional, default=false]Unuse UnityEngine's RuntimeInitializeOnLoadMethodAttribute on MasterMemoryInitializer
  -c, --conditionalsymbol=VALUE [optional, default=empty]conditional compiler symbol
  -n, --namespace=VALUE         [optional, default=MasterMemory]Set namespace root name
```

TODO:....

Author Info
---
Yoshifumi Kawai(a.k.a. neuecc) is a software developer in Japan.  
He is the Director/CTO at Grani, Inc.  
Grani is a top social game developer in Japan.  
He is awarding Microsoft MVP for Visual C# since 2011.  
He is known as the creator of [UniRx](http://github.com/neuecc/UniRx/)(Reactive Extensions for Unity)  

Blog: https://medium.com/@neuecc (English)  
Blog: http://neue.cc/ (Japanese)  
Twitter: https://twitter.com/neuecc (Japanese)   

License
---
This library is under the MIT License.
