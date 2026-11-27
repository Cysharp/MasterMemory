[![GitHub Actions](https://github.com/Cysharp/MasterMemory/workflows/Build-Debug/badge.svg)](https://github.com/Cysharp/MasterMemory/actions) [![Releases](https://img.shields.io/github/release/Cysharp/MasterMemory.svg)](https://github.com/Cysharp/MasterMemory/releases)

MasterMemory
===
Source Generator based Embedded Typed Readonly In-Memory Document Database for .NET and Unity. 

![image](https://user-images.githubusercontent.com/46207/61031896-61890800-a3fb-11e9-86b7-84c821d347a4.png)

A lookup by primary key allocates nothing and, in a table of 10,000 rows, takes about 40 nanoseconds: about **75** times faster than SQLite queried the usual way, with a command created for every query, and still about **25** times faster than SQLite with a prepared statement that is kept and reused (SQLite in memory, both sides returning the row as an object; SQLite allocates 1.3 to 2.3 KB for every row it returns). When the ids are consecutive (1, 2, 3, ...) MasterMemory reads the row at its position without a search, in about a nanosecond. The DB size is small, too: for the same 10,000 rows SQLite is 4,196 KB and MasterMemory is 4,078 KB, the size of the rows as MessagePack, or 1,823 KB with LZ4 plugged in (see "Compression"). The benchmark is in [sandbox/Benchmark](sandbox/Benchmark).

Source Generator automatically generates a typed database structure from schemas (classes), which ensures that all queries are type-safe with full autocompletion support.

![image](https://github.com/user-attachments/assets/e804fa52-f6a5-4972-a510-0b3b17a31230)

![image](https://user-images.githubusercontent.com/46207/61035808-cb58e000-a402-11e9-9209-d51665d1cd56.png)

This ensures both optimal performance and excellent usability.

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Concept](#concept)
- [Getting Started(.NET)](#getting-startednet)
- [Getting Started(Unity)](#getting-startedunity)
- [DataTable configuration](#datatable-configuration)
- [MemoryDatabase/RangeView](#memorydatabaserangeview)
- [Extend Table](#extend-table)
- [DatabaseBuilder](#databasebuilder)
- [Validator](#validator)
- [Metadata](#metadata)
- [Inheritance](#inheritance)
- [Optimization](#optimization)
- [Compression](#compression)
- [Native AOT](#native-aot)
- [MasterMemoryGeneratorOptions](#mastermemorygeneratoroptions)
- [v3 -> v4 migration](#v3---v4-migration)
- [v2 -> v3 migration](#v2---v3-migration)
- [License](#license)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

Concept
---

* **Memory Efficient**, Only use underlying data memory and do aggressively string interning.
* **Performance**, Similar as dictionary lookup.
* **TypeSafe**, 100% Type safe by Source Generator.
* **Fast load speed**,  MasterMemory save data by [MessagePack for C#, a fastest C# serializer](https://github.com/neuecc/MessagePack-CSharp) so load speed is blazing fast.
* **Flexible Search**, Supports multiple key, multiple result, range/closest query.
* **Validator**, You can define custom data validation by C#.
* **Metadata**, To make custom importer/exporter, get the all database metadata.

These features are suitable for master data management(write-once, read-heavy) on embedded application, data analysis, game, etc. MasterMemory has better performance than any other database solutions. [PalDB](https://github.com/linkedin/PalDB) developed by LinkedIn has a similar concept(embeddable write-once key-value store), but the implementation and performance characteristics are completely different.

Getting Started(.NET)
---
Install the [MasterMemory](https://www.nuget.org/packages/MasterMemory) library(Runtime, Source Generator(Analyzer) via NuGet.

```
dotnet add package MasterMemory
```

Prepare the example table definition like following.

```csharp
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
```

Data in MasterMemory is readonly, so it is recommended to use an immutable structure. While both records and classes are supported, records might be preferable as they generate more readable ToString methods.

MasterMemory's Source Generator detects types marked with the `MemoryTable` attribute and automatically generates types like the following:

![image](https://github.com/user-attachments/assets/e804fa52-f6a5-4972-a510-0b3b17a31230)

Finally, you can regsiter and query by these files.

```csharp
using ...; // Your project default namespace

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

// -----------------------

// for query phase, create MemoryDatabase.
// (MemoryDatabase is recommended to store in singleton container(static field/DI)).
var db = new MemoryDatabase(data);

// .PersonTable.FindByPersonId is fully typed by code-generation.
Person person = db.PersonTable.FindByPersonId(5);

// Multiple key is also typed(***And * **), Return value is multiple if key is marked with `NonUnique`.
RangeView<Person> result = db.PersonTable.FindByGenderAndAge(Gender.Female, 23);

// Get nearest value(choose lower(default) or higher).
RangeView<Person> age1 = db.PersonTable.FindClosestByAge(31);

// Get range(min-max inclusive).
RangeView<Person> age2 = db.PersonTable.FindRangeByAge(20, 29);
```

All table(marked by `MemoryTableAttribute`) and methods(created by `PrimaryKeyAttribute` or `SecondaryKeyAttribute`) are typed.

![image](https://user-images.githubusercontent.com/46207/61035808-cb58e000-a402-11e9-9209-d51665d1cd56.png)

You can invoke all indexed query by IntelliSense.

Getting Started(Unity)
---
The minimum supported Unity version will be `2022.3.12f1`, as it is necessary to support C# Incremental Source Generator(Compiler Version, 4.3.0).

Since this library is provided via NuGet, install [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity), then navigate to Open Window from NuGet -> Manage NuGet Packages, Search "MasterMemory" and Press Install.

First, it is recommended to define assembly attributes in any cs file to enable the use of `init`.

```csharp
// Optional: Unity can't load default namespace to Source Generator
// If not specified, 'MasterMemory' will be used by default,
// but you can use this attribute if you want to specify a different namespace.
[assembly: MasterMemoryGeneratorOptions(Namespace = "MyProj")]

// Optional: If you want to use init keyword, copy-and-paste this.
namespace System.Runtime.CompilerServices
{
    internal sealed class IsExternalInit { }
}
```

Everything else is the same as the standard .NET version. While the `required` keyword can't be used since it's from C# 11, using `init` alone is sufficient to guarantee immutability.

```csharp
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
    public int PersonId { get; init; }

    // secondary index can add multiple(discriminated by index-number).
    [SecondaryKey(0), NonUnique]
    [SecondaryKey(1, keyOrder: 1), NonUnique]
    public int Age { get; init; }

    [SecondaryKey(2), NonUnique]
    [SecondaryKey(1, keyOrder: 0), NonUnique]
    public Gender Gender { get; init; }

    public string Name { get; init; }
}
```

No serializer setup is needed for IL2CPP: the generated `MemoryDatabase` and `DatabaseBuilder` register the formatter of every table themselves, and by default they run on `MessagePackFormatterFactory.DefaultAot`, which never creates a generic formatter at run time. The table types themselves are served by the MessagePack for C# source generator, which runs in every assembly that references MessagePack.

If your tables contain types that need their own formatters (the `UnityEngine` value types, for example), pass the factory that serves them:

```csharp
// Vector3 and friends: the MessagePack.Unity package
var options = new MemoryDatabaseOptions { FormatterFactory = MessagePackFormatterFactory.DefaultAot.WithUnity() };

var bin = new DatabaseBuilder(options).Append(data).Build();
var db = new MemoryDatabase(bin, options);
```

Nothing is compressed by default, and MasterMemory itself has no native library. If you plug in LZ4 (see "Compression"), the `MessagePack.LZ4` package uses the native libraries of [NativeCompressions](https://github.com/Cysharp/NativeCompressions): in Unity it needs the NuGetForUnity runtime settings and, for iOS, the `NativeCompressions.Unity` editor package, as described in the [NativeCompressions README](https://github.com/Cysharp/NativeCompressions#unity).

DataTable configuration
---
Element type of datatable must be marked by `[MemoryTable(tableName)]`, datatable is generated from marked type. `string tableName` is saved in database binary, you can rename class name if tableName is same.

`[PrimaryKey(keyOrder = 0)]`, `[SecondaryKey(indexNo, keyOrder)]`, `[NonUnique]` can add to public property, `[PrimaryKey]` must use in MemoryTable, `[SecondaryKey]` is option.

Both `PrimaryKey` and `SecondaryKey` can add to multiple properties, it will be generated `***And***And***...`. `keyOrder` is order of column names, default is zero(sequential in which they appear).

A key can be declared wherever the property is: in the class itself, in a base class, in another part of a `partial` class, or on a parameter of a positional record as `[property: PrimaryKey]`. Properties of one key that are declared in different files of a partial class have no "order in which they appear", so they need an explicit `keyOrder` (error `MAM006` otherwise). A table class may be `internal`; the generated members that have it in their signature are then `internal` as well.

```csharp
[MemoryTable("sample"), MessagePackObject(true)]
public class Sample
{
    [PrimaryKey]
    public int Foo { get; set; }
    [PrimaryKey]
    public int Bar { get; set; }
}

db.Sample.FindByFooAndBar(int Foo, int Bar)

// ----

[MemoryTable("sample"), MessagePackObject(true)]
public class Sample
{
    [PrimaryKey(keyOrder: 1)]
    public int Foo { get; set; }
    [PrimaryKey(keyOrder: 0)]
    public int Bar { get; set; }
}

db.Sample.FindByBarAndFoo(int Bar, int Foo)
```

A composite key is passed member by member, in the order of the key. A range takes the members of `min` and then the members of `max`:

```csharp
Sample a = db.SampleTable.FindByBarAndFoo(bar, foo);
bool found = db.SampleTable.TryFindByBarAndFoo(bar, foo, out Sample b);
Sample? c = db.SampleTable.FindClosestByBarAndFoo(bar, foo, selectLower: true);
RangeView<Sample> d = db.SampleTable.FindRangeByBarAndFoo(minBar, minFoo, maxBar, maxFoo);
```

The head of a composite key can be looked up on its own. An index sorted by `(Gender, Age)` is sorted by `Gender` too, so it answers `FindByGender` and `FindRangeByGender` without a second index being built and kept in memory:

```csharp
[MemoryTable("person"), MessagePackObject(true)]
public class Person
{
    [PrimaryKey]
    public int PersonId { get; set; }
    [SecondaryKey(0, keyOrder: 0), NonUnique]
    public Gender Gender { get; set; }
    [SecondaryKey(0, keyOrder: 1), NonUnique]
    public int Age { get; set; }
}

RangeView<Person> a = db.PersonTable.FindByGenderAndAge(Gender.Female, 23);
RangeView<Person> b = db.PersonTable.FindByGender(Gender.Female);                  // every Female, in order of Age
RangeView<Person> c = db.PersonTable.FindRangeByGender(Gender.Male, Gender.Female);
```

* Every head gets these two methods: of a key `(A, B, C)`, both `FindByA(A)` and `FindByAAndB(A, B)`
* They return `RangeView<T>` whether or not the whole key is unique, and the rows come in the order of the whole key
* A key declared on exactly those members is what answers instead, as before (`[SecondaryKey(1)] Gender` keeps its own `FindByGender`). If you declared such a key only to look up by `Gender`, you can remove it and save its index
* When several composite keys start with the same members, the primary key answers, and otherwise the secondary key with the lowest index number. The rows are the same whichever answers; only their order differs

By default, `FindBy***` returns a single element and throws `KeyNotFoundException` if the key is not found. Keys are unique by default. To return `null` for a missing unique key instead, set `IsReturnNullIfKeyNotFound = true` in `[MasterMemoryGeneratorOptions]`. If marked with `[NonUnique]` in the same AttributeList, the return type is `RangeView<T>` (empty if not found). The rows of one key come in the order of the primary key; rows that share a `[NonUnique]` primary key come in the order they were given to the builder.

```csharp
[MemoryTable("sample"), MessagePackObject(true)]
public class Sample
{
    [PrimaryKey, NonUnique]
    public int Foo { get; set; }
    [PrimaryKey, NonUnique]
    public int Bar { get; set; }
}

RangeView<Sample> q = db.Sample.FindByFooAndBar(int Foo, int Bar)
```

```csharp
[MemoryTable("sample"), MessagePackObject(true)]
public class Sample
{
    [PrimaryKey]
    [SecondaryKey(0)]
    public int Foo { get; set; }
    [SecondaryKey(0)]
    [SecondaryKey(1)]
    public int Bar { get; set; }
}

db.Sample.FindByFoo(int Foo)
db.Sample.FindByFooAndBar(int Foo, int Bar)
db.Sample.FindByBar(int Bar)
```

`[StringComparisonOption]` allow to configure how compare if key is string. Default is `Ordinal`.

```csharp
[MemoryTable("sample"), MessagePackObject(true)]
public class Sample
{
    [PrimaryKey]
    [StringComparisonOption(StringComparison.InvariantCultureIgnoreCase)]
    public string Foo { get; set; }
}
```

The option belongs to the property: it holds in every key the property is a member of, a composite key included, and it may be written in any attribute list of the property. A composite key is compared member by member, each string by its own option:

```csharp
[MemoryTable("sample"), MessagePackObject(true)]
public class Sample
{
    [PrimaryKey(0), StringComparisonOption(StringComparison.OrdinalIgnoreCase)]
    public string Foo { get; set; }   // ignoring case
    [PrimaryKey(1)]
    public string Bar { get; set; }   // ordinal
}
```

A table is stored in the database binary in the order of its primary key, and the same order is used to search it when it is loaded. `Ordinal` and `OrdinalIgnoreCase` are the same on every machine; the culture options depend on the culture and its data on the machine that builds and on the device that loads, so prefer the ordinal ones for a primary key.

If computation property exists, add `[IgnoreMember]` of MessagePack should mark.

```csharp
[MemoryTable("person"), MessagePackObject(true)]
public class Person
{
    [PrimaryKey]
    public int Id { get;}

    public string FirstName { get; }
    public string LastName { get; }

    [IgnoreMember]
    public string FullName => FirstName + LastName;
}
```

MemoryDatabase/RangeView
---
In default, `MemoryDatabase` do all string data automatically interning(see: [Wikipedia/String interning](https://en.wikipedia.org/wiki/String_interning)). If multiple same string value exists in database(ex: "goblin","goblin", "goblin", "goblin", "goblin"....), standard database creates string value per query or store multiple same values. But MasterMemory stores single string value reference, it can save much memory if data is denormalized.

The strings are interned per database: a string is one object within a database, wherever it occurs (in two rows, in two tables). The pool that finds it is made for the load and gone with it, so nothing but the database keeps its strings alive, and a database that is dropped takes them with it. Interning is done by [MessagePack for C#'s `StringInterningFormatterFactory`](https://github.com/MessagePack-CSharp/MessagePack-CSharp); on .NET 9 and later a string the pool already has is found by its characters and not allocated a second time, so interning also makes loading a database with repeated values cheaper.

How a database is read is said by `MemoryDatabaseOptions`, which is optional: `new MemoryDatabase(bin)` reads with `MemoryDatabaseOptions.Default`.

```csharp
public sealed record MemoryDatabaseOptions
{
    public bool InternString { get; init; } = true;
    public bool ShareInternedStrings { get; init; } = false;
    public MessagePackFormatterFactory? FormatterFactory { get; init; } = null;   // null: MessagePackFormatterFactory.DefaultAot
    public MessagePackMessageProcessor? MessageProcessor { get; init; } = null;   // null: the tables are not compressed
}

// make the options once, and give the same ones to the database and to the builder
static readonly MemoryDatabaseOptions options = new MemoryDatabaseOptions { InternString = false };

var db = new MemoryDatabase(bin, options);
var builder = new DatabaseBuilder(options);
```

* `InternString`: if you want to disable automatically interning, set it to false.
* `ShareInternedStrings`: by default each database has a pool of its own, as said above. Set to true, the pool is the options' own, and every database that is read with those options shares it: a database that is loaded again while the application runs (master data that is reloaded) gets the string objects of the one before it for every string that did not change. In return a string stays alive as long as the options do, also when no database has it any more.
* `FormatterFactory` supplies the formatters of the table types and their members. The default is `MessagePackFormatterFactory.DefaultAot` (source-generated and built-in formatters only, the same on the JIT as under IL2CPP and Native AOT); give `MessagePackFormatterFactory.Default` for types that only the reflection-based formatters can serve, or your own chain for custom formatters. Interning works with any of them: MasterMemory puts it in front of the factory you give.
* `MessageProcessor` is what the tables are compressed with; the default leaves them as they are, see "Compression".

The options hold what MessagePack has made for the formatters, which is why they are worth making once and keeping: a second builder with the same options does not make it again, and neither does a second database where strings are not interned or the pool is shared (a pool of its own is made for each database, and the formatters with it, which costs a kilobyte or two). A changed copy is made with `with` (`options with { MessageProcessor = lz4 }`) and starts on its own. `db.Options` and `builder.Options` are the options a database or a builder has.

`MemoryDatabase(byte[] databaseBinary, MemoryDatabaseOptions options = null, int maxDegreeOfParallelism = 1)`.

The database binary need not be an array of its own. There is a constructor for `ReadOnlyMemory<byte>` with the same parameters, and one for `ReadOnlySpan<byte>` without `maxDegreeOfParallelism` (reading tables at the same time hands the binary to other threads, which a span cannot do):

```csharp
// a part of a rented buffer
var buffer = ArrayPool<byte>.Shared.Rent(length);
stream.ReadExactly(buffer, 0, length);
var db = new MemoryDatabase(buffer.AsMemory(0, length), maxDegreeOfParallelism: Environment.ProcessorCount);
ArrayPool<byte>.Shared.Return(buffer);

// Unity: the bytes of a TextAsset without the copy that TextAsset.bytes makes
var db = new MemoryDatabase(textAsset.GetData<byte>().AsReadOnlySpan());
```

The binary is read until the constructor returns and is not kept, so the buffer can be reused or released right after.

MemoryDatabase has three(or four) query methods.

* `T|RangeView<T>` FindBy***(TKey key)
* bool TryFindBy***(TKey key, out T result)
* `T|RangeView<T>` FindClosestBy***(TKey key, bool selectLower = true)
* `RangeView<T>` FindRangeBy***(TKey min, TKey max)

If index key is unique, generates `FindBy***` and `TryFindBy***` methods and then `FindBy***` throws `KeyNotFoundException` when key is not found.

`By***` is generated by `PrimaryKey` and `SecondaryKey` defines.

And has some utility properties.

* `int` Count
* `RangeView<T>` All
* `ReverseRangeView<T>` AllReverse
* `RangeView<T>` SortBy***
* `T[] GetRawDataUnsafe()`

`struct RangeView<T>` is the view of database elements: rows that lie together in an index, in the order of that index. It is a window on the array of the index and copies nothing. It has following property/method.

* `T` [int index]
* `int` Count
* `T` First
* `T` Last
* `T?` FirstOrDefault()
* `T?` LastOrDefault()
* `bool` Any()
* `ReadOnlySpan<T>` AsSpan()
* `T[]` ToArray()
* `IReadOnlyList<T>` AsReadOnlyList()
* `ReverseRangeView<T>` Reverse
* `Enumerator` GetEnumerator()

`Reverse` is the same rows from the last to the first. It is a type of its own, `struct ReverseRangeView<T>`, with the same members except `AsSpan()` (a span runs one way only), and its `Reverse` is the `RangeView<T>` again. `AllReverse` of a table is one, too.

```csharp
RangeView<Person> twenties = db.PersonTable.FindRangeByAge(20, 29);   // in order of Age
ReverseRangeView<Person> oldestFirst = twenties.Reverse;              // the same rows, the other way
ReadOnlySpan<Person> span = twenties.AsSpan();                        // the rows as a span of the index
Person? youngest = twenties.FirstOrDefault();                         // null when there is none

foreach (var person in twenties) { }                                  // no allocation

// LINQ, and anything else that takes an IEnumerable<T>, goes through AsReadOnlyList()
string[] names = twenties.AsReadOnlyList().Where(x => x.Gender == Gender.Female).Select(x => x.Name).ToArray();
```

`foreach`, the indexer and the members above do not allocate (`ToArray()` allocates its array, `AsReadOnlyList()` one small object).

A view implements no interface, `IEnumerable<T>` included. A struct that is read through an interface is boxed, and LINQ over it allocates on top of that; if a view were an `IEnumerable<T>`, a `Where` written after a lookup would do both without a sign of it in the code. So LINQ does not compile on a view, and `AsReadOnlyList()` is how to say that a list is wanted: it allocates one object, copies no row, and what it returns cannot be changed. For a loop that has to stay free of allocation, read the view with `foreach` or as a span.

Extend Table
---
Generated table class is defined partial class so create same namespace and class name's partial class on another file, you can add your custom method to generated table.

Table class also defined partial `OnAfterConstruct` method, it called after table has been constructed. You can use it to store custom data to field after all data has been constructed.

```csharp
// create MonsterTable.Partial.cs

public sealed partial class MonsterTable
{
    int maxHp;
#pragma warning disable CS0649
    readonly int minHp;
#pragma warning restore CS0649    

    // called after constructed
    partial void OnAfterConstruct()
    {
        maxHp = All.AsReadOnlyList().Select(x => x.MaxHp).Max();
        // you can use Unsafe.AsRef to set readonly field
        Unsafe.AsRef(minHp) = All.AsReadOnlyList().Select(x => x.MaxHp).Min();
    }
    
    // add custom method other than standard Find method
    public IEnumerable<Monster> GetRangedMonster(int arg1)
    {
        return All.AsReadOnlyList().Where....();
    }
}
```

DatabaseBuilder
---
`DatabaseBuilder` holds tables as rows. Rows go in with `Append`, and what comes out is the database binary (`Build`, `WriteToStream`) or the database itself (`ToMemoryDatabase`).

```csharp
var builder = new DatabaseBuilder()
    .Append(persons)
    .Append(items);

// the database binary, to save and to ship
byte[] bin = builder.Build();

// or the database, with no binary in between
MemoryDatabase db = builder.ToMemoryDatabase();
```

`ToMemoryDatabase` serializes nothing, which makes it the short way for tests, tools and editor code, for example to `Validate()` rows before their binary is written. Its rows are the objects that were appended: strings are not interned, and what the serializer checks when it reads a row is not checked. An application should load the binary it ships.

If you want to add/modify data to loaded database, make a builder from it with `ToDatabaseBuilder`. The builder starts with every table of the database, and the database itself is never changed.

```csharp
// Create DatabaseBuilder from original database.
var builder = db.ToDatabaseBuilder();

// Add Or Replace compare with PrimaryKey
builder.AddOrReplace(addOrReplaceData);

// Remove by PrimaryKey
builder.RemovePerson(1, 10, 100);

// Replace all data
builder.ReplaceAll(newData);

// Finally create new database, tables that were not changed are shared with the original
MemoryDatabase newDatabase = builder.ToMemoryDatabase();

// If you want to save new database, the same builder writes it
var newBinary = builder.Build(); // or use WriteToStream
```

* `Append` is for a table the builder does not have yet and throws for one it has, so that two sources of one table do not go unnoticed. A builder from `ToDatabaseBuilder` has every table: use `ReplaceAll` there.
* `AddOrReplace` and `Remove***` are generated for tables whose primary key is unique. `AddOrReplace` takes its rows in order: of two rows with one key, the later one stays.
* `Remove***` takes a composite key as a tuple: `builder.RemoveSkill((1, 2), (3, 1))`.
* A change is one pass over the rows of its table. The indexes of a changed table are built when a database is made from the builder, once, however many changes there were.
* A builder from `ToDatabaseBuilder` writes with the options the database was read with (`db.Options`): the same formatters, and compressed if the database was. `db.ToDatabaseBuilder(options)` writes with others.

MemoryDatabase's reference can use as snapshot.

```csharp
// 1 game per 1 instance
public class GameRoom
{
    MemoryDatabase database;

    // The reference is a snapshot of the timing of game begins.
    public GameRoom(MemoryDatabase database)
    {
        this.database = database;
    }
}
```

Validator
---
You can validate data by `MemoryDatabase.Validate` method. In default, it check unique key(data duplicated) and you can define custom validate logics.

```csharp
// Implements IValidatable<T> to targeted validation
[MemoryTable("quest_master"), MessagePackObject(true)]
public class Quest : IValidatable<Quest>
{
    // If index is Unique, validate duplicate in default.
    [PrimaryKey]
    public int Id { get; }
    public string Name { get; }
    public int RewardId { get; }
    public int Cost { get; }

    void IValidatable<Quest>.Validate(IValidator<Quest> validator)
    {
        // get the external reference table
        var items = validator.GetReferenceSet<Item>();

        // Custom if logics.
        if (this.RewardId > 0)
        {
            // RewardId must exists in Item.ItemId
            items.Exists(x => x.RewardId, x => x.ItemId);
        }

        // Range check, Cost must be 10..20
        validator.Validate(x => x.Cost >= 10);
        validator.Validate(x => x.Cost <= 20);

        // In this region, only called once so enable to validate overall of tables.
        if (validator.CallOnce())
        {
            var quests = validator.GetTableSet();
            // Check unique othe than index property.
            quests.Where(x => x.RewardId != 0).Unique(x => x.RewardId);
        }
    }
}

[MemoryTable("item_master"), MessagePackObject(true)]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; }
}

void Main()
{
    var db = new MemoryDatabase(bin);

    // Get the validate result.
    var validateResult = db.Validate();
    if (validateResult.IsValidationFailed)
    {
        // Output string format.
        Console.WriteLine(validateResult.FormatFailedResults());

        // Get the raw FaildItem[]. (.Type, .Message, .Data)
        // validateResult.FailedResults
    }
}
```

Following is list of validation methods.

```csharp
// all void methods are assert function, it stores message to ValidateResult if failed.
interface IValidator<T>
{
    ValidatableSet<T> GetTableSet();
    ReferenceSet<T, TRef> GetReferenceSet<TRef>();
    void Validate(Expression<Func<T, bool>> predicate);
    void Validate(Func<T, bool> predicate, string message);
    void ValidateAction(Expression<Func<bool>> predicate);
    void ValidateAction(Func<bool> predicate, string message);
    void Fail(string message);
    bool CallOnce();
}

class ReferenceSet<TElement, TReference>
{
    IReadOnlyList<TReference> TableData { get; }
    void Exists<TProperty>(Expression<Func<TElement, TProperty>> elementSelector, Expression<Func<TReference, TProperty>> referenceElementSelector);
    void Exists<TProperty>(Expression<Func<TElement, TProperty>> elementSelector, Expression<Func<TReference, TProperty>> referenceElementSelector, EqualityComparer<TProperty> equalityComparer);
}

class ValidatableSet<TElement>
{
    IReadOnlyList<TElement> TableData { get; }
    void Unique<TProperty>(Expression<Func<TElement, TProperty>> selector);
    void Unique<TProperty>(Expression<Func<TElement, TProperty>> selector, IEqualityComparer<TProperty> equalityComparer);
    void Unique<TProperty>(Func<TElement, TProperty> selector, string message);
    void Unique<TProperty>(Func<TElement, TProperty> selector, IEqualityComparer<TProperty> equalityComparer, string message);
    void Sequential(Expression<Func<TElement, SByte|Int16|Int32|...>> selector, bool distinct = false);
    ValidatableSet<TElement> Where(Func<TElement, bool> predicate);
}
```

Metadata
---
You can get the table-info, properties, indexes by metadata api. It helps to make custom importer/exporter application.

```csharp
var metaDb = MemoryDatabase.GetMetaDatabase();
foreach (var table in metaDb.GetTableInfos())
{
    // for example, generate CSV header
    var sb = new StringBuilder();
    foreach (var prop in table.Properties)
    {
        if (sb.Length != 0) sb.Append(",");

        // Name can convert to LowerCamelCase or SnakeCase.
        sb.Append(prop.NameSnakeCase);
    }
    File.WriteAllText(table.TableName + ".csv", sb.ToString(), new UTF8Encoding(false));
}
```

If creates console-app, our [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework/) can easy to make helper applications.

Here is sample of reading and creating dynamic from csv. `builder.AppendDynamic` and `System.Runtime.Serialization.FormatterServices.GetUninitializedObject` will help it.

```csharp
var csv = @"monster_id,name,max_hp
1,foo,100
2,bar,200";
var fileName = "monster";

var builder = new DatabaseBuilder();

var meta = MemoryDatabase.GetMetaDatabase();
var table = meta.GetTableInfo(fileName);

var tableData = new List<object>();

using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
using (var sr = new StreamReader(ms, Encoding.UTF8))
using (var reader = new TinyCsvReader(sr))
{
    while ((reader.ReadValuesWithHeader() is Dictionary<string, string> values))
    {
        // create data without call constructor
        // use System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject instead on .NET 8
        var data = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(table.DataType);

        foreach (var prop in table.Properties)
        {
            if (values.TryGetValue(prop.NameSnakeCase, out var rawValue))
            {
                var value = ParseValue(prop.PropertyInfo.PropertyType, rawValue);
                if (prop.PropertyInfo.SetMethod == null)
                {
                    throw new Exception("Target property does not exists set method. If you use {get;}, please change to { get; private set; }, Type:" + prop.PropertyInfo.DeclaringType + " Prop:" + prop.PropertyInfo.Name);
                }
                prop.PropertyInfo.SetValue(data, value);
            }
            else
            {
                throw new KeyNotFoundException($"Not found \"{prop.NameSnakeCase}\" in \"{fileName}.csv\" header.");
            }
        }

        tableData.Add(data);
    }
}

// add dynamic collection.
builder.AppendDynamic(table.DataType, tableData);

var bin = builder.Build();
var database = new MemoryDatabase(bin);

    static object ParseValue(Type type, string rawValue)
    {
        if (type == typeof(string)) return rawValue;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (string.IsNullOrWhiteSpace(rawValue)) return null;
            return ParseValue(type.GenericTypeArguments[0], rawValue);
        }

        if (type.IsEnum)
        {
            var value = Enum.Parse(type, rawValue);
            return value;
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                // True/False or 0,1
                if (int.TryParse(rawValue, out var intBool))
                {
                    return Convert.ToBoolean(intBool);
                }
                return Boolean.Parse(rawValue);
            case TypeCode.Char:
                return Char.Parse(rawValue);
            case TypeCode.SByte:
                return SByte.Parse(rawValue, CultureInfo.InvariantCulture);
            case TypeCode.Byte:
                return Byte.Parse(rawValue, CultureInfo.InvariantCulture);
            case TypeCode.Int16:
                return Int16.Parse(rawValue, CultureInfo.InvariantCulture);
            case TypeCode.UInt16:
                return UInt16.Parse(rawValue, CultureInfo.InvariantCulture);
            case TypeCode.Int32:
                return Int32.Parse(rawValue, CultureInfo.InvariantCulture);
            case TypeCode.UInt32:
                return UInt32.Parse(rawValue, CultureInfo.InvariantCulture);
            case TypeCode.Int64:
                return Int64.Parse(rawValue, CultureInfo.InvariantCulture);
            case TypeCode.UInt64:
                return UInt64.Parse(rawValue, CultureInfo.InvariantCulture);
            case TypeCode.Single:
                return Single.Parse(rawValue, CultureInfo.InvariantCulture);
            case TypeCode.Double:
                return Double.Parse(rawValue, CultureInfo.InvariantCulture);
            case TypeCode.Decimal:
                return Decimal.Parse(rawValue, CultureInfo.InvariantCulture);
            case TypeCode.DateTime:
                return DateTime.Parse(rawValue, CultureInfo.InvariantCulture);
            default:
                if (type == typeof(DateTimeOffset))
                {
                    return DateTimeOffset.Parse(rawValue, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(TimeSpan))
                {
                    return TimeSpan.Parse(rawValue, CultureInfo.InvariantCulture);
                }
                else if (type == typeof(Guid))
                {
                    return Guid.Parse(rawValue);
                }

                // or other your custom parsing.
                throw new NotSupportedException();
        }
    }

    // Non string escape, tiny reader with header.
    public class TinyCsvReader : IDisposable
    {
        static char[] trim = new[] { ' ', '\t' };

        readonly StreamReader reader;
        public IReadOnlyList<string> Header { get; private set; }

        public TinyCsvReader(StreamReader reader)
        {
            this.reader = reader;
            {
                var line = reader.ReadLine();
                if (line == null) throw new InvalidOperationException("Header is null.");

                var index = 0;
                var header = new List<string>();
                while (index < line.Length)
                {
                    var s = GetValue(line, ref index);
                    if (s.Length == 0) break;
                    header.Add(s);
                }
                this.Header = header;
            }
        }

        string GetValue(string line, ref int i)
        {
            var temp = new char[line.Length - i];
            var j = 0;
            for (; i < line.Length; i++)
            {
                if (line[i] == ',')
                {
                    i += 1;
                    break;
                }
                temp[j++] = line[i];
            }

            return new string(temp, 0, j).Trim(trim);
        }

        public string[] ReadValues()
        {
            var line = reader.ReadLine();
            if (line == null) return null;
            if (string.IsNullOrWhiteSpace(line)) return null;

            var values = new string[Header.Count];
            var lineIndex = 0;
            for (int i = 0; i < values.Length; i++)
            {
                var s = GetValue(line, ref lineIndex);
                values[i] = s;
            }
            return values;
        }

        public Dictionary<string, string> ReadValuesWithHeader()
        {
            var values = ReadValues();
            if (values == null) return null;

            var dict = new Dictionary<string, string>();
            for (int i = 0; i < values.Length; i++)
            {
                dict.Add(Header[i], values[i]);
            }

            return dict;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
```

Inheritance
---
Currently MasterMemory does not support inheritance. Recommend way to create common method, use interface and extension method. But if you want to create common method with common cached field(made by `OnAfterConstruct`), for workaround, create abstract class and all data properties to abstract.

```csharp
public abstract class FooAndBarBase
{
    // all data properties to virtual
    public virtual int Prop1 { get; protected set; }
    public virtual int Prop2 { get; protected set; }

    [IgnoreMember]
    public int Prop3 => Prop1 + Prop2;

    public IEnumerable<FooAndBarBase> CommonMethod()
    {
        throw new NotImplementedException();
    }
}

[MemoryTable("foo_table"), MessagePackObject(true)]
public class FooTable : FooAndBarBase
{
    [PrimaryKey]
    public override int Prop1 { get; protected set; }
    public override int Prop2 { get; protected set; }
}

[MemoryTable("bar_table"), MessagePackObject(true)]
public class BarTable : FooAndBarBase
{
    [PrimaryKey]
    public override int Prop1 { get; protected set; }
    public override int Prop2 { get; protected set; }
}
```

Optimization
---
When invoking `new MemoryDatabase(byte[] databaseBinary...)`, read and construct database from binary. If binary size is large then construct performance will slow down. `MemoryDatabase` has `ctor(..., int maxDegreeOfParallelism = 1)` option in constructor to construct in parallel.

```csharp
var database = new MemoryDatabase(bin, maxDegreeOfParallelism: Environment.ProcessorCount);
```

The use of Parallel can greatly improve the construct performance. Recommend to use `Environment.ProcessorCount`.

If you want to reduce code size of generated code, the Validator and the MetaDatabase can be left out of it with `GenerateValidator = false` and `GenerateMetaDatabase = false` of `[MasterMemoryGeneratorOptions]`, see below. To leave them out of one build only (the client, but not the tool that imports and validates the data), write the attribute twice, inside `#if`:

```csharp
#if UNITY_EDITOR
[assembly: MasterMemoryGeneratorOptions(Namespace = "MyProj")]
#else
[assembly: MasterMemoryGeneratorOptions(Namespace = "MyProj", GenerateMetaDatabase = false, GenerateValidator = false)]
#endif
```

The database generation/loading speed and size are affected by MessagePack's serialization format. With `[MessagePackObject(true)]` every row carries the name of every member; with `[MessagePackObject]` and `[Key(0)]`, `[Key(1)]`, ... a row is an array of its values. In a table of mostly numeric columns the names are most of the binary, so integer keys make it several times smaller and faster to load, which is more than compressing the string-keyed table gains. In a table that is mostly text (dialogue, descriptions) the keys hardly matter, and compression is what makes it smaller, see "Compression". Integer keys are numbered by hand, and a column that is added takes a new number.

Compression
---
A database binary is not compressed: a table is stored as the MessagePack bytes of its rows, and MasterMemory depends on no compression library. Where the size of the binary matters more than the time to load it, plug in a `MessagePackMessageProcessor` of MessagePack for C# with the `MessageProcessor` of the options. LZ4 is in the `MessagePack.LZ4` package:

```csharp
// dotnet add package MessagePack.LZ4
var options = new MemoryDatabaseOptions { MessageProcessor = new Lz4BlockProcessor() };

var bin = new DatabaseBuilder(options).Append(data).Build();
var db = new MemoryDatabase(bin, options);
```

* Each table is compressed by itself, and a table too small to gain from it is left as it is
* A database is read with the processor it was written with, which is what giving the same options to both sees to. A compressed database read without one throws an `InvalidOperationException` that names the table and says what to pass. The other way round works: a processor leaves a table that is not compressed as it is, so a database that is given the processor reads both
* `Lz4BlockProcessor` writes what MasterMemory v3 wrote, so this is also how a database built by v3 is read, see "v3 -> v4 migration"
* `new Lz4BlockProcessor()` refuses a table that says it decompresses to more than 64 MB, which guards against data from outside. Master data is your own: `new Lz4BlockProcessor(maxDecompressedSize)` raises the limit for a table that is larger
* A builder made by `db.ToDatabaseBuilder()` writes with the options the database was read with, so a database that was read with a processor is written compressed again. `db.ToDatabaseBuilder(MemoryDatabaseOptions.Default)` writes it as it is
* `TableInfo.DumpAsJson(options)` shows a compressed table
* `MessagePack.LZ4` uses the native libraries of [NativeCompressions](https://github.com/Cysharp/NativeCompressions); see its supported platforms, and "Getting Started(Unity)" for Unity

What compression gains depends on the data. The table of [the benchmark](sandbox/Benchmark) (10,000 rows of an id, a name and a paragraph of text) is 4,078 KB as it is and 1,823 KB with LZ4. Before reaching for it, look at how the rows are serialized: with `[MessagePackObject(true)]` every row carries the name of every member, and integer keys take that away without compressing anything, see "Optimization".

Native AOT
---
MasterMemory runs in an application that is trimmed or compiled with Native AOT (`PublishAot`), and is marked so (`IsAotCompatible`): the library and the code the source generator emits produce no trimming or AOT warning. Nothing in them looks a member up by its name or makes a type at run time.

* **Loading and lookups** read through formatters and comparisons that are generated at compile time. The default formatters are `MessagePackFormatterFactory.DefaultAot`, see "MemoryDatabase/RangeView"
* **`Validate()`** works. A check written as an expression (`validator.Validate(x => x.MaxHp > 0)`) is run by the expression interpreter, as Native AOT has no JIT to compile it with; that is slower than on the JIT and gives the same result and the same message
* **The metadata** (`GetMetaDatabase()`) works, and so does an importer that goes by it: `MetaTable.DataType` is annotated, so the constructors and public properties of every row type are kept, and making a row with `RuntimeHelpers.GetUninitializedObject(table.DataType)` and setting it through `PropertyInfo.SetValue` passes the analysis
* **`AppendDynamic`** does not use reflection: the generated `DatabaseBuilder` has a branch for each table that names its row type

What the row types need is what MessagePack for C# needs of them: a source-generated formatter, which `[MessagePackObject]` gives.

`tests/MasterMemory.Tests.Aot` is an application that exercises all of this and returns the number of checks that failed. It is how the above is verified, and a template for trying your own tables:

```
dotnet publish tests/MasterMemory.Tests.Aot -c Release -r win-x64 -o artifacts/aot
artifacts/aot/MasterMemory.Tests.Aot
```
MasterMemoryGeneratorOptions
---
The Source Generator settings are configured using the assembly attribute `[MasterMemoryGeneratorOptions]`. By placing it in any file, you can configure the following settings.

```csharp
[assembly: MasterMemoryGeneratorOptions(
    Namespace = "MyConsoleApp",
    IsReturnNullIfKeyNotFound = true,
    PrefixClassName = "Foo",
    GenerateMetaDatabase = false,
    GenerateValidator = false
)]
```

* `Namespace`: Changes the namespace of generated files. If not specified, it tries to get the `RootNamespace` set in the csproj file; if that's not available, it defaults to `MasterMemory`
* `IsReturnNullIfKeyNotFound`: By default, the `Find` method throws a `KeyNotFoundException` when a key is not found. If set to true, the return type becomes `T?` and returns null instead
* `PrefixClassName`: Adds a prefix to the class names of generated files. For example, `DatabaseBuilder` becomes `FooDatabaseBuilder`. This allows you to distinguish between multiple MasterMemory projects by name.
* `GenerateMetaDatabase`: By default (true), `MemoryDatabase.GetMetaDatabase()` and the `CreateMetaTable()` of every table are generated, see "Metadata". If set to false they are not, which makes the generated code smaller where nothing reads the metadata
* `GenerateValidator`: By default (true), `MemoryDatabase.Validate()` is generated, see "Validator". If set to false it is not, and the `Validate` methods of your `IValidatable<T>` classes are called by nothing

v3 -> v4 migration
---
MasterMemory v4 runs on [MessagePack for C# v4](https://github.com/MessagePack-CSharp/MessagePack-CSharp). The database binary format is unchanged: v4 reads a database built by v3 (with the LZ4 processor, as v4 does not compress by itself; see the second item below), and v3 reads a database built by v4. The one other thing that changed is the order some tables are stored in: those whose primary key has a string member that v3 did not compare the way it was declared (a `string?`, a string in a composite key, a `[StringComparisonOption]` that was not applied). v4 reads such a table from a database built by v3 and puts its rows in order while loading; v3 cannot read one from a database built by v4. See below.

* **How a database is read and written is in `MemoryDatabaseOptions`, and the constructors take that one object**: `new MemoryDatabase(bin, internString: false)` becomes `new MemoryDatabase(bin, new MemoryDatabaseOptions { InternString = false })`, and an `IFormatterResolver` given to the database or the builder becomes a `MessagePackFormatterFactory` in the options: `new MemoryDatabaseOptions { FormatterFactory = factory }`. The same options go to `new DatabaseBuilder(options)`, so make them once and keep them. `maxDegreeOfParallelism` is still a parameter of the constructor, and `new MemoryDatabase(bin)` and `new MemoryDatabase(bin, maxDegreeOfParallelism: n)` are unchanged. A builder made by `db.ToDatabaseBuilder()` writes with the options the database was read with. See "MemoryDatabase/RangeView"
* The generated `MasterMemoryResolver` is gone, and so is its registration (`StaticCompositeResolver`, `MessagePackSerializer.DefaultOptions`). The generated code registers the formatter of each table array with MessagePack itself, when the first `MemoryDatabase` or `DatabaseBuilder` is made, so it is found through `DefaultAot`, `Default` and any factory built around them: there is nothing of MasterMemory's to put into a chain of your own
* With no factory given, the formatters come from `MessagePackFormatterFactory.DefaultAot` instead of the process-wide default options, which v4 no longer has. A type that has no source-generated formatter (declared in an assembly built without the MessagePack package, or without attributes) needs `MessagePackFormatterFactory.Default` or `Default.WithContractless()` as the `FormatterFactory` of the options
* `TableInfo.DumpAsJson(MessagePackSerializerOptions)` became `DumpAsJson(MemoryDatabaseOptions)`: `DumpAsJson()` for a table that is not compressed, `DumpAsJson(options)` with the options that have the processor for one that is
* **Tables are not compressed unless you say so, and a database built by v3 needs the LZ4 processor to be read.** v3 compressed every table with LZ4, which MessagePack for C# v3 had built in. In MessagePack for C# v4 LZ4 is a package of its own, `MessagePack.LZ4`, with native libraries ([NativeCompressions](https://github.com/Cysharp/NativeCompressions)); MasterMemory does not reference it and writes a table as it is. To read a database built by v3, reference `MessagePack.LZ4` and give its processor in the options: `new MemoryDatabase(bin, new MemoryDatabaseOptions { MessageProcessor = new Lz4BlockProcessor() })`. Without it the constructor throws an `InvalidOperationException` that says so. A builder given the same options writes what v3 wrote, compressed. The processor also reads a table that is not compressed, so it can stay in place while the databases are rebuilt. See "Compression", also for the 64 MB a table may decompress to by default
* The table types follow MessagePack for C# v4's rules, see [Migrating from v3](https://github.com/MessagePack-CSharp/MessagePack-CSharp/tree/v4#migrating-from-v3). The ones master data meets most: in a `#nullable enable` context a `null` in a member declared non-nullable is refused on both build and load (declare it `string?`), and constructor parameters bind to members by name
* A key declared `string?` is compared like a `string` key: `StringComparer.Ordinal`, or what `[StringComparisonOption]` says. Before, it was compared by the current culture and the attribute was ignored. A table is stored in the order of its primary key, so such a table is stored in another order now; see "the order of a primary key with a string member" below. Secondary keys are sorted on load and need nothing
* A string in a composite key is compared ordinally, like a key of one string. Before, a composite key was compared by the default comparer of its tuple, which compares strings by the current culture: the machine that built a database and the device that loaded it could disagree on the order of the same rows. Such a primary key is stored in another order now, see below
* `[StringComparisonOption]` is applied wherever it is written on the property, and to a string member of a composite key. Before, it was applied only when it came after the key attribute in the same attribute list; before it, or in a list of its own as this README shows it, it was ignored and the key stayed ordinal. A primary key that gets its option only now is stored in another order, see below
* **The order of a primary key with a string member.** In the three cases above a table from a database built by v3 is not in the order v4 searches by. v4 handles it: the generated table checks the order of its rows when it is constructed (one comparison a row, and only for a primary key with a string member) and, if they are out of order, sorts a copy of them. So a database built by v3 keeps working, at the cost of that sort on every load until it is rebuilt with v4, which is still worth doing. The other direction is not covered: v3 loads a database built by v4 without an error and then misses rows of such a table in its lookups, so do not ship a database built by v4 to a client that still runs v3 if it has such a table
* Keys are read wherever the property is declared: a base class, another part of a `partial` class, a parameter of a positional record (`[property: PrimaryKey]`). Before, only the properties written in the class body that carries `[MemoryTable]` were seen
* A table class may be `internal`. The table class generated for it, and the members of `MemoryDatabase` and `DatabaseBuilder` that have it in their signature, are `internal` too
* Attributes are recognized however they are written (`[PrimaryKeyAttribute]`, `[MasterMemory.SecondaryKey(0)]`, through an alias), and their arguments may be any constant (`[SecondaryKey(Indexes.Name)]`). Key types are recognized through an alias as well, and `short`, `ushort` and `char` keys are compared directly like the other integers
* The generator no longer fails on code that does not compile yet. An attribute that is still being typed (`[SecondaryKey]` before its argument) used to take every generated type of the project with it
* New errors, for what used to be accepted and then did nothing or failed in generated code: `MAM005` two tables with one `[MemoryTable]` name, `MAM006` properties of a key in different files of a partial class without `keyOrder`, `MAM007` `[NonUnique]` in an attribute list without a key, `MAM008` `[StringComparisonOption]` on a property that is not a string key, `MAM009` a generic table class, `MAM010` a table class that is `private` or `protected`, `MAM011` a key that is static, or not readable from generated code
* `MetaTable.Properties` lists the public instance properties, those of base classes included. Before, a non-public or static property without attributes was written into the list too
* The generated `MemoryDatabase` is `partial`: members of your own, such as a query across several tables, can be added to it
* Generated code names table and key types in full (`global::MyGame.Master.Item`), so a table may be declared in a file-scoped namespace, inside another type, or in a file whose `using` directives sit inside `#if`
* Two `[MemoryTable]` classes with the same name in different namespaces are reported as error `MAM004`; before, nothing was generated at all
* With `IsReturnNullIfKeyNotFound = true` every unique `FindBy***` returns `T?`; before, only a numeric primary key did
* `FindClosestBy***(key, selectLower: false)` of a unique key returns `null` when the key is after every row; before, it threw `IndexOutOfRangeException`
* `FindRangeBy***` of a `[NonUnique]` string key no longer returns an empty range for a valid one: whether `min` is after `max` was decided by the culture, not by the ordinal order the index is in (`FindRangeByName("B", "a")`)
* `RangeView<T>.CopyTo` of an empty view does nothing instead of throwing `ArgumentNullException`
* `RangeView<T>.GetEnumerator()` returns a struct, so `foreach` over a range no longer allocates
* **A view runs one way, and its reverse is a type of its own**: `RangeView<T>` is always in the order of its index, and `Reverse` and `AllReverse` return a `ReverseRangeView<T>`. `FindRangeBy***` no longer takes `ascendant`: `FindRangeByAge(20, 29, ascendant: false)` becomes `FindRangeByAge(20, 29).Reverse`. A variable or parameter declared `RangeView<T>` that was given a reversed view becomes a `ReverseRangeView<T>` (or an `IReadOnlyList<T>`, which `AsReadOnlyList()` of both gives). The compiler points at each of these. In return a `RangeView<T>` can always be read as a span, and reading a view costs less: it no longer asks which way it runs for every row
* `RangeView<T>.AsSpan()` is new, and so are `FirstOrDefault()`, `LastOrDefault()` and `ToArray()` on both views. The methods of LINQ with these names were what a call bound to before, and they boxed the view; after a recompile the same call binds to the view's own method, which does not
* The constructor of `RangeView<T>` is `(array)` or `(array, offset, count)` and checks its arguments; it was `(array, left, right, ascendant)`
* `Validate()` finds the rows that share a unique key by the comparison of that key. Before, it compared keys by their default equality, so with `[StringComparisonOption(StringComparison.OrdinalIgnoreCase)]` the rows "abc" and "ABC" passed as two keys, while a lookup found only one of them
* Rows that share a key have an order. Those of a `[NonUnique]` secondary key come in the order of the primary key, and those of a `[NonUnique]` primary key in the order they were given to `Append` or `ReplaceAll`. Before, their order was whatever `Array.Sort` left, which nothing promised and which could differ from one runtime to another. The order of the keys themselves is unchanged, and so is a database binary whose primary key is unique
* A table with secondary indexes is made faster: measured on x64 (.NET 9) with nine indexes, in three quarters of the time up to 1,000 rows and nine tenths at 100,000, the first table of a process included. `Append` of rows that are in the order of the primary key already takes less than half the time; `AddOrReplace` of many rows in no order takes about a third more
* Lookups are faster. Every key is searched through a struct generated for it, with no delegate and no comparer object per comparison. On top of that, a unique integer key of 32 bits or less is searched without conditional jumps in tables up to 32,768 rows, and a unique numeric primary key whose values run without a gap (1, 2, 3, ...) is not searched at all: the row is read at its position. Nothing changes in the database binary or in what a lookup returns
* **A composite key is passed member by member, not as a tuple**: `FindByGenderAndAge((Gender.Female, 23))` becomes `FindByGenderAndAge(Gender.Female, 23)`, and likewise `TryFindBy***` and `FindClosestBy***`. A range takes the members of `min` and then of `max`: `FindRangeByGenderAndAge((g1, a1), (g2, a2))` becomes `FindRangeByGenderAndAge(g1, a1, g2, a2)`. The tuple overloads are gone, so every such call has to be rewritten; the compiler points at each of them. Lookups by a key of one member are unchanged. `PrimaryKeySelector` and `DatabaseBuilder.Remove***` still use a tuple for a composite key, a key there being a value
* A database binary that is cut short or damaged is turned away with an `InvalidOperationException` that says what is wrong (a table the header places outside the data, nil where rows should be, a table listed twice), or with the `MessagePackSerializationException` of the table that cannot be read. Before, some of these surfaced as `ArgumentOutOfRangeException`, `NullReferenceException` or `ArgumentException` from inside
* The head of a composite key has lookups of its own, `FindBy***` and `FindRangeBy***`, answered by the index of the whole key: with a key `(Gender, Age)` there is `FindByGender(Gender)`, see "DataTable configuration". If you wrote a method of that name and signature yourself in a partial table class, it now collides with the generated one: remove yours
* An enum key, alone, nullable or as a member of a composite key, is compared by its underlying value in generated code, in lookups and in the sort of its index. Before, it went through `Comparer<TEnum>.Default`, which in Unity (Mono and IL2CPP) boxes both values on every comparison: garbage on every lookup by an enum key. The order is the same
* **`ImmutableBuilder` is gone, `DatabaseBuilder` does its work**: `db.ToImmutableBuilder()` becomes `db.ToDatabaseBuilder()`, `Diff(rows)` becomes `AddOrReplace(rows)`, and the `Build()` that returned a database becomes `ToMemoryDatabase()`; `Build()` of a `DatabaseBuilder` is the binary, as it was. `ReplaceAll` and `Remove***` keep their names, take any sequence (`Remove***` its keys as `params`), and like `Append` return the builder. The compiler points at every call that has to change. Saving the changed database no longer needs a second builder (`newDatabase.ToDatabaseBuilder().Build()`): the builder that made it writes it. See "DatabaseBuilder"
* `AddOrReplace` given two rows with one key keeps the later one. `Diff` threw an `ArgumentException` from inside when a row of that key was in the table, and added both rows, to a unique key, when none was
* Changing a loaded database costs less: a change used to copy and sort its table, sort every index of it and construct a database, each time; now it is one pass over the rows, and the indexes are built once when the database is made. The generated code for it no longer grows with the square of the number of tables (every method of every table named all tables)
* `DatabaseBuilder.ToMemoryDatabase()` is new: a database from rows without building and reading a binary
* `DatabaseBuilder` writes the tables in `Build` and `WriteToStream`, not in `Append`: a row the serializer cannot write is reported there, and the rows are held by the builder until then. The binary is the same. `db.ToDatabaseBuilder()` therefore serializes nothing by itself
* A null row given to `Append` is an `ArgumentException` that says which table and which index, instead of a `NullReferenceException`
* The name of a table is no longer read from its `[MemoryTable]` attribute by reflection when a database is loaded or a table is appended; the generated code passes it
* Target frameworks are `netstandard2.0`, `netstandard2.1` and `net9.0`
* **A view is not an `IEnumerable<T>`**: `RangeView<T>` and `ReverseRangeView<T>` no longer implement `IReadOnlyList<T>` and `IList<T>`, so LINQ on the result of a lookup does not compile, and neither does passing a view where an `IEnumerable<T>` is taken. Write `AsReadOnlyList()` there: `FindByGender(Gender.Female).Where(...)` becomes `FindByGender(Gender.Female).AsReadOnlyList().Where(...)`. The compiler points at each place, though what it says is only that `Where` is not found. Until v3 every such place boxed the view; now the ones that are left are the ones written out. `foreach`, the indexer, `Count`, `First`/`Last`, `FirstOrDefault()`, `ToArray()`, `Contains`, `IndexOf` and `CopyTo` are members of the views and are unchanged
* `MemoryDatabase` is also read from a `ReadOnlyMemory<byte>` and from a `ReadOnlySpan<byte>`, and so is `GetTableInfo` from a span; see "MemoryDatabase/RangeView". `new MemoryDatabase(bytes)` with a `byte[]` is unchanged. An `ArraySegment<byte>` converts to both and has to say which: `segment.AsMemory()` or `segment.AsSpan()`
* The constructor of `MemoryDatabase` that took every table as a parameter (`new MemoryDatabase(itemTable, personTable, ...)`) is gone; a database from rows is made by `DatabaseBuilder.ToMemoryDatabase()`. With hundreds of tables that constructor had hundreds of parameters
* Strings are interned by MessagePack for C#'s `StringInterningFormatterFactory`, into a pool of the database that is being loaded, and no longer with `string.Intern`. Within a database a string is one object as before. What changed: the strings are not in the runtime's intern pool (`string.IsInterned` says no, and a string of a database is not the same object as an equal literal in your code), two databases do not share their strings, and the strings of a database that is dropped are collected, where they used to stay for the life of the process. A pool that several databases share is there again for those who ask for it, `ShareInternedStrings` of `MemoryDatabaseOptions`: it is the pool of those options and lives as long as they do
* MasterMemory is compatible with trimming and Native AOT and says so (`IsAotCompatible`), see "Native AOT". `AppendDynamic` no longer finds `Append` by reflection and no longer converts the rows with `Convert.ChangeType`: a row has to be an instance of the row type, which is what an importer makes
* The symbols `DISABLE_MASTERMEMORY_METADATABASE` and `DISABLE_MASTERMEMORY_VALIDATOR` are no longer read: the generated code has no `#if` in it. Set `GenerateMetaDatabase = false` and `GenerateValidator = false` in `[MasterMemoryGeneratorOptions]` instead, see "Optimization" for one build among several. A project that still defines one of the symbols builds as before, with that code generated, and gets the warning `MAM012` for it
* The attributes (`[MemoryTable]`, `[PrimaryKey]`, `[SecondaryKey]`, `[NonUnique]`, `[StringComparisonOption]`) are in the `MasterMemory` package itself, in the same namespace. `MasterMemory.Annotations` is kept as a shim that has no types of its own and type-forwards to `MasterMemory`, so an assembly compiled against the v3 one keeps working; it depends on `MasterMemory` now, the other way round from v3. Reference `MasterMemory` alone in new projects, and drop a `MasterMemory.Annotations` reference where you have one

v2 -> v3 migration
---
Since there are no changes to the API, binary format, or behavior, you can migrate simply by changing the command-line tool settings to the assembly attribute `[MasterMemoryGeneratorOptions]`.

* The code generator (MSBuild Task, .NET Core Global/Local Tools) has been removed and replaced with Source Generator
* Tool options are now available through `MasterMemoryGeneratorOptions` (e.g. `-usingNamespace`)
* The `-addImmutableConstructor` option has been completely removed; please use C#'s record or init keyword instead
* The library is now only available through NuGet for Unity. Please use NuGetForUnity

License
---
This library is under the MIT License.
