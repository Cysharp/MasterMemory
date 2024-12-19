namespace MasterMemory.SourceGenerator.Tests;

public class AssemblyAtrributeTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    [Fact]
    public void NoGeneratorOptions()
    {
        var codes = Helper.GenerateCode("""
[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}
""");

        codes.TryGetValue("MasterMemory.DatabaseBuilder.g.cs", out _).Should().BeTrue();

        var mainCode = codes["MasterMemory.ItemTable.g.cs"];
        WriteLine(mainCode);

        mainCode.Should().Contain("namespace MasterMemory.Tables");
        mainCode.Should().Contain("return ThrowKeyNotFound(key);");
        mainCode.Should().Contain("public sealed partial class ItemTable");
    }


    [Fact]
    public void FullOptions()
    {
        var codes = Helper.GenerateCode("""
[assembly: MasterMemoryGeneratorOptions(
    Namespace = "MyNamespace",
    IsReturnNullIfKeyNotFound = true,
    PrefixClassName = "FooBarBaz")]

[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}
""");

        codes.TryGetValue("MasterMemory.FooBarBazDatabaseBuilder.g.cs", out _).Should().BeTrue();

        var mainCode = codes["MasterMemory.ItemTable.g.cs"];
        WriteLine(mainCode);

        mainCode.Should().Contain("namespace MyNamespace.Tables");
        mainCode.Should().NotContain("return ThrowKeyNotFound(key);");
        mainCode.Should().Contain("public sealed partial class ItemTable");
    }
}
