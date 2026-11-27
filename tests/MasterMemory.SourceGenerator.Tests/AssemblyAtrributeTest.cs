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

    const string ItemTable = """
[MemoryTable("item")]
public class Item
{
    [PrimaryKey]
    public int ItemId { get; set; }
}
""";

    // The metadata and the validation are generated unless an option says otherwise. Until v3 the generated code wrapped
    // them in "#if !DISABLE_MASTERMEMORY_..." and a symbol took them out; nothing is left to a symbol now.
    [Fact]
    public void MetaDatabaseAndValidatorAreGeneratedByDefault()
    {
        var codes = Helper.GenerateCode(ItemTable);

        codes["MasterMemory.MemoryDatabase.g.cs"].ShouldContain("public static MasterMemory.Meta.MetaDatabase GetMetaDatabase()");
        codes["MasterMemory.MemoryDatabase.g.cs"].ShouldContain("public ValidateResult Validate()");
        codes["MasterMemory.ItemTable.g.cs"].ShouldContain("public static MasterMemory.Meta.MetaTable CreateMetaTable()");
        codes["MasterMemory.ItemTable.g.cs"].ShouldContain("void ITableUniqueValidate.ValidateUnique(ValidateResult resultSet)");

        foreach (var code in codes)
        {
            code.Value.ShouldNotContain("#if", customMessage: code.Key);
            code.Value.ShouldNotContain("DISABLE_MASTERMEMORY", customMessage: code.Key);
        }
    }

    [Fact]
    public void GenerateMetaDatabaseFalse()
    {
        var codes = Helper.GenerateCode("[assembly: MasterMemoryGeneratorOptions(GenerateMetaDatabase = false)]\n" + ItemTable);

        foreach (var code in codes.Where(x => x.Key != "MasterMemory.MasterMemoryGeneratorOptions.g.cs"))
        {
            code.Value.ShouldNotContain("Meta", customMessage: code.Key);
        }

        // the other one is untouched
        codes["MasterMemory.MemoryDatabase.g.cs"].ShouldContain("public ValidateResult Validate()");
        codes["MasterMemory.ItemTable.g.cs"].ShouldContain("ITableUniqueValidate");
    }

    [Fact]
    public void GenerateValidatorFalse()
    {
        var codes = Helper.GenerateCode("[assembly: MasterMemoryGeneratorOptions(GenerateValidator = false)]\n" + ItemTable);

        codes["MasterMemory.MemoryDatabase.g.cs"].ShouldNotContain("Validate");
        codes["MasterMemory.ItemTable.g.cs"].ShouldNotContain("Validate");
        codes["MasterMemory.ItemTable.g.cs"].ShouldContain("public sealed partial class ItemTable : TableBase<global::Item>");

        // the other one is untouched
        codes["MasterMemory.MemoryDatabase.g.cs"].ShouldContain("GetMetaDatabase()");
        codes["MasterMemory.ItemTable.g.cs"].ShouldContain("CreateMetaTable()");
    }

    // The generated code has to compile in each of the four combinations: with one part left out, nothing that
    // remains may refer to it.
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void EveryCombinationCompiles(bool generateMetaDatabase, bool generateValidator)
    {
        var options = $"[assembly: MasterMemoryGeneratorOptions(GenerateMetaDatabase = {(generateMetaDatabase ? "true" : "false")}, GenerateValidator = {(generateValidator ? "true" : "false")})]\n";
        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator(options + ItemTable + """

[MemoryTable("person"), MessagePack.MessagePackObject(true)]
public class Person
{
    [PrimaryKey] public int Id { get; set; }
    [SecondaryKey(0), NonUnique] public string Name { get; set; } = "";
}
""");
        diagnostics.ShouldBeEmpty();

        // CS0246 / CS0234 / CS0012 for MessagePack are the references this test compilation does not have; what is
        // looked for here is a name of the generated code itself that no longer exists.
        var unknownMembers = compilation.GetDiagnostics()
            .Where(x => x.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .Where(x => x.Id is "CS0103" or "CS0117" or "CS1061" or "CS0535" or "CS0540" or "CS0539")
            .Select(x => x.ToString())
            .ToArray();
        unknownMembers.ShouldBeEmpty();
    }

    // MAM012: a symbol of v3 that is still defined is read by nothing, and the code it took out is generated
    [Fact]
    public void ObsoleteSymbolIsReported()
    {
        var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator(ItemTable, preprocessorSymbols: ["DISABLE_MASTERMEMORY_METADATABASE", "DISABLE_MASTERMEMORY_VALIDATOR"]);

        diagnostics.Select(x => x.Id).ShouldBe(["MAM012", "MAM012"]);
        diagnostics.ShouldAllBe(x => x.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Warning);
        diagnostics[0].GetMessage().ShouldContain("DISABLE_MASTERMEMORY_METADATABASE");
        diagnostics[0].GetMessage().ShouldContain("GenerateMetaDatabase = false");
        diagnostics[1].GetMessage().ShouldContain("DISABLE_MASTERMEMORY_VALIDATOR");
        diagnostics[1].GetMessage().ShouldContain("GenerateValidator = false");
    }

    // where the option is set, the symbol left behind does no harm and is not reported
    [Fact]
    public void ObsoleteSymbolIsNotReportedWhereTheOptionIsSet()
    {
        var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator(
            "[assembly: MasterMemoryGeneratorOptions(GenerateMetaDatabase = false)]\n" + ItemTable,
            preprocessorSymbols: ["DISABLE_MASTERMEMORY_METADATABASE", "DISABLE_MASTERMEMORY_VALIDATOR"]);

        diagnostics.Length.ShouldBe(1);
        diagnostics[0].GetMessage().ShouldContain("DISABLE_MASTERMEMORY_VALIDATOR");
    }

    // a symbol is often defined for a whole solution: a project without tables has nothing the symbol could take out
    [Fact]
    public void ObsoleteSymbolIsNotReportedWithoutTables()
    {
        var (_, diagnostics) = CSharpGeneratorRunner.RunGenerator("public class NoTable { }", preprocessorSymbols: ["DISABLE_MASTERMEMORY_METADATABASE"]);

        diagnostics.ShouldBeEmpty();
    }
}
