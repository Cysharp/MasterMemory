﻿namespace MasterMemory.SourceGenerator.Tests;

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

        codes.TryGetValue("MasterMemory.DatabaseBuilder.g.cs", out _).ShouldBeTrue();

        var mainCode = codes["MasterMemory.ItemTable.g.cs"];
        WriteLine(mainCode);

        mainCode.ShouldContain("namespace MasterMemory.Tables");
        mainCode.ShouldContain("return ThrowKeyNotFound(key);");
        mainCode.ShouldContain("public sealed partial class ItemTable");
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

        codes.TryGetValue("MasterMemory.FooBarBazDatabaseBuilder.g.cs", out _).ShouldBeTrue();

        var mainCode = codes["MasterMemory.ItemTable.g.cs"];
        WriteLine(mainCode);

        mainCode.ShouldContain("namespace MyNamespace.Tables");
        mainCode.ShouldNotContain("return ThrowKeyNotFound(key);");
        mainCode.ShouldContain("public sealed partial class ItemTable");
    }
}
