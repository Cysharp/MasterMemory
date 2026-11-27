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

        // an enum is compared by its underlying value, which is what Comparer<Kind>.Default orders by
        codes["MasterMemory.PersonTable.g.cs"].ShouldContain("return ((int)key < (int)other.Kind) ? -1 : ((int)key > (int)other.Kind) ? 1 : 0;");
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
        codes["MasterMemory.MasterMemoryTableFormatters.g.cs"].ShouldContain("registry.RegisterHarvested<global::Foo.Outer.Inner[]>(new global::MessagePack.Formatters.ArrayFormatterFactory<global::Foo.Outer.Inner>());");
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

        // sorting: the comparer an index is built with, a struct the sort takes as a type argument
        // a composite key has its own, which compares the members as the lookups do (and not the tuple's default one)
        code.ShouldContain("CloneAndSortBy(this.secondaryIndex2Selector, default(SecondaryIndex2KeyComparer), ref sortBuffer)");
        code.ShouldContain("internal readonly struct SecondaryIndex2KeyComparer : System.Collections.Generic.IComparer<(int Level, global::System.DateTime CreatedAt)>, System.Collections.IComparer");
        code.ShouldContain("c = (x.Level < y.Level) ? -1 : (x.Level > y.Level) ? 1 : 0; if (c != 0) { return c; }");
        code.ShouldContain("return System.Collections.Generic.Comparer<global::System.DateTime>.Default.Compare(x.CreatedAt, y.CreatedAt);");
        code.ShouldNotContain("Comparer<(int Level, global::System.DateTime CreatedAt)>.Default");

        // searching: what the struct of each key compares with, which has to be the same order
        code.ShouldContain("return string.CompareOrdinal(key, other.Name);");
        code.ShouldContain("return string.CompareOrdinal(key, other.Alias);");
        code.ShouldContain("return string.Compare(key, other.Code, System.StringComparison.OrdinalIgnoreCase);");
        code.ShouldContain("return (key < other.Serial) ? -1 : (key > other.Serial) ? 1 : 0;");
        code.ShouldContain("c = (key.Level < other.Level) ? -1 : (key.Level > other.Level) ? 1 : 0; if (c != 0) { return c; }");
        code.ShouldContain("return System.Collections.Generic.Comparer<global::System.DateTime>.Default.Compare(key.CreatedAt, other.CreatedAt);");
        // a nullable string is compared like a string: ordinal unless told otherwise, never by the current culture
        code.ShouldContain("CloneAndSortBy(this.secondaryIndex0Selector, System.StringComparer.Ordinal, ref sortBuffer)");
        code.ShouldContain("CloneAndSortBy(this.secondaryIndex3Selector, System.StringComparer.OrdinalIgnoreCase, ref sortBuffer)");
        code.ShouldContain("Func<global::Item, string?> secondaryIndex0Selector");
        code.ShouldNotContain("Comparer<string?>");
        code.ShouldContain("CloneAndSortBy(this.secondaryIndex1Selector, System.Collections.Generic.Comparer<long>.Default, ref sortBuffer)");

        // A key of one member that is not an enum is sorted by a comparer of the runtime, which every key of its type
        // shares; only the composite key has a comparer generated. The sorts of a table work in one buffer.
        code.Split("KeyComparer :").Length.ShouldBe(2);
        code.Split("int[]? sortBuffer = null;").Length.ShouldBe(2);
        code.ShouldNotContain("System.Array.Sort");
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
        code.ShouldContain("return default;");
        code.ShouldNotContain("return ThrowKeyNotFound(key);");
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

    // which lookup a primary key gets: the direct hit and the struct comparable need a number, the jump-free search 32 bits
    [Fact]
    public void PrimaryKeyLookupByKeyType()
    {
        var codes = Helper.GenerateCode("""
[MemoryTable("a")] public class IntKey { [PrimaryKey] public int Id { get; set; } }
[MemoryTable("b")] public class LongKey { [PrimaryKey] public long Id { get; set; } }
[MemoryTable("c")] public class StringKey { [PrimaryKey] public string Id { get; set; } }
[MemoryTable("d")] public class NonUniqueKey { [PrimaryKey, NonUnique] public int Id { get; set; } }
[MemoryTable("e")] public class SecondaryOnly { [PrimaryKey] public string Id { get; set; } [SecondaryKey(0)] public int No { get; set; } }
""");

        var intKey = codes["MasterMemory.IntKeyTable.g.cs"];
        WriteLine(intKey);
        intKey.ShouldContain("readonly int primaryIndexFirstKey;");
        intKey.ShouldContain("var offset = unchecked((uint)(key - primaryIndexFirstKey));");
        intKey.ShouldContain("return (data.Length <= BranchlessSearchMaxLength) ? FindIndexByBranchlessSearch(data, comparable) : FindIndexByBinarySearch(data, comparable);");
        intKey.ShouldContain("readonly struct PrimaryIndexComparable : IBranchlessComparable<global::IntKey>");
        intKey.ShouldContain("return unchecked((int)(((long)other.Id - key - 1) >> 63));");

        // 64 bits: the difference of two keys does not fit a long, so an ordinary comparison and the jumping search
        var longKey = codes["MasterMemory.LongKeyTable.g.cs"];
        longKey.ShouldContain("readonly long primaryIndexFirstKey;");
        longKey.ShouldContain("var offset = unchecked((ulong)(key - primaryIndexFirstKey));");
        longKey.ShouldContain("return FindIndexByBinarySearch(data, comparable);");
        longKey.ShouldContain("readonly struct PrimaryIndexComparable : IComparable<global::LongKey>");
        longKey.ShouldContain("return (key < other.Id) ? -1 : (key > other.Id) ? 1 : 0;");
        longKey.ShouldNotContain("FindIndexByBranchlessSearch");
        longKey.ShouldNotContain("AtOrAfterMask");

        // no direct hit for a primary key that is not a unique number, and no jump-free search for a string or a shared key
        foreach (var name in new[] { "StringKey", "NonUniqueKey", "SecondaryOnly" })
        {
            codes[$"MasterMemory.{name}Table.g.cs"].ShouldNotContain("primaryIndexFirstKey");
        }
        codes["MasterMemory.StringKeyTable.g.cs"].ShouldContain("readonly struct PrimaryIndexComparable : IComparable<global::StringKey>");
        codes["MasterMemory.NonUniqueKeyTable.g.cs"].ShouldContain("readonly struct PrimaryIndexComparable : IComparable<global::NonUniqueKey>");
        codes["MasterMemory.NonUniqueKeyTable.g.cs"].ShouldContain("return FindManyCore(data, new PrimaryIndexComparable(key));");

        // a unique integer secondary key is searched like a primary one, over its own index
        var secondary = codes["MasterMemory.SecondaryOnlyTable.g.cs"];
        secondary.ShouldContain("readonly struct SecondaryIndex0Comparable : IBranchlessComparable<global::SecondaryOnly>");
        secondary.ShouldContain("return (secondaryIndex0.Length <= BranchlessSearchMaxLength) ? FindIndexByBranchlessSearch(secondaryIndex0, comparable) : FindIndexByBinarySearch(secondaryIndex0, comparable);");
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

    // What is generated for the database as a whole grows with the number of tables, and no faster. The class that
    // changed the tables of a loaded database used to construct the database, naming every table, in three methods
    // of every table: 168 KB of source for some thirty tables.
    [Fact]
    public void DatabaseWideCodeGrowsWithTheNumberOfTables()
    {
        Dictionary<string, string> Generate(int tables)
        {
            return Helper.GenerateCode(string.Join("\n", Enumerable.Range(0, tables).Select(i => $$"""
[MemoryTable("t{{i:D3}}")]
public class T{{i:D3}}
{
    [PrimaryKey] public int Id { get; set; }
}
""")));
        }

        var ten = Generate(10);
        var eighty = Generate(80);
        ShouldHaveNoSyntaxError(eighty);

        // eight times the tables: eight times the code, not sixty-four times
        foreach (var file in new[] { "MasterMemory.DatabaseBuilder.g.cs", "MasterMemory.MemoryDatabase.g.cs", "MasterMemory.MasterMemoryTableFormatters.g.cs" })
        {
            ((double)eighty[file].Length / ten[file].Length).ShouldBeLessThan(8.5, file);
        }

        // the database is made from the builder itself, and no constructor has a parameter per table
        var builder = eighty["MasterMemory.DatabaseBuilder.g.cs"];
        builder.ShouldContain("return new MemoryDatabase(this);");
        (builder.Split("new MemoryDatabase(").Length - 1).ShouldBe(1);
        (builder.Split("T079Table").Length - 1).ShouldBeLessThan(12);

        // AppendDynamic goes through a branch per table that names its row type: no reflection, for trimming and Native AOT
        builder.ShouldContain("protected override bool AppendDynamicCore(Type dataType, System.Collections.Generic.IList<object> tableData)");
        builder.ShouldContain("if (dataType == typeof(global::T079)) { Append(CastRows<global::T079>(tableData)); return true; }");
        builder.ShouldNotContain("GetMethod");

        var database = eighty["MasterMemory.MemoryDatabase.g.cs"];
        database.ShouldContain("internal MemoryDatabase(DatabaseBuilder builder)");
        database.ShouldContain("this.T079Table = builder.BuildT079Table();");
        database.ShouldNotContain("T078Table T078Table,");

        // The formatter of every table array goes into MessagePack's own registry of generated formatters, which its
        // chains look in first: a line per table, in the static constructor of a class of its own. RegisterHarvested
        // leaves an entry that is already there alone, so a formatter the application declared for an array stays.
        var formatters = eighty["MasterMemory.MasterMemoryTableFormatters.g.cs"];
        formatters.ShouldContain("internal static class MasterMemoryTableFormatters");
        formatters.ShouldContain("static MasterMemoryTableFormatters()");
        formatters.ShouldContain("var registry = global::MessagePack.SourceGeneratedFormatterFactory.Instance;");
        formatters.ShouldContain("registry.RegisterHarvested<global::T079[]>(new global::MessagePack.Formatters.ArrayFormatterFactory<global::T079>());");
        (formatters.Split("RegisterHarvested<").Length - 1).ShouldBe(80);
        formatters.ShouldContain("internal static global::MasterMemory.MemoryDatabaseOptions? Registered(global::MasterMemory.MemoryDatabaseOptions? options)");

        // no factory of MasterMemory's own any more, no module initializer, and nothing that depends on the target framework
        eighty.Keys.ShouldNotContain("MasterMemory.MasterMemoryFormatterFactory.g.cs");
        foreach (var code in eighty)
        {
            code.Value.ShouldNotContain("ModuleInitializer", customMessage: code.Key);
            code.Value.ShouldNotContain("#if", customMessage: code.Key);
            code.Value.ShouldNotContain("MasterMemoryFormatterFactory", customMessage: code.Key);

            // no processor is named in the code: the comments of the constructors say which one to pass
            var withoutComments = string.Join("\n", code.Value.Split('\n').Where(x => !x.TrimStart().StartsWith("//", StringComparison.Ordinal)));
            withoutComments.Contains("Lz4Block", StringComparison.Ordinal).ShouldBeFalse(code.Key);
        }

        // A database is read from an array, from memory (both can be read in parallel) and from a span. How it is read
        // is in MemoryDatabaseOptions, which is optional and goes through the registration to the base class as it is.
        const string options = "MasterMemory.MemoryDatabaseOptions? options";
        database.ShouldContain($"public MemoryDatabase(byte[] databaseBinary, {options} = null, int maxDegreeOfParallelism = 1)");
        database.ShouldContain($"public MemoryDatabase(System.ReadOnlyMemory<byte> databaseBinary, {options} = null, int maxDegreeOfParallelism = 1)");
        database.ShouldContain($"public MemoryDatabase(System.ReadOnlySpan<byte> databaseBinary, {options} = null)");
        (database.Split("public MemoryDatabase(").Length - 1).ShouldBe(3);
        (database.Split(": base(databaseBinary, MasterMemoryTableFormatters.Registered(options)").Length - 1).ShouldBe(3);

        // a builder made from a database writes with the options of the database unless it is given others, and a
        // database made from a builder has the options of the builder
        database.ShouldContain($"public DatabaseBuilder ToDatabaseBuilder({options} = null)");
        database.ShouldContain("return new DatabaseBuilder(this, options ?? Options);");
        database.ShouldContain(": base(builder.Options)");

        // new() finds the builder without parameters
        builder.ShouldContain("public DatabaseBuilder() : this(null) { }");
        builder.ShouldContain($"public DatabaseBuilder({options}) : base(MasterMemoryTableFormatters.Registered(options)) {{ }}");
        builder.ShouldContain($"internal DatabaseBuilder(MemoryDatabase database, {options}) : base(MasterMemoryTableFormatters.Registered(options))");
        eighty.Keys.ShouldNotContain("MasterMemory.ImmutableBuilder.g.cs");
    }
}
