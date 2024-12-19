using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

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



    }


    [Fact]
    public void PrefixClassNameChange()
    {
        var codes = Helper.GenerateCode("""
[assembly: MasterMemoryGeneratorOptions(
    Namespace = "",
    IsReturnNullIfKeyNotFound = true,
    PrefixClassName = "foo")]

[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}
""");
    }
}
