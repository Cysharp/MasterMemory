using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
