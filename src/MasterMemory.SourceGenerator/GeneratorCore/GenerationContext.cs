namespace MasterMemory.GeneratorCore;

// What one [MemoryTable] class is emitted from. Everything here is text and numbers, compared by value: the incremental
// pipeline caches these between runs, so they hold no symbol, no syntax node and no compilation.
internal sealed record GenerationContext
{
    // the simple name: what the generated members are named after (ItemTable, RemoveItem)
    public string ClassName { get; init; } = "";
    // global::Namespace.Outer.Item: how generated code refers to the type, wherever it is declared
    public string FullTypeName { get; init; } = "";
    // "public" or "internal": what the type can be seen as, counting the types it is nested in. Generated members that
    // have the type in their signature are declared with it.
    public string Accessibility { get; init; } = "public";
    public string MemoryTableName { get; init; } = "";
    // MemoryTableName as a C# string literal, quotes included
    public string MemoryTableNameLiteral { get; init; } = "\"\"";
    public PrimaryKey PrimaryKey { get; init; } = new PrimaryKey();
    public EquatableArray<SecondaryKey> SecondaryKeys { get; init; } = new EquatableArray<SecondaryKey>([]);

    // the columns: public instance properties that are serialized
    public EquatableArray<Property> Properties { get; init; } = new EquatableArray<Property>([]);

    public KeyBase[] Keys => new KeyBase[] { PrimaryKey }.Concat(SecondaryKeys).ToArray();

    // What the database-wide files (builder, database, immutable builder, formatter factory) need of a table. They are
    // emitted again only when this changes, not when a secondary key or a column does.
    public GenerationContext ToDatabaseEntry()
    {
        return this with { SecondaryKeys = new EquatableArray<SecondaryKey>([]), Properties = new EquatableArray<Property>([]) };
    }
}

internal sealed record Property
{
    // as reflection names it
    public string Name { get; init; } = "";
}

internal abstract record KeyBase
{
    public bool IsNonUnique { get; init; }
    // sorted by KeyOrder; more than one is a composite key, handed around as a tuple
    public EquatableArray<KeyProperty> Properties { get; init; } = new EquatableArray<KeyProperty>([]);
    public abstract string SelectorName { get; }
    public abstract string TableName { get; }
    public abstract bool IsPrimary { get; }
    // PrimaryIndex, SecondaryIndex0, ...: what the types generated for the key are named after
    public abstract string IndexName { get; }

    // the key of a lookup, as the searches take it
    public string ComparableName => IndexName + "Comparable";
    // the order of the key, as the sorts take it, when one is generated for it
    public string KeyComparerName => IndexName + "KeyComparer";

    public bool IsComposite => Properties.Length != 1;

    // A comparer is generated for a composite key, and for an enum key. The default comparer of an enum boxes both
    // values on every comparison under Mono and IL2CPP (it goes through the non-generic IComparable), and the default
    // comparer of a tuple compares its strings by the current culture.
    public bool HasGeneratedComparer => IsComposite || Properties[0].EnumUnderlyingType != null;

    public string BuildKeyAccessor(string lambdaArgument)
    {
        if (Properties.Length == 1)
        {
            return lambdaArgument + "." + Properties[0].Identifier;
        }
        else
        {
            return "(" + string.Join(", ", Properties.Select(x => lambdaArgument + "." + x.Identifier)) + ")";
        }
    }

    public string BuildTypeName()
    {
        if (Properties.Length == 1)
        {
            return Properties[0].FullTypeName;
        }
        else
        {
            return "(" + string.Join(", ", Properties.Select(x => x.FullTypeName + " " + x.Identifier)) + ")";
        }
    }

    public string BuildMethodName()
    {
        if (Properties.Length == 1)
        {
            return Properties[0].Name;
        }
        else
        {
            return string.Join("And", Properties.Select(x => x.Name));
        }
    }

    public string BuildPropertyTupleName()
    {
        if (Properties.Length == 1)
        {
            return Properties[0].Name;
        }
        else
        {
            return "(" + string.Join(", ", Properties.Select(x => x.Name)) + ")";
        }
    }

    /// <summary>
    /// The comparer the index is sorted with. A composite key and an enum key have one generated in their table
    /// class, which makes exactly the comparisons the lookups do; <paramref name="tableClassQualifier"/> is how the
    /// caller reaches that class ("ItemTable." from another file, "" from inside it).
    /// </summary>
    public string BuildComparer(string tableClassQualifier)
    {
        if (HasGeneratedComparer)
        {
            return tableClassQualifier + KeyComparerName + ".Instance";
        }

        var property = Properties[0];
        if (property.Kind == KeyTypeKind.String)
        {
            return "System.StringComparer." + (property.StringComparison ?? "Ordinal");
        }
        return $"System.Collections.Generic.Comparer<{property.FullTypeName}>.Default";
    }

    public bool IsComparableNumberType => Properties.Length == 1 && Properties[0].IsComparableNumberType;

    // a unique primary key of one integer: read at (key - first key) when the keys run without a gap
    public bool CanInlineBinarySearch => IsPrimary && IsComparableNumberType && !IsNonUnique;
}

internal sealed record PrimaryKey : KeyBase
{
    public override string SelectorName => "primaryIndexSelector";
    public override string TableName => "data";
    public override bool IsPrimary => true;
    public override string IndexName => "PrimaryIndex";
}

internal sealed record SecondaryKey : KeyBase
{
    public int IndexNo { get; init; }
    public override string SelectorName => $"secondaryIndex{IndexNo}Selector";
    public override string TableName => $"secondaryIndex{IndexNo}";
    public override bool IsPrimary => false;
    public override string IndexName => $"SecondaryIndex{IndexNo}";
}

// what decides how a key member is compared
internal enum KeyTypeKind
{
    // Comparer<T>.Default (an enum: its underlying value)
    Other,
    // ordinal, unless [StringComparisonOption] says otherwise
    String,
    // sbyte, byte, short, ushort, char, int, uint: compared with < and >, and a distance between two fits a uint
    Integer32,
    // long, ulong: compared with < and >
    Integer64,
}

internal sealed record KeyProperty
{
    public int KeyOrder { get; init; }
    // as reflection names it, and what the generated methods are named after (FindByName)
    public string Name { get; init; } = "";
    // as code writes it: Name, with @ in front when it is a keyword
    public string Identifier { get; init; } = "";
    // fully qualified, from the symbol
    public string FullTypeName { get; init; } = "";
    public KeyTypeKind Kind { get; init; }
    // the integer type an enum, or a nullable enum, is compared in ("int", "byte", ...); null for every other type
    public string? EnumUnderlyingType { get; init; }
    // TEnum?: null is before every value
    public bool IsNullableEnum { get; init; }
    // the member name of System.StringComparison from [StringComparisonOption]; null when there is none
    public string? StringComparison { get; init; }

    // the integers a lookup compares with < and > directly
    public bool IsComparableNumberType => Kind == KeyTypeKind.Integer32 || Kind == KeyTypeKind.Integer64;
}
