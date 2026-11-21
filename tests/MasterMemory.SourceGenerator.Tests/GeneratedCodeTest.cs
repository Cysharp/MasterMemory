using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MasterMemory.SourceGenerator.Tests;

public class GeneratedCodeTest(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    static void ShouldHaveNoSyntaxError(Dictionary<string, string> codes)
    {
        foreach (var item in codes)
        {
            var errors = CSharpSyntaxTree.ParseText(item.Value).GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.ToString()).ToArray();
            errors.ShouldBeEmpty(item.Key);
        }
    }

    // issue 109: the first using directive carried "#if" as its leading trivia into every generated file, without the "#endif"
    [Fact]
    public void UsingDirectivesInsideIf()
    {
        var codes = Helper.GenerateCode("""
#if !MASTER_MEMORY_DISABLED
using MasterMemory;
using System.Text;
#endif

namespace Example1
{
#if !MASTER_MEMORY_DISABLED
    [MemoryTable("person")]
#endif
    public record Person
    {
#if !MASTER_MEMORY_DISABLED
        [PrimaryKey]
#endif
        public int Id { get; set; }
    }
}
""");

        ShouldHaveNoSyntaxError(codes);
        codes["MasterMemory.PersonTable.g.cs"].ShouldNotContain("#if !MASTER_MEMORY_DISABLED");
        codes["MasterMemory.PersonTable.g.cs"].ShouldContain("TableBase<global::Example1.Person>");
    }

    // a file-scoped namespace is not a NamespaceDeclarationSyntax, so no "using Foo.Bar;" was generated for it
    [Fact]
    public void FileScopedNamespace()
    {
        var codes = Helper.GenerateCode("""
namespace Foo.Bar;

public enum Kind { A, B }

[MemoryTable("person")]
public class Person
{
    [PrimaryKey] public int Id { get; set; }
    [SecondaryKey(0), NonUnique] public Kind Kind { get; set; }
}
""");

        ShouldHaveNoSyntaxError(codes);
        codes["MasterMemory.DatabaseBuilder.g.cs"].ShouldContain("IEnumerable<global::Foo.Bar.Person>");
        codes["MasterMemory.PersonTable.g.cs"].ShouldContain("Func<global::Foo.Bar.Person, global::Foo.Bar.Kind>");
        codes["MasterMemory.PersonTable.g.cs"].ShouldContain("FindByKind(global::Foo.Bar.Kind key)");
    }

    [Fact]
    public void NestedType()
    {
        var codes = Helper.GenerateCode("""
namespace Foo
{
    public class Outer
    {
        [MemoryTable("inner")]
        public class Inner
        {
            [PrimaryKey] public int Id { get; set; }
        }
    }
}
""");

        codes["MasterMemory.InnerTable.g.cs"].ShouldContain("public sealed partial class InnerTable : TableBase<global::Foo.Outer.Inner>");
        codes["MasterMemory.MasterMemoryFormatterFactory.g.cs"].ShouldContain(".Add<global::Foo.Outer.Inner>()");
    }

    // how a key is sorted and searched: a database is stored in the order of its primary key, so none of this may move
    [Fact]
    public void KeyComparers()
    {
        var codes = Helper.GenerateCode("""
#nullable enable
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public string Name { get; set; } = default!;
    [SecondaryKey(0)] public string? Alias { get; set; }
    [SecondaryKey(3), StringComparisonOption(System.StringComparison.OrdinalIgnoreCase)] public string? Code { get; set; }
    [SecondaryKey(1)] public long Serial { get; set; }
    [SecondaryKey(2), NonUnique] public int Level { get; set; }
    [SecondaryKey(2, 1), NonUnique] public System.DateTime CreatedAt { get; set; }
}
""");

        var code = codes["MasterMemory.ItemTable.g.cs"];
        WriteLine(code);

        code.ShouldContain("FindUniqueCore(data, primaryIndexSelector, System.StringComparer.Ordinal, key, true)");
        // a nullable string is compared like a string: ordinal unless told otherwise, never by the current culture
        code.ShouldContain("CloneAndSortBy(this.secondaryIndex0Selector, System.StringComparer.Ordinal)");
        code.ShouldContain("CloneAndSortBy(this.secondaryIndex3Selector, System.StringComparer.OrdinalIgnoreCase)");
        code.ShouldContain("Func<global::Item, string?> secondaryIndex0Selector");
        code.ShouldNotContain("Comparer<string?>");
        code.ShouldContain("CloneAndSortBy(this.secondaryIndex1Selector, System.Collections.Generic.Comparer<long>.Default)");
        code.ShouldContain("System.Collections.Generic.Comparer<(int Level, global::System.DateTime CreatedAt)>.Default");
    }

    // issue 108: the same file name was added twice, which threw and left the project without any generated code
    [Fact]
    public void SameClassNameInTwoNamespaces()
    {
        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator("""
namespace A
{
    [MemoryTable("a_item")]
    public class Item
    {
        [PrimaryKey] public int Id { get; set; }
    }
}
namespace B
{
    [MemoryTable("b_item")]
    public class Item
    {
        [PrimaryKey] public int Id { get; set; }
    }
}
namespace C
{
    [MemoryTable("other")]
    public class Other
    {
        [PrimaryKey] public int Id { get; set; }
    }
}
""");

        foreach (var item in diagnostics)
        {
            WriteLine(item.ToString());
        }

        diagnostics.Select(x => x.Id).ToArray().ShouldBe(new[] { "MAM004", "MAM004" });
        diagnostics[0].GetMessage().ShouldContain("global::A.Item, global::B.Item");

        // the table that is not part of the clash is still generated
        var generated = compilation.SyntaxTrees.Select(x => Path.GetFileName(x.FilePath)).ToArray();
        generated.ShouldContain("MasterMemory.OtherTable.g.cs");
        generated.ShouldNotContain("MasterMemory.ItemTable.g.cs");
    }

    [Fact]
    public void ReturnNullIfKeyNotFound()
    {
        var codes = Helper.GenerateCode("""
[assembly: MasterMemoryGeneratorOptions(IsReturnNullIfKeyNotFound = true)]

[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int ItemId { get; set; }
    [SecondaryKey(0)] public string Name { get; set; }
    [SecondaryKey(1), NonUnique] public int Level { get; set; }
}
""");

        var code = codes["MasterMemory.ItemTable.g.cs"];
        WriteLine(code);

        // every unique key says it in its return type, not only the inlined numeric primary key
        code.ShouldContain("public global::Item? FindByItemId(int key)");
        code.ShouldContain("public global::Item? FindByName(string key)");
        code.ShouldContain("System.StringComparer.Ordinal, key, false);");
        code.ShouldContain("public RangeView<global::Item> FindByLevel(int key)");
    }

    [Fact]
    public void ThrowIfKeyNotFoundByDefault()
    {
        var codes = Helper.GenerateCode("""
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int ItemId { get; set; }
    [SecondaryKey(0)] public string Name { get; set; }
}
""");

        var code = codes["MasterMemory.ItemTable.g.cs"];
        code.ShouldContain("public global::Item FindByItemId(int key)");
        code.ShouldContain("public global::Item FindByName(string key)");
    }

    // issues 112 and 115: a place to add members of one's own, such as a query across tables
    [Fact]
    public void MemoryDatabaseIsPartial()
    {
        var codes = Helper.GenerateCode("""
[MemoryTable("item")]
public class Item
{
    [PrimaryKey] public int ItemId { get; set; }
}
""");

        codes["MasterMemory.MemoryDatabase.g.cs"].ShouldContain("public sealed partial class MemoryDatabase : MemoryDatabaseBase");
    }
}
