using MasterMemory.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MasterMemory.SourceGenerator.Tests;

public class IncrementalGeneratorTest
{
    // there are three outputs (diagnostics, each table, the database-wide files); "cached" means every one of them
    static string[] SourceOutputReasons((string Key, string Reasons)[] reasons)
    {
        return reasons.First(x => x.Key == "SourceOutput").Reasons.Split([", "], StringSplitOptions.RemoveEmptyEntries);
    }

    [Fact]
    public void CheckReasons()
    {
        var step1 = """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}
""";

        var step2 = """
[MemoryTable("item")]
public class Item
{
    // add unrelated line
    [PrimaryKey]
    public int ItemId { get; set; }
}
""";

        var step3 = """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId2 { get; set; } // changed name
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("MasterMemory.SyntaxProvider.", step1, step2, step3);

        SourceOutputReasons(reasons[1]).ShouldAllBe(x => x == "Cached");
        SourceOutputReasons(reasons[2]).ShouldContain(x => x != "Cached");
    }

    // Lines added above a class move it in the file, and nothing else. The model does not carry where the class is,
    // so nothing is written again.
    [Fact]
    public void MovingAClassIsCached()
    {
        var step1 = """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}
""";

        var step2 = """
// a comment
// and another

[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}
""";

        var reasons = CSharpGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("MasterMemory.SyntaxProvider.", step1, step2);

        SourceOutputReasons(reasons[1]).ShouldAllBe(x => x == "Cached");
    }

    // Each table has its own output: a change to one table writes that table's file, and not the files of the others.
    // The database-wide files are written again only for what they contain (the name, the type, the primary key).
    [Fact]
    public void OneTableChangesOneFile()
    {
        const string other = """
using MasterMemory;

[MemoryTable("other")]
public class Other
{
    [PrimaryKey] public int Id { get; set; }
    [SecondaryKey(0)] public string Name { get; set; }
}
""";

        var step1 = other + """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int ItemId { get; set; }
    public int Level { get; set; }
}
""";

        // a secondary key on Item: ItemTable changes, nothing else
        var step2 = other + """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int ItemId { get; set; }
    [SecondaryKey(0), NonUnique] public int Level { get; set; }
}
""";

        // the primary key of Item: ItemTable and the database-wide files, still not OtherTable
        var step3 = other + """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public long ItemId { get; set; }
    [SecondaryKey(0), NonUnique] public int Level { get; set; }
}
""";

        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp12);
        var driver = CSharpGeneratorDriver.Create(
            [new MasterMemoryGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true))
            .WithUpdatedParseOptions(parseOptions);
        var references = CSharpGeneratorRunner.RunGenerator("").Item1.References;

        // per output, whether it ran: the outputs are told apart by the step they take their input from, and the
        // per-table one runs once per table, in the order the tables are emitted in (by class name: Item, Other)
        Dictionary<string, bool[]> Run(string source)
        {
            var compilation = CSharpCompilation.Create("incremental", [CSharpSyntaxTree.ParseText(source, parseOptions)], references);
            driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);
            return driver.GetRunResult().Results[0].TrackedOutputSteps["SourceOutput"]
                .GroupBy(x => x.Inputs[0].Source.Name!.Substring("MasterMemory.SyntaxProvider.".Length))
                .ToDictionary(x => x.Key, x => x.SelectMany(step => step.Outputs).Select(o => o.Reason is IncrementalStepRunReason.New or IncrementalStepRunReason.Modified).ToArray());
        }

        var first = Run(step1);
        first["3_EachTable"].ShouldBe([true, true]);
        first["4_DatabaseEntries"].ShouldBe([true]);

        var second = Run(step2);
        second["3_EachTable"].ShouldBe([true, false]);
        second["4_DatabaseEntries"].ShouldBe([false]);

        var third = Run(step3);
        third["3_EachTable"].ShouldBe([true, false]);
        third["4_DatabaseEntries"].ShouldBe([true]);
    }
}
