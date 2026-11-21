namespace MasterMemory.SourceGenerator.Tests;

// How a [MemoryTable] class is read: from symbols, so what counts is what the code means and not how it is spelled.
// Every case here either crashed the generator, was dropped without a word, or was reported as "no primary key"
// when attributes and types were read from the text of the declaration.
public class TableReadingTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    string Table(string code, string className = "Item")
    {
        var codes = Helper.GenerateCode(code);
        var table = codes[$"MasterMemory.{className}Table.g.cs"];
        WriteLine(table);
        return table;
    }

    // ---- code that does not compile yet: the state every attribute is in while it is being typed ----

    [Fact]
    public void AttributeBeingTyped()
    {
        // [SecondaryKey] and [StringComparisonOption] without their arguments are compile errors, which the compiler
        // reports. The generator has to live through them: it used to throw, and with it went every generated type.
        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int Id { get; set; }
    [SecondaryKey] public int Group { get; set; }
    [SecondaryKey(0), StringComparisonOption] public string Name { get; set; }
    [SecondaryKey(] public int Broken { get; set; }
}
[MemoryTable("other")]
public class Other
{
    [PrimaryKey] public int Id { get; set; }
}
""");

        diagnostics.ShouldBeEmpty();
        var files = compilation.SyntaxTrees.Select(x => Path.GetFileName(x.FilePath)).ToArray();
        files.ShouldContain("MasterMemory.ItemTable.g.cs");
        files.ShouldContain("MasterMemory.OtherTable.g.cs");

        var table = compilation.SyntaxTrees.First(x => x.FilePath.EndsWith("MasterMemory.ItemTable.g.cs")).ToString();
        table.ShouldContain("FindById(int key)");
        table.ShouldContain("FindByName(string key)");     // the key that is complete
        table.ShouldNotContain("FindByGroup");             // the one that is not, yet
    }

    // ---- attributes, however they are written ----

    [Fact]
    public void AttributeSpellings()
    {
        var table = Table("""
using PK = MasterMemory.PrimaryKeyAttribute;

[MemoryTable("item")]
public class Item
{
    [PK] public int Id { get; set; }
    [SecondaryKeyAttribute(0)] public int A { get; set; }
    [global::MasterMemory.SecondaryKey(1), MasterMemory.NonUniqueAttribute] public int B { get; set; }
}
""");

        table.ShouldContain("public global::Item FindById(int key)");
        table.ShouldContain("public global::Item FindByA(int key)");
        table.ShouldContain("public RangeView<global::Item> FindByB(int key)");
    }

    [Fact]
    public void ConstantArguments()
    {
        var table = Table("""
public static class Index
{
    public const int Name = 2;
    public const int First = 0;
}

[MemoryTable("item")]
public class Item
{
    [PrimaryKey(keyOrder: 1)] public int Id { get; set; }
    [PrimaryKey(Index.First)] public int Group { get; set; }
    [SecondaryKey(Index.Name)] public string Name { get; set; }
    [SecondaryKey(Index.Name + 1, keyOrder: -1)] public int A { get; set; }
    [SecondaryKey(keyOrder: 5, indexNo: 3)] public int B { get; set; }
}
""");

        table.ShouldContain("FindByGroupAndId(int group, int id)");
        table.ShouldContain("readonly global::Item[] secondaryIndex2;");
        table.ShouldContain("FindByName(string key)");
        table.ShouldContain("FindByAAndB(int a, int b)");
    }

    [Fact]
    public void StringComparisonOptionWherever()
    {
        // Before the key in its list, through using static, as a cast of a number, and in a list of its own, which is
        // how the README shows it. Only "after the key, in the same list" used to work; anywhere else the option was
        // dropped and the index stayed ordinal.
        var table = Table("""
using static System.StringComparison;

[MemoryTable("item")]
public class Item
{
    [StringComparisonOption(System.StringComparison.OrdinalIgnoreCase), PrimaryKey] public string Id { get; set; }
    [SecondaryKey(0), StringComparisonOption(InvariantCultureIgnoreCase)] public string A { get; set; }
    [SecondaryKey(1), StringComparisonOption((System.StringComparison)5)] public string B { get; set; }

    [SecondaryKey(2)]
    [SecondaryKey(3), NonUnique]
    [StringComparisonOption(System.StringComparison.InvariantCulture)]
    public string C { get; set; }
}
""");

        // one per property: it holds in every key the property is a member of
        table.ShouldContain("CloneAndSortBy(this.secondaryIndex2Selector, System.StringComparer.InvariantCulture)");
        table.ShouldContain("CloneAndSortBy(this.secondaryIndex3Selector, System.StringComparer.InvariantCulture)");

        table.ShouldContain("return string.Compare(key, other.Id, System.StringComparison.OrdinalIgnoreCase);");
        table.ShouldContain("CloneAndSortBy(this.secondaryIndex0Selector, System.StringComparer.InvariantCultureIgnoreCase)");
        table.ShouldContain("return string.Compare(key, other.A, System.StringComparison.InvariantCultureIgnoreCase);");
        table.ShouldContain("CloneAndSortBy(this.secondaryIndex1Selector, System.StringComparer.OrdinalIgnoreCase)");

        var builder = Helper.GenerateCode("""
[MemoryTable("item")]
public class Item
{
    [StringComparisonOption(System.StringComparison.OrdinalIgnoreCase), PrimaryKey] public string Id { get; set; }
}
""")["MasterMemory.DatabaseBuilder.g.cs"];
        builder.ShouldContain("AppendCore(dataSource, x => x.Id, System.StringComparer.OrdinalIgnoreCase);");
    }

    // ---- where a key is declared ----

    [Fact]
    public void PositionalRecord()
    {
        var table = Table("""
[MemoryTable("item")]
public record Item([property: PrimaryKey] int Id, [property: SecondaryKey(0), NonUnique] string Name, int Level);
""");

        table.ShouldContain("public global::Item FindById(int key)");
        table.ShouldContain("public RangeView<global::Item> FindByName(string key)");
        table.ShouldContain("GetProperty(\"Level\")");
        table.ShouldNotContain("EqualityContract");
    }

    [Fact]
    public void PartialClass()
    {
        var table = Table("""
[MemoryTable("item")]
public partial class Item
{
    public string Name { get; set; }
}

public partial class Item
{
    [PrimaryKey] public int Id { get; set; }
}
""");

        table.ShouldContain("public global::Item FindById(int key)");
        table.ShouldContain("GetProperty(\"Name\")");
    }

    [Fact]
    public void BaseClass()
    {
        var table = Table("""
public abstract class Entity
{
    [PrimaryKey] public int Id { get; set; }
    [SecondaryKey(0), NonUnique] public virtual int Group { get; set; }
    [SecondaryKey(1)] public virtual string Code { get; set; }
}

[MemoryTable("item")]
public class Item : Entity
{
    public string Name { get; set; }
    public override int Group { get; set; }                  // no attribute of its own: the base declaration counts
    [SecondaryKey(2)] public override string Code { get; set; }   // attributes of its own: these count
}
""");

        table.ShouldContain("public global::Item FindById(int key)");
        table.ShouldContain("public RangeView<global::Item> FindByGroup(int key)");
        table.ShouldContain("readonly global::Item[] secondaryIndex2;");
        table.ShouldNotContain("secondaryIndex1;");
        table.Split("GetProperty(\"Group\")").Length.ShouldBe(3); // once as a column, once as a key
    }

    // ---- what the type is, however it is spelled ----

    [Fact]
    public void KeyTypes()
    {
        var table = Table("""
using MyId = System.Int32;

[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public MyId Id { get; set; }
    [SecondaryKey(0)] public short S { get; set; }
    [SecondaryKey(1)] public char C { get; set; }
    [SecondaryKey(2)] public ushort U { get; set; }
    [SecondaryKey(3)] public global::System.Int64 L { get; set; }
    [SecondaryKey(4)] public System.String Name { get; set; }
    [SecondaryKey(5)] public int? N { get; set; }
}
""");

        // an alias of int is an int: read at its position when the keys run without a gap, searched without jumps
        table.ShouldContain("readonly int primaryIndexDenseLength;");
        table.ShouldContain("readonly struct PrimaryIndexComparable : IBranchlessComparable<global::Item>");

        // every integer of 32 bits or less is compared directly, and searched without jumps
        table.ShouldContain("return (key < other.S) ? -1 : (key > other.S) ? 1 : 0;");
        table.ShouldContain("return (key < other.C) ? -1 : (key > other.C) ? 1 : 0;");
        table.ShouldContain("return (key < other.U) ? -1 : (key > other.U) ? 1 : 0;");
        table.ShouldContain("readonly struct SecondaryIndex0Comparable : IBranchlessComparable<global::Item>");
        table.ShouldContain("readonly struct SecondaryIndex1Comparable : IBranchlessComparable<global::Item>");
        table.ShouldContain("readonly struct SecondaryIndex2Comparable : IBranchlessComparable<global::Item>");

        // 64 bits: compared directly, searched with jumps
        table.ShouldContain("return (key < other.L) ? -1 : (key > other.L) ? 1 : 0;");
        table.ShouldContain("readonly struct SecondaryIndex3Comparable : IComparable<global::Item>");

        table.ShouldContain("return string.CompareOrdinal(key, other.Name);");
        table.ShouldContain("return System.Collections.Generic.Comparer<int?>.Default.Compare(key, other.N);");
    }

    [Fact]
    public void KeywordAsPropertyName()
    {
        var table = Table("""
[MemoryTable("item")]
public class Item
{
    [PrimaryKey(0)] public int @class { get; set; }
    [PrimaryKey(1)] public int @event { get; set; }
    [SecondaryKey(0)] public int @int { get; set; }
}
""");

        table.ShouldContain("FindByclassAndevent(int @class, int @event)");
        table.ShouldContain("FindRangeByclassAndevent(int minclass, int minevent, int maxclass, int maxevent, bool ascendant = true)");
        table.ShouldContain("x => (x.@class, x.@event)");
        table.ShouldContain("return (key < other.@int) ? -1 : (key > other.@int) ? 1 : 0;");
        table.ShouldContain("GetProperty(\"class\")");
    }

    // ---- how a key is passed ----

    // A composite key is passed member by member, not as a tuple: FindByAAndB(a, b), and for a range the members of
    // min and then of max. A key of one member is "key", "min" and "max", as it always was.
    [Fact]
    public void CompositeKeyParameters()
    {
        var table = Table("""
public enum Gender { Male, Female }

[MemoryTable("item")]
public class Item
{
    [PrimaryKey(0)] public Gender Gender { get; set; }
    [PrimaryKey(1)] public int Age { get; set; }
    [SecondaryKey(0, 0), NonUnique] public string Name { get; set; }
    [SecondaryKey(0, 1), NonUnique] public long? No { get; set; }
    [SecondaryKey(1)] public int Single { get; set; }
}
""");

        // unique
        table.ShouldContain("public global::Item FindByGenderAndAge(global::Gender gender, int age)");
        table.ShouldContain("public bool TryFindByGenderAndAge(global::Gender gender, int age, out global::Item result)");
        table.ShouldContain("public global::Item? FindClosestByGenderAndAge(global::Gender gender, int age, bool selectLower = true)");
        table.ShouldContain("public RangeView<global::Item> FindRangeByGenderAndAge(global::Gender minGender, int minAge, global::Gender maxGender, int maxAge, bool ascendant = true)");
        table.ShouldContain("var comparable = new PrimaryIndexComparable(gender, age);");
        table.ShouldContain("return FindUniqueRangeCore(data, new PrimaryIndexComparable(minGender, minAge), new PrimaryIndexComparable(maxGender, maxAge), ascendant);");
        table.ShouldContain("return ThrowKeyNotFound((gender, age));");

        // shared
        table.ShouldContain("public RangeView<global::Item> FindByNameAndNo(string name, long? no)");
        table.ShouldContain("public RangeView<global::Item> FindClosestByNameAndNo(string name, long? no, bool selectLower = true)");
        table.ShouldContain("return FindManyCore(secondaryIndex0, new SecondaryIndex0Comparable(row.Name, row.No));");
        table.ShouldContain("public RangeView<global::Item> FindRangeByNameAndNo(string minName, long? minNo, string maxName, long? maxNo, bool ascendant = true)");

        // the struct takes the members and keeps them together
        table.ShouldContain("public PrimaryIndexComparable(global::Gender gender, int age)");
        table.ShouldContain("this.key = (gender, age);");

        // one member: unchanged
        table.ShouldContain("public global::Item FindBySingle(int key)");
        table.ShouldContain("public RangeView<global::Item> FindRangeBySingle(int min, int max, bool ascendant = true)");

        // no lookup takes a tuple
        table.ShouldNotContain("FindByGenderAndAge((");
        table.ShouldNotContain(") key)");
        table.ShouldNotContain(") min, (");
    }

    // a member whose name would shadow a parameter or a variable of the generated method gets another name
    [Fact]
    public void CompositeKeyParameterNames()
    {
        var table = Table("""
[MemoryTable("item")]
public class Item
{
    [PrimaryKey(0)] public int Result { get; set; }
    [PrimaryKey(1)] public int Index { get; set; }
    [PrimaryKey(2)] public int Class { get; set; }
    [PrimaryKey(3)] public int Key { get; set; }
}
""");

        table.ShouldContain("public bool TryFindByResultAndIndexAndClassAndKey(int resultKey1, int indexKey2, int @class, int keyKey4, out global::Item result)");
        table.ShouldContain("FindRangeByResultAndIndexAndClassAndKey(int minResult, int minIndex, int minClass, int minKey, int maxResult, int maxIndex, int maxClass, int maxKey, bool ascendant = true)");
    }

    // ---- composite keys ----

    [Fact]
    public void CompositeKeyOrder()
    {
        var codes = Helper.GenerateCode("""
public enum Kind : byte { A, B }

[MemoryTable("item")]
public class Item
{
    [PrimaryKey(0)] public string Name { get; set; }
    [PrimaryKey(1)] public Kind Kind { get; set; }
    [SecondaryKey(0, 0), NonUnique, StringComparisonOption(System.StringComparison.OrdinalIgnoreCase)] public string Code { get; set; }
    [SecondaryKey(0, 1), NonUnique] public int Level { get; set; }
}
""");
        var table = codes["MasterMemory.ItemTable.g.cs"];
        WriteLine(table);

        // A string in a composite key is compared ordinally, as a key of one string is. It used to be compared by the
        // tuple's default comparer, which means by the current culture.
        table.ShouldContain("internal sealed class PrimaryIndexKeyComparer : System.Collections.Generic.IComparer<(string Name, global::Kind Kind)>, System.Collections.IComparer");
        table.ShouldContain("c = string.CompareOrdinal(x.Name, y.Name); if (c != 0) { return c; }");
        table.ShouldContain("return ((byte)x.Kind < (byte)y.Kind) ? -1 : ((byte)x.Kind > (byte)y.Kind) ? 1 : 0;");

        // the lookup makes the same comparisons, in the same order
        table.ShouldContain("c = string.CompareOrdinal(key.Name, other.Name); if (c != 0) { return c; }");
        table.ShouldContain("return ((byte)key.Kind < (byte)other.Kind) ? -1 : ((byte)key.Kind > (byte)other.Kind) ? 1 : 0;");

        // [StringComparisonOption] is per member, and no longer ignored on a composite key
        table.ShouldContain("c = string.Compare(x.Code, y.Code, System.StringComparison.OrdinalIgnoreCase); if (c != 0) { return c; }");
        table.ShouldContain("c = string.Compare(key.Code, other.Code, System.StringComparison.OrdinalIgnoreCase); if (c != 0) { return c; }");
        table.ShouldContain("CloneAndSortBy(this.secondaryIndex0Selector, SecondaryIndex0KeyComparer.Instance)");

        // everything that sorts by the key uses that comparer
        table.ShouldNotContain(">.Default)");
        codes["MasterMemory.DatabaseBuilder.g.cs"].ShouldContain("AppendCore(dataSource, x => (x.Name, x.Kind), ItemTable.PrimaryIndexKeyComparer.Instance);");
        codes["MasterMemory.ImmutableBuilder.g.cs"].ShouldContain("CloneAndSortBy(data, x => (x.Name, x.Kind), ItemTable.PrimaryIndexKeyComparer.Instance);");
        codes["MasterMemory.ImmutableBuilder.g.cs"].ShouldNotContain("Comparer<(");
    }

    // ---- enum keys ----

    // The default comparer of an enum boxes both values on every comparison under Mono and IL2CPP (Unity): garbage on
    // every lookup, and on every comparison of a sort. Nothing generated for an enum key goes through it: the
    // lookups, the sort of the index, and the builders all compare the underlying value.
    [Fact]
    public void EnumKeysDoNotUseTheDefaultComparer()
    {
        var codes = Helper.GenerateCode("""
public enum Gender { Male, Female, Unknown }
public enum Rank : ushort { Low, High }

[MemoryTable("person")]
public class Person
{
    [PrimaryKey] public Rank Rank { get; set; }
    [SecondaryKey(0), NonUnique] public Gender Gender { get; set; }
    [SecondaryKey(1, 0), NonUnique] public Gender Gender2 { get; set; }
    [SecondaryKey(1, 1), NonUnique] public int Age { get; set; }
    [SecondaryKey(2), NonUnique] public Gender? MaybeGender { get; set; }
}
""");
        var table = codes["MasterMemory.PersonTable.g.cs"];
        WriteLine(table);

        // a lookup
        table.ShouldContain("return ((ushort)key < (ushort)other.Rank) ? -1 : ((ushort)key > (ushort)other.Rank) ? 1 : 0;");
        table.ShouldContain("return ((int)key < (int)other.Gender) ? -1 : ((int)key > (int)other.Gender) ? 1 : 0;");
        table.ShouldContain("c = ((int)key.Gender2 < (int)other.Gender2) ? -1 : ((int)key.Gender2 > (int)other.Gender2) ? 1 : 0; if (c != 0) { return c; }");
        table.ShouldContain("return key.HasValue ? (other.MaybeGender.HasValue ? (((int)key.GetValueOrDefault() < (int)other.MaybeGender.GetValueOrDefault()) ? -1 : ((int)key.GetValueOrDefault() > (int)other.MaybeGender.GetValueOrDefault()) ? 1 : 0) : 1) : (other.MaybeGender.HasValue ? -1 : 0);");

        // the sort of an index
        table.ShouldContain("internal sealed class PrimaryIndexKeyComparer : System.Collections.Generic.IComparer<global::Rank>, System.Collections.IComparer");
        table.ShouldContain("return ((ushort)x < (ushort)y) ? -1 : ((ushort)x > (ushort)y) ? 1 : 0;");
        table.ShouldContain("CloneAndSortBy(this.secondaryIndex0Selector, SecondaryIndex0KeyComparer.Instance)");
        table.ShouldContain("CloneAndSortBy(this.secondaryIndex1Selector, SecondaryIndex1KeyComparer.Instance)");
        table.ShouldContain("CloneAndSortBy(this.secondaryIndex2Selector, SecondaryIndex2KeyComparer.Instance)");
        table.ShouldContain("internal sealed class SecondaryIndex2KeyComparer : System.Collections.Generic.IComparer<global::Gender?>, System.Collections.IComparer");

        // and nowhere else either: every key of this table is an enum or has one, so no default comparer is left at all
        foreach (var code in codes.Values)
        {
            code.ShouldNotContain(">.Default");
        }
        codes["MasterMemory.DatabaseBuilder.g.cs"].ShouldContain("AppendCore(dataSource, x => x.Rank, PersonTable.PrimaryIndexKeyComparer.Instance);");
    }

    // ---- accessibility ----

    [Fact]
    public void InternalTable()
    {
        var codes = Helper.GenerateCode("""
[MemoryTable("item")]
internal class Item
{
    [PrimaryKey] public int Id { get; set; }
}

[MemoryTable("open")]
public class Open
{
    [PrimaryKey] public int Id { get; set; }
}

internal static class Outer
{
    [MemoryTable("nested")]
    public class Nested
    {
        [PrimaryKey] public int Id { get; set; }
    }
}
""");

        // members with an internal type in their signature are internal, and nothing else is
        codes["MasterMemory.ItemTable.g.cs"].ShouldContain("internal sealed partial class ItemTable : TableBase<global::Item>");
        codes["MasterMemory.NestedTable.g.cs"].ShouldContain("internal sealed partial class NestedTable : TableBase<global::Outer.Nested>");
        codes["MasterMemory.OpenTable.g.cs"].ShouldContain("public sealed partial class OpenTable : TableBase<global::Open>");

        var database = codes["MasterMemory.MemoryDatabase.g.cs"];
        WriteLine(database);
        database.ShouldContain("public sealed partial class MemoryDatabase");
        database.ShouldContain("internal ItemTable ItemTable { get; private set; }");
        database.ShouldContain("internal NestedTable NestedTable { get; private set; }");
        database.ShouldContain("public OpenTable OpenTable { get; private set; }");
        database.ShouldContain("internal MemoryDatabase(");
        database.ShouldContain("public MemoryDatabase(byte[] databaseBinary");

        var builder = codes["MasterMemory.DatabaseBuilder.g.cs"];
        builder.ShouldContain("internal DatabaseBuilder Append(System.Collections.Generic.IEnumerable<global::Item> dataSource)");
        builder.ShouldContain("public DatabaseBuilder Append(System.Collections.Generic.IEnumerable<global::Open> dataSource)");

        var immutable = codes["MasterMemory.ImmutableBuilder.g.cs"];
        immutable.ShouldContain("internal void ReplaceAll(System.Collections.Generic.IList<global::Item> data)");
        immutable.ShouldContain("internal void RemoveItem(int[] keys)");
        immutable.ShouldContain("internal void Diff(global::Item[] addOrReplaceData)");
        immutable.ShouldContain("public void ReplaceAll(System.Collections.Generic.IList<global::Open> data)");
    }

    [Fact]
    public void AllTablesPublic()
    {
        var database = Helper.GenerateCode("""
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int Id { get; set; }
}
""")["MasterMemory.MemoryDatabase.g.cs"];

        database.ShouldContain("public MemoryDatabase(");
        database.ShouldNotContain("internal");
    }

    // ---- the metadata ----

    [Fact]
    public void Columns()
    {
        var table = Table("""
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int Id { get; set; }
    public string Name { get; set; }
    [MessagePack.IgnoreMember] public string Ignored { get; set; }
    int Private { get; set; }
    internal int Internal { get; set; }
    public static Item Default { get; } = new Item();
    public int this[int index] => index;
}

namespace MessagePack
{
    public class IgnoreMemberAttribute : System.Attribute { }
}
""");

        // the public instance properties: what reflection is asked for by name has to be what GetProperty finds
        table.ShouldContain("GetProperty(\"Id\")");
        table.ShouldContain("GetProperty(\"Name\")");
        table.ShouldNotContain("GetProperty(\"Ignored\")");
        table.ShouldNotContain("GetProperty(\"Private\")");
        table.ShouldNotContain("GetProperty(\"Internal\")");
        table.ShouldNotContain("GetProperty(\"Default\")");
        table.ShouldNotContain("GetProperty(\"this[]\")");
    }

    [Fact]
    public void TableNameIsWrittenAsALiteral()
    {
        var codes = Helper.GenerateCode("""
[MemoryTable("it\"em\\1")]
public class Item
{
    [PrimaryKey] public int Id { get; set; }
}
""");

        codes["MasterMemory.MemoryDatabase.g.cs"].ShouldContain("""case "it\"em\\1":""");
        codes["MasterMemory.ItemTable.g.cs"].ShouldContain("""typeof(ItemTable), "it\"em\\1",""");
    }
}
