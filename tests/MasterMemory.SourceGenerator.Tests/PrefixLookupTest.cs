using System.Text.RegularExpressions;

namespace MasterMemory.SourceGenerator.Tests;

// The head of a composite key is a key too: an index sorted by (Gender, Age) is sorted by Gender, so it answers
// FindByGender, and no index of Gender alone has to be built and kept in memory.
public class PrefixLookupTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    string Table(string code)
    {
        var table = Helper.GenerateCode(code)["MasterMemory.PersonTable.g.cs"];
        WriteLine(table);
        return table;
    }

    static int Count(string code, string text) => Regex.Matches(code, Regex.Escape(text)).Count;

    [Fact]
    public void HeadOfACompositeKey()
    {
        var table = Table("""
public enum Gender { Male, Female, Unknown }

[MemoryTable("person")]
public class Person
{
    [PrimaryKey] public int PersonId { get; set; }
    [SecondaryKey(0, keyOrder: 0), NonUnique] public Gender Gender { get; set; }
    [SecondaryKey(0, keyOrder: 1), NonUnique] public int Age { get; set; }
}
""");

        // through the index of the whole key: nothing more is sorted or stored
        table.ShouldContain("public RangeView<global::Person> FindByGender(global::Gender key)");
        table.ShouldContain("return FindManyCore(secondaryIndex0, new SecondaryIndex0Prefix1Comparable(key));");
        table.ShouldContain("public RangeView<global::Person> FindRangeByGender(global::Gender min, global::Gender max, bool ascendant = true)");
        table.ShouldContain("return FindManyRangeCore(secondaryIndex0, new SecondaryIndex0Prefix1Comparable(min), new SecondaryIndex0Prefix1Comparable(max), ascendant);");
        Count(table, "CloneAndSortBy(").ShouldBe(1);
        Count(table, "readonly global::Person[] secondaryIndex").ShouldBe(1);

        // only the members it is given, compared the way the index is sorted
        table.ShouldContain("readonly struct SecondaryIndex0Prefix1Comparable : IComparable<global::Person>");
        table.ShouldContain("return ((int)key < (int)other.Gender) ? -1 : ((int)key > (int)other.Gender) ? 1 : 0;");

        // Find and FindRange; the nearest head is not offered
        table.ShouldNotContain("FindClosestByGender(");
        table.ShouldNotContain("SortByGender ");
    }

    [Fact]
    public void EveryHeadOfALongerKey()
    {
        var table = Table("""
[MemoryTable("person")]
public class Person
{
    [PrimaryKey(0)] public int Category { get; set; }
    [PrimaryKey(1), StringComparisonOption(System.StringComparison.OrdinalIgnoreCase)] public string Name { get; set; }
    [PrimaryKey(2)] public long No { get; set; }
}
""");

        // a unique key: its heads are still several rows each
        table.ShouldContain("public global::Person FindByCategoryAndNameAndNo((int Category, string Name, long No) key)");
        table.ShouldContain("public RangeView<global::Person> FindByCategory(int key)");
        table.ShouldContain("return FindManyCore(data, new PrimaryIndexPrefix1Comparable(key));");
        table.ShouldContain("public RangeView<global::Person> FindByCategoryAndName((int Category, string Name) key)");
        table.ShouldContain("return FindManyCore(data, new PrimaryIndexPrefix2Comparable(key));");
        table.ShouldContain("public RangeView<global::Person> FindRangeByCategoryAndName((int Category, string Name) min, (int Category, string Name) max, bool ascendant = true)");

        // member by member, each as the key compares it
        table.ShouldContain("readonly struct PrimaryIndexPrefix2Comparable : IComparable<global::Person>");
        Count(table, "c = (key.Category < other.Category) ? -1 : (key.Category > other.Category) ? 1 : 0; if (c != 0) { return c; }").ShouldBe(2); // the key, its head of two
        Count(table, "return string.Compare(key.Name, other.Name, System.StringComparison.OrdinalIgnoreCase);").ShouldBe(1);

        // the whole key is not a head of itself
        table.ShouldNotContain("PrimaryIndexPrefix3Comparable");
    }

    // a key declared on exactly those members is what answers, as it always did
    [Fact]
    public void ADeclaredKeyComesFirst()
    {
        var table = Table("""
[MemoryTable("person")]
public class Person
{
    [PrimaryKey] public int PersonId { get; set; }

    [SecondaryKey(0, 0), NonUnique]
    [SecondaryKey(1)]
    public int Group { get; set; }

    [SecondaryKey(0, 1), NonUnique]
    public int Age { get; set; }
}
""");

        table.ShouldContain("public global::Person FindByGroup(int key)"); // the unique key 1
        Count(table, " FindByGroup(").ShouldBe(1);
        Count(table, " FindRangeByGroup(").ShouldBe(1);
        table.ShouldNotContain("Prefix1Comparable");
    }

    // several keys with one head: the primary key, then the secondary keys by index number. The rows are the same
    // whichever answers; the order they come in is that of its index.
    [Fact]
    public void SeveralKeysWithOneHead()
    {
        var table = Table("""
[MemoryTable("person")]
public class Person
{
    [PrimaryKey(0)]
    [SecondaryKey(3, 0), NonUnique]
    public int A { get; set; }

    [PrimaryKey(1)]
    public int B { get; set; }

    [SecondaryKey(3, 1), NonUnique]
    [SecondaryKey(7, 1), NonUnique]
    [SecondaryKey(5, 1), NonUnique]
    public int C { get; set; }

    [SecondaryKey(7, 0), NonUnique]
    [SecondaryKey(5, 0), NonUnique]
    public int D { get; set; }

    [SecondaryKey(5, 2), NonUnique]
    public int E { get; set; }
}
""");

        // A: the primary key (A, B), not the secondary key 3 (A, C)
        Count(table, " FindByA(").ShouldBe(1);
        table.ShouldContain("return FindManyCore(data, new PrimaryIndexPrefix1Comparable(key));");
        table.ShouldNotContain("SecondaryIndex3Prefix1Comparable");

        // D and (D, C): the key 5 (D, C, E), declared after the key 7 (D, C) but numbered before it
        Count(table, " FindByD(").ShouldBe(1);
        table.ShouldContain("return FindManyCore(secondaryIndex5, new SecondaryIndex5Prefix1Comparable(key));");
        table.ShouldNotContain("SecondaryIndex7Prefix1Comparable");

        // (D, C) is the whole of the key 7: that key answers, and no head of the key 5 is generated for it
        Count(table, " FindByDAndC(").ShouldBe(1);
        table.ShouldNotContain("SecondaryIndex5Prefix2Comparable");
    }

    [Fact]
    public void NoCompositeKeyNoHead()
    {
        var table = Table("""
[MemoryTable("person")]
public class Person
{
    [PrimaryKey] public int PersonId { get; set; }
    [SecondaryKey(0), NonUnique] public int Age { get; set; }
}
""");

        table.ShouldNotContain("Prefix");
    }
}
