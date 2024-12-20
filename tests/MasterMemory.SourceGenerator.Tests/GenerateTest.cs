using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace MasterMemory.SourceGenerator.Tests;

public class GenerateTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    [Fact]
    public void GenerateClass()
    {
        Helper.Ok("""
[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}
""");
    }

    [Fact]
    public void GenerateRecord()
    {
        Helper.Ok("""
[MemoryTable("item")]
public record Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}
""");
    }
}
