namespace MasterMemory.SourceGenerator.Tests;

public class DiagnosticsTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    [Fact]
    public void RequirePrimaryKey()
    {
        Helper.Verify(1, """
[MemoryTable("item")]
public class Item
{
    // [PrimaryKey] // No PrimaryKey
    public int ItemId { get; set; }
}
""", "Item");
    }

    [Fact]
    public void DuplicateSecondaryKey()
    {
        Helper.Verify(3, """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId1 { get; set; }
    [SecondaryKey(0), SecondaryKey(1)]
    public int ItemId2 { get; set; }
}
""", "ItemId2");
    }

    // the table name is the key of a table in the database binary; two of one name used to end in a duplicate case label
    [Fact]
    public void DuplicateTableName()
    {
        var diagnostics = Helper.Verify("""
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int Id { get; set; }
}

[MemoryTable("item")]
public class Item2
{
    [PrimaryKey] public int Id { get; set; }
}

[MemoryTable("other")]
public class Other
{
    [PrimaryKey] public int Id { get; set; }
}
""");

        diagnostics.ShouldBe([("MAM005", "Item"), ("MAM005", "Item2")]);
    }

    // Members of a key at one keyOrder are compared in the order they are declared in: [PrimaryKey] on A and then on B
    // is the key (A, B). That order exists within a file and from a base class to a derived one, and not between two
    // files of a partial class.
    [Fact]
    public void DuplicateKeyOrder()
    {
        var table = Helper.GenerateCode("""
public class Entity
{
    [PrimaryKey] public int Tenant { get; set; }
}

[MemoryTable("item")]
public partial class Item : Entity
{
    [PrimaryKey] public int A { get; set; }
    [PrimaryKey] public int B { get; set; }
}

public partial class Item
{
    [PrimaryKey(1)] public int C { get; set; }
}
""")["MasterMemory.ItemTable.g.cs"];
        table.ShouldContain("FindByTenantAndAAndBAndC(");

        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("partial",
            [
                Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("""
using MasterMemory;
[MemoryTable("item")]
public partial class Item
{
    [PrimaryKey] public int A { get; set; }
}
""", path: "Item.A.cs"),
                Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("""
using MasterMemory;
public partial class Item
{
    [PrimaryKey] public int B { get; set; }
}
""", path: "Item.B.cs"),
            ],
            CSharpGeneratorRunner.RunGenerator("").Item1.References);
        Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver.Create(new MasterMemoryGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        diagnostics.Select(x => x.Id).ShouldBe(["MAM006"]);
        diagnostics[0].GetMessage().ShouldContain("PrimaryKey of Item has A, B at keyOrder 0");
    }

    // [NonUnique] is read with the key in its own attribute list. Apart from one it did nothing, silently: the key
    // stayed unique.
    [Fact]
    public void NonUniqueWithoutKey()
    {
        Helper.Verify(7, """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int Id { get; set; }
    [SecondaryKey(0)]
    [NonUnique]
    public int Group { get; set; }
}
""", "Group");


        Helper.Verify(7, """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int Id { get; set; }
    [NonUnique] public int Group { get; set; }
}
""", "Group");
    }

    [Fact]
    public void StringComparisonOptionOnNonStringKey()
    {
        Helper.Verify(8, """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey, StringComparisonOption(System.StringComparison.OrdinalIgnoreCase)] public int Id { get; set; }
}
""", "Id");

        Helper.Verify(8, """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int Id { get; set; }
    [StringComparisonOption(System.StringComparison.OrdinalIgnoreCase)] public string Name { get; set; }
}
""", "Name");
    }

    [Fact]
    public void GenericTable()
    {
        Helper.Verify(9, """
[MemoryTable("item")]
public class Item<T>
{
    [PrimaryKey] public int Id { get; set; }
}
""", "Item");

        Helper.Verify(9, """
public class Outer<T>
{
    [MemoryTable("item")]
    public class Item
    {
        [PrimaryKey] public int Id { get; set; }
    }
}
""", "Item");
    }

    [Fact]
    public void InaccessibleTable()
    {
        Helper.Verify(10, """
public class Outer
{
    [MemoryTable("item")]
    private class Item
    {
        [PrimaryKey] public int Id { get; set; }
    }
}
""", "Item");
    }

    [Fact]
    public void InvalidKeyProperty()
    {
        Helper.Verify(11, """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] int Id { get; set; }
}
""", "Id");

        Helper.Verify(11, """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int Id { get; set; }
    [SecondaryKey(0)] public static int Shared { get; set; }
}
""", "Shared");

        Helper.Verify(11, """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int Id { private get; set; }
}
""", "Id");

        // internal is readable from generated code
        Helper.Ok("""
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] internal int Id { get; set; }
}
""");
    }

    // a table that is wrong is left out, and the others are generated all the same
    [Fact]
    public void OtherTablesAreGenerated()
    {
        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
[MemoryTable("item")]
public class Item
{
    public int Id { get; set; }
}

[MemoryTable("other")]
public class Other
{
    [PrimaryKey] public int Id { get; set; }
}
""");

        diagnostics.Select(x => x.Id).ShouldBe(["MAM001"]);
        var files = compilation.SyntaxTrees.Select(x => Path.GetFileName(x.FilePath)).ToArray();
        files.ShouldContain("MasterMemory.OtherTable.g.cs");
        files.ShouldNotContain("MasterMemory.ItemTable.g.cs");
    }
}
