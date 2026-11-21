using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// Emits one table class: the secondary indexes (each a copy of the rows sorted by that key), the Find / TryFind /
/// FindClosest / FindRange methods of every key, the uniqueness validation and the metadata of the table.
/// </summary>
/// <remarks>
/// Every lookup goes through a struct declared per key that compares the key to a row. The searches are TableBase's and
/// take that struct as a type argument (the shape of SpanHelpers.BinarySearch in the BCL), so the comparison of each key
/// is compiled into the search: no selector delegate, no IComparer, no tuple built per probe. What the struct compares
/// with is exactly what the index was sorted with (KeyBase.BuildComparer), member by member: for a composite key both
/// are written here from the same expression (CompareExpression), so they cannot drift apart.
/// </remarks>
static class TableEmitter
{
    const string AggressiveInlining = "[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";

    public static string Emit(string @namespace, GenerationContext table, bool throwIfKeyNotFound)
    {
        var writer = CodeWriter.Rent();
        var element = table.FullTypeName;
        var primaryKey = table.PrimaryKey;
        var keys = table.Keys;

        using (DatabaseModel.BeginFile(writer, DatabaseModel.CommonUsings, @namespace + ".Tables"))
        using (writer.Block($"{table.Accessibility} sealed partial class {table.ClassName}Table : TableBase<{element}>, ITableUniqueValidate"))
        {
            writer.Line($"public Func<{element}, {primaryKey.BuildTypeName()}> PrimaryKeySelector => {primaryKey.SelectorName};");
            writer.Line($"readonly Func<{element}, {primaryKey.BuildTypeName()}> {primaryKey.SelectorName};");
            if (primaryKey.CanInlineBinarySearch)
            {
                writer.Line("// When the primary keys run without a gap (1, 2, 3, ...), a row is at (key - first key) and needs no search:");
                writer.Line("// primaryIndexDenseLength is then the row count, and 0 for every other table.");
                writer.Line($"readonly {primaryKey.BuildTypeName()} primaryIndexFirstKey;");
                writer.Line("readonly int primaryIndexDenseLength;");
            }
            writer.Line();
            foreach (var key in table.SecondaryKeys)
            {
                writer.Line($"readonly {element}[] {key.TableName};");
                writer.Line($"readonly Func<{element}, {key.BuildTypeName()}> {key.SelectorName};");
            }

            writer.Line();
            writer.Line($"public {table.ClassName}Table({element}[] sortedData)");
            writer.Indent().Line(": base(sortedData)").Unindent();
            using (writer.OpenScope())
            {
                writer.Line($"this.{primaryKey.SelectorName} = x => {primaryKey.BuildKeyAccessor("x")};");
                if (primaryKey.CanInlineBinarySearch)
                {
                    // sorted rows are dense exactly when last - first == count - 1, provided the keys are unique; nothing
                    // enforces that, which is why a lookup checks the row it lands on
                    var member = primaryKey.Properties[0].Identifier;
                    var unsigned = UnsignedDistanceType(primaryKey);
                    writer.Lines($$"""
                        if (sortedData.Length != 0)
                        {
                            this.primaryIndexFirstKey = sortedData[0].{{member}};
                            if (unchecked(({{unsigned}})(sortedData[sortedData.Length - 1].{{member}} - sortedData[0].{{member}})) == ({{unsigned}})(sortedData.Length - 1))
                            {
                                this.primaryIndexDenseLength = sortedData.Length;
                            }
                        }
                        """);
                }
                foreach (var key in table.SecondaryKeys)
                {
                    writer.Line($"this.{key.SelectorName} = x => {key.BuildKeyAccessor("x")};");
                    writer.Line($"this.{key.TableName} = CloneAndSortBy(this.{key.SelectorName}, {key.BuildComparer("")});");
                }
                writer.Line("OnAfterConstruct();");
            }
            writer.Line();
            writer.Line("partial void OnAfterConstruct();");
            writer.Line();

            foreach (var key in table.SecondaryKeys)
            {
                writer.Line($"public RangeView<{element}> SortBy{key.BuildMethodName()} => new RangeView<{element}>({key.TableName}, 0, {key.TableName}.Length - 1, true);");
            }

            foreach (var key in keys)
            {
                writer.Line();
                if (key.IsNonUnique)
                {
                    EmitNonUniqueLookups(writer, element, key);
                }
                else
                {
                    EmitUniqueLookups(writer, element, key, throwIfKeyNotFound);
                }
                writer.Line();
                EmitComparable(writer, element, key);
                if (key.HasGeneratedComparer)
                {
                    writer.Line();
                    EmitKeyComparer(writer, key);
                }
            }

            foreach (var (key, length) in GetPrefixLookups(table))
            {
                writer.Line();
                EmitPrefixLookups(writer, element, key, length);
            }

            writer.Line();
            using (writer.Block("void ITableUniqueValidate.ValidateUnique(ValidateResult resultSet)"))
            {
                writer.Directive("#if !DISABLE_MASTERMEMORY_VALIDATOR");
                writer.Line();
                foreach (var key in keys)
                {
                    if (!key.IsNonUnique)
                    {
                        writer.Line($"ValidateUniqueCore({key.TableName}, {key.SelectorName}, \"{key.BuildPropertyTupleName()}\", resultSet);");
                    }
                }
                writer.Line();
                writer.Directive("#endif");
            }

            writer.Line();
            writer.Directive("#if !DISABLE_MASTERMEMORY_METADATABASE");
            writer.Line();
            EmitCreateMetaTable(writer, table);
            writer.Line();
            writer.Directive("#endif");
        }

        return writer.ToStringAndReturn();
    }

    // one row per key: Find, TryFind, FindClosest (the row at the key, or the nearest one), FindRange
    static void EmitUniqueLookups(CodeWriter writer, string element, KeyBase key, bool throwIfKeyNotFound)
    {
        var methodName = key.BuildMethodName();
        var parameters = new KeyParameters(key.Properties.ToArray());
        var index = key.TableName;
        var comparable = key.ComparableName;
        var nullable = throwIfKeyNotFound ? "" : "?";
        var notFound = throwIfKeyNotFound ? $"return ThrowKeyNotFound({parameters.Value});" : "return default;";

        writer.Line(AggressiveInlining);
        using (writer.Block($"public {element}{nullable} FindBy{methodName}({parameters.Declaration})"))
        {
            if (key.CanInlineBinarySearch)
            {
                EmitDirectHit(writer, key, "return row;");
            }
            writer.Line($"var index = FindIndexBy{methodName}({parameters.Arguments});");
            writer.Line($"if (index >= 0) {{ return {index}[index]; }}");
            writer.Line(notFound);
        }

        writer.Line();
        writer.Line(AggressiveInlining);
        using (writer.Block($"public bool TryFindBy{methodName}({parameters.Declaration}, out {element} result)"))
        {
            if (key.CanInlineBinarySearch)
            {
                EmitDirectHit(writer, key, "result = row; return true;");
            }
            writer.Line($"var index = FindIndexBy{methodName}({parameters.Arguments});");
            writer.Line($"if (index >= 0) {{ result = {index}[index]; return true; }}");
            writer.Line("result = default!;");
            writer.Line("return false;");
        }

        // Up to BranchlessSearchMaxLength rows a search costs the mispredicted jump of each step, so a key that can be
        // compared by arithmetic is searched without jumps there; a larger table is better served by the jumps, which
        // let the processor load the next row ahead of time.
        var search = IsBranchless(key)
            ? $"({index}.Length <= BranchlessSearchMaxLength) ? FindIndexByBranchlessSearch({index}, comparable) : FindIndexByBinarySearch({index}, comparable)"
            : $"FindIndexByBinarySearch({index}, comparable)";

        writer.Line();
        writer.Line("// the index of the row with the key, negative when there is none");
        writer.Line(AggressiveInlining);
        using (writer.Block($"int FindIndexBy{methodName}({parameters.Declaration})"))
        {
            writer.Line($"var comparable = new {comparable}({parameters.Arguments});");
            writer.Line($"return {search};");
        }

        writer.Line();
        using (writer.Block($"public {element}? FindClosestBy{methodName}({parameters.Declaration}, bool selectLower = true)"))
        {
            writer.Line($"return FindUniqueClosestCore({index}, new {comparable}({parameters.Arguments}), selectLower);");
        }

        writer.Line();
        using (writer.Block($"public RangeView<{element}> FindRangeBy{methodName}({parameters.MinDeclaration}, {parameters.MaxDeclaration}, bool ascendant = true)"))
        {
            writer.Line($"return FindUniqueRangeCore({index}, new {comparable}({parameters.MinArguments}), new {comparable}({parameters.MaxArguments}), ascendant);");
        }
    }

    /// <summary>
    /// How a key is passed to a lookup. A key of one member is "key" (and "min", "max"). A composite key is one
    /// parameter per member, named after the property: FindByGenderAndAge(Gender gender, int age), and for a range
    /// (minGender, minAge, maxGender, maxAge). It is not a tuple: a call reads as what it asks, and there is nothing
    /// to wrap in a second pair of parentheses.
    /// </summary>
    sealed class KeyParameters
    {
        // what the generated method bodies name their own variables, and the parameters they add
        static readonly HashSet<string> Reserved = new HashSet<string> { "key", "min", "max", "result", "selectLower", "ascendant", "index", "row", "closest", "comparable", "offset", "data" };

        public string Declaration { get; }
        public string Arguments { get; }
        public string MinDeclaration { get; }
        public string MinArguments { get; }
        public string MaxDeclaration { get; }
        public string MaxArguments { get; }
        // the key as one value, for a message
        public string Value { get; }

        public KeyParameters(KeyProperty[] properties)
        {
            if (properties.Length == 1)
            {
                var type = properties[0].FullTypeName;
                (Declaration, Arguments, Value) = ($"{type} key", "key", "key");
                (MinDeclaration, MinArguments) = ($"{type} min", "min");
                (MaxDeclaration, MaxArguments) = ($"{type} max", "max");
                return;
            }

            var names = new List<string>();
            foreach (var property in properties)
            {
                var name = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
                if (Reserved.Contains(name) || names.Contains(name))
                {
                    name += "Key" + (names.Count + 1);
                }
                names.Add(name);
            }

            // "class" is a parameter name only as "@class"
            string Escape(string name) => (Microsoft.CodeAnalysis.CSharp.SyntaxFacts.GetKeywordKind(name) != Microsoft.CodeAnalysis.CSharp.SyntaxKind.None) ? "@" + name : name;
            string Join(Func<int, string> item) => string.Join(", ", Enumerable.Range(0, properties.Length).Select(item));

            Declaration = Join(i => $"{properties[i].FullTypeName} {Escape(names[i])}");
            Arguments = Join(i => Escape(names[i]));
            Value = "(" + Arguments + ")";
            MinDeclaration = Join(i => $"{properties[i].FullTypeName} min{properties[i].Name}");
            MinArguments = Join(i => "min" + properties[i].Name);
            MaxDeclaration = Join(i => $"{properties[i].FullTypeName} max{properties[i].Name}");
            MaxArguments = Join(i => "max" + properties[i].Name);
        }

        // the key of a row, as arguments
        public static string ArgumentsOf(KeyProperty[] properties, string row)
        {
            return string.Join(", ", properties.Select(x => row + "." + x.Identifier));
        }
    }

    // A unique numeric primary key whose values run without a gap: the row is at (key - first key). The row found there
    // is checked, since nothing guarantees the keys are unique; a miss goes on to the search.
    static void EmitDirectHit(CodeWriter writer, KeyBase key, string onHit)
    {
        var unsigned = UnsignedDistanceType(key);
        writer.Lines($$"""
            var offset = unchecked(({{unsigned}})(key - primaryIndexFirstKey));
            if (offset < ({{unsigned}})primaryIndexDenseLength)
            {
                var row = data[(int)offset];
                if (row.{{key.Properties[0].Identifier}} == key) { {{onHit}} }
            }

            """);
    }

    // several rows per key: Find and FindClosest return every row of the key (of the nearest key), FindRange as above
    static void EmitNonUniqueLookups(CodeWriter writer, string element, KeyBase key)
    {
        var methodName = key.BuildMethodName();
        var properties = key.Properties.ToArray();
        var parameters = new KeyParameters(properties);
        var index = key.TableName;
        var comparable = key.ComparableName;

        using (writer.Block($"public RangeView<{element}> FindBy{methodName}({parameters.Declaration})"))
        {
            writer.Line($"return FindManyCore({index}, new {comparable}({parameters.Arguments}));");
        }

        writer.Line();
        using (writer.Block($"public RangeView<{element}> FindClosestBy{methodName}({parameters.Declaration}, bool selectLower = true)"))
        {
            writer.Line($"var closest = FindClosestIndex({index}, new {comparable}({parameters.Arguments}), selectLower);");
            writer.Line($"if ((closest == -1) || (closest >= {index}.Length)) {{ return RangeView<{element}>.Empty; }}");
            writer.Line();
            writer.Line("// every row that shares the key of the nearest one");
            writer.Line($"var row = {index}[closest];");
            writer.Line($"return FindManyCore({index}, new {comparable}({KeyParameters.ArgumentsOf(properties, "row")}));");
        }

        writer.Line();
        using (writer.Block($"public RangeView<{element}> FindRangeBy{methodName}({parameters.MinDeclaration}, {parameters.MaxDeclaration}, bool ascendant = true)"))
        {
            writer.Line($"return FindManyRangeCore({index}, new {comparable}({parameters.MinArguments}), new {comparable}({parameters.MaxArguments}), ascendant);");
        }
    }
    // The head of a composite key is a key too: an index sorted by (Gender, Age) is sorted by Gender, so it answers
    // FindByGender without an index of its own being built and kept in memory (the leftmost prefix of a composite
    // index, in the terms of a database). Every head gets its lookups, unless something is already named so: a key
    // declared on exactly those members comes first, then the head of the primary key, then the heads of the
    // secondary keys in the order of their index numbers. Which index answers decides only the order of the rows.
    static List<(KeyBase Key, int Length)> GetPrefixLookups(GenerationContext table)
    {
        var names = new HashSet<string>(table.Keys.Select(x => x.BuildMethodName()));
        var result = new List<(KeyBase Key, int Length)>();
        var candidates = new KeyBase[] { table.PrimaryKey }.Concat(table.SecondaryKeys.ToArray().OrderBy(x => x.IndexNo));
        foreach (var key in candidates)
        {
            for (var length = 1; length < key.Properties.Length; length++)
            {
                if (names.Add(PrefixMethodName(key, length)))
                {
                    result.Add((key, length));
                }
            }
        }
        return result;
    }

    static string PrefixMethodName(KeyBase key, int length)
    {
        return string.Join("And", key.Properties.ToArray().Take(length).Select(x => x.Name));
    }

    // Find and FindRange by the first members of a composite key. The rows that share them lie together in the index of
    // the whole key, in the order of its other members; the result is a range of that index, whether or not the whole
    // key is unique. The struct compares only the members it is given, with the comparisons the index is sorted by.
    static void EmitPrefixLookups(CodeWriter writer, string element, KeyBase key, int length)
    {
        var properties = key.Properties.ToArray().Take(length).ToArray();
        var methodName = PrefixMethodName(key, length);
        var parameters = new KeyParameters(properties);
        var comparable = $"{key.IndexName}Prefix{length}Comparable";
        var index = key.TableName;

        writer.Line($"// by the first {(length == 1 ? "member" : length + " members")} of the key {key.BuildPropertyTupleName()}, through its index: the rows come in the order of that key");
        using (writer.Block($"public RangeView<{element}> FindBy{methodName}({parameters.Declaration})"))
        {
            writer.Line($"return FindManyCore({index}, new {comparable}({parameters.Arguments}));");
        }

        writer.Line();
        using (writer.Block($"public RangeView<{element}> FindRangeBy{methodName}({parameters.MinDeclaration}, {parameters.MaxDeclaration}, bool ascendant = true)"))
        {
            writer.Line($"return FindManyRangeCore({index}, new {comparable}({parameters.MinArguments}), new {comparable}({parameters.MaxArguments}), ascendant);");
        }

        writer.Line();
        EmitComparableStruct(writer, element, comparable, properties, "IComparable", null);
    }
    // The key of a lookup, as the searches take it. CompareTo orders the key and a row exactly as the comparer the index
    // is sorted with orders two keys; a different order here would search a sorted array by another order.
    static void EmitComparable(CodeWriter writer, string element, KeyBase key)
    {
        var branchless = IsBranchless(key);
        writer.Line("// the key of a lookup: negative, zero or positive as it is before, at or after a row");
        EmitComparableStruct(writer, element, key.ComparableName, key.Properties.ToArray(), branchless ? "IBranchlessComparable" : "IComparable", branchless ? key.Properties[0] : null);
    }

    // A struct that holds a key, or the first members of one, and compares it to a row member by member. The members of
    // a composite key arrive one by one, as the lookups take them, and are kept together.
    static void EmitComparableStruct(CodeWriter writer, string element, string name, KeyProperty[] properties, string @interface, KeyProperty? branchless)
    {
        var parameters = new KeyParameters(properties);
        var keyType = (properties.Length == 1)
            ? properties[0].FullTypeName
            : "(" + string.Join(", ", properties.Select(x => x.FullTypeName + " " + x.Identifier)) + ")";

        using (writer.Block($"readonly struct {name} : {@interface}<{element}>"))
        {
            writer.Line($"readonly {keyType} key;");
            writer.Line();
            using (writer.Block($"public {name}({parameters.Declaration})"))
            {
                writer.Line($"this.key = {parameters.Value};");
            }

            writer.Line();
            writer.Line(AggressiveInlining);
            using (writer.Block($"public int CompareTo({element} other)"))
            {
                if (properties.Length == 1)
                {
                    writer.Line($"return {CompareExpression(properties[0], "key", "other." + properties[0].Identifier)};");
                }
                else
                {
                    EmitMemberwiseCompare(writer, properties, "key", "other");
                }
            }

            if (branchless != null)
            {
                writer.Line();
                writer.Line("// (other - key - 1) is negative exactly when other <= key, and exact in a long; its sign, spread over the word, is the mask");
                writer.Line(AggressiveInlining);
                using (writer.Block($"public int AtOrAfterMask({element} other)"))
                {
                    writer.Line($"return unchecked((int)(((long)other.{branchless.Identifier} - key - 1) >> 63));");
                }
            }
        }
    }
    // The order of a composite key or of an enum key, as the sorts take it: what DatabaseBuilder sorts the rows by, what
    // a secondary index is sorted by when a table is constructed, and what MetaIndex.Comparer returns. The default
    // comparers are not used for these. That of a tuple compares strings by the current culture: the machine that
    // builds the database and the device that loads it could then disagree on the order of the same rows. That of an
    // enum boxes both values on every comparison under Mono and IL2CPP, which is garbage on every load of a table.
    static void EmitKeyComparer(CodeWriter writer, KeyBase key)
    {
        var name = key.KeyComparerName;
        var keyType = key.BuildTypeName();

        writer.Line("// the order of the index: the same comparisons a lookup makes");
        using (writer.Block($"internal sealed class {name} : System.Collections.Generic.IComparer<{keyType}>, System.Collections.IComparer"))
        {
            writer.Line($"public static readonly {name} Instance = new {name}();");
            writer.Line();
            using (writer.Block($"public int Compare({keyType} x, {keyType} y)"))
            {
                if (key.IsComposite)
                {
                    EmitMemberwiseCompare(writer, key, "x", "y");
                }
                else
                {
                    writer.Line($"return {CompareExpression(key.Properties[0], "x", "y")};");
                }
            }
            writer.Line();
            using (writer.Block("int System.Collections.IComparer.Compare(object? x, object? y)"))
            {
                writer.Line($"return Compare(({keyType})x!, ({keyType})y!);");
            }
        }
    }

    // member by member, stopping at the first that differs
    static void EmitMemberwiseCompare(CodeWriter writer, KeyBase key, string left, string right)
    {
        EmitMemberwiseCompare(writer, key.Properties.ToArray(), left, right);
    }

    static void EmitMemberwiseCompare(CodeWriter writer, KeyProperty[] properties, string left, string right)
    {
        writer.Line("int c;");
        for (var i = 0; i < properties.Length; i++)
        {
            var member = properties[i].Identifier;
            var expression = CompareExpression(properties[i], left + "." + member, right + "." + member);
            writer.Line((i == properties.Length - 1)
                ? $"return {expression};"
                : $"c = {expression}; if (c != 0) {{ return c; }}");
        }
    }

    // "negative, zero or positive as left is before, at or after right", in the order the index is sorted by
    static string CompareExpression(KeyProperty property, string left, string right)
    {
        if (property.IsComparableNumberType)
        {
            return $"({left} < {right}) ? -1 : ({left} > {right}) ? 1 : 0";
        }

        if (property.EnumUnderlyingType != null)
        {
            // Comparer<TEnum>.Default orders by the underlying value. Comparing that value says the same, and does not
            // box: under Mono and IL2CPP the default comparer of an enum allocates on every comparison.
            var type = property.EnumUnderlyingType;
            if (!property.IsNullableEnum)
            {
                return $"(({type}){left} < ({type}){right}) ? -1 : (({type}){left} > ({type}){right}) ? 1 : 0";
            }

            // TEnum?: null before every value, as Comparer<TEnum?>.Default has it
            var (l, r) = ($"({type}){left}.GetValueOrDefault()", $"({type}){right}.GetValueOrDefault()");
            return $"{left}.HasValue ? ({right}.HasValue ? (({l} < {r}) ? -1 : ({l} > {r}) ? 1 : 0) : 1) : ({right}.HasValue ? -1 : 0)";
        }

        if (property.Kind == KeyTypeKind.String)
        {
            // ordinal unless [StringComparisonOption] says otherwise, alone or as a member of a composite key
            var option = property.StringComparison;
            return (option == null || option == "Ordinal")
                ? $"string.CompareOrdinal({left}, {right})"
                : $"string.Compare({left}, {right}, System.StringComparison.{option})";
        }

        return $"System.Collections.Generic.Comparer<{property.FullTypeName}>.Default.Compare({left}, {right})";
    }

    // keys a search can halve its range for without a jump: unique, one integer of 32 bits or less
    static bool IsBranchless(KeyBase key)
    {
        return !key.IsNonUnique && !key.IsComposite && key.Properties[0].Kind == KeyTypeKind.Integer32;
    }

    // The type the distance between two keys is computed in. A difference of sorted keys never exceeds the unsigned
    // range of their width, so unchecked subtraction followed by this cast is exact even across the sign.
    static string UnsignedDistanceType(KeyBase key)
    {
        return (key.Properties[0].Kind == KeyTypeKind.Integer64) ? "ulong" : "uint";
    }

    static void EmitCreateMetaTable(CodeWriter writer, GenerationContext table)
    {
        var element = table.FullTypeName;

        using (writer.Block("public static MasterMemory.Meta.MetaTable CreateMetaTable()"))
        {
            writer.Line($"return new MasterMemory.Meta.MetaTable(typeof({element}), typeof({table.ClassName}Table), {table.MemoryTableNameLiteral},");
            writer.Indent();

            writer.Line("new MasterMemory.Meta.MetaProperty[]");
            using (writer.OpenScope(","))
            {
                foreach (var property in table.Properties)
                {
                    writer.Line($"new MasterMemory.Meta.MetaProperty(typeof({element}).GetProperty(\"{property.Name}\")!),");
                }
            }

            writer.Line("new MasterMemory.Meta.MetaIndex[]");
            using (writer.OpenScope(");"))
            {
                foreach (var key in table.Keys)
                {
                    writer.Line("new MasterMemory.Meta.MetaIndex(new System.Reflection.PropertyInfo[]");
                    var isPrimary = key.IsPrimary ? "true" : "false";
                    var isUnique = key.IsNonUnique ? "false" : "true";
                    using (writer.OpenScope($", {isPrimary}, {isUnique}, {key.BuildComparer("")}),"))
                    {
                        foreach (var keyProperty in key.Properties)
                        {
                            writer.Line($"typeof({element}).GetProperty(\"{keyProperty.Name}\")!,");
                        }
                    }
                }
            }

            writer.Unindent();
        }
    }
}
