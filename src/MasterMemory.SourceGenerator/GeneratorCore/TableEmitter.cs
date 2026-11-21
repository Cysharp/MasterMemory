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
/// with is exactly what the index was sorted with (KeyBase.BuildComparer), member by member.
/// </remarks>
static class TableEmitter
{
    const string AggressiveInlining = "[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";

    public static string Emit(string @namespace, GenerationContext table, bool throwIfKeyNotFound)
    {
        var writer = CodeWriter.Rent();
        var element = table.FullTypeName;
        var primaryKey = table.PrimaryKey;

        using (DatabaseModel.BeginFile(writer, DatabaseModel.CommonUsings, @namespace + ".Tables"))
        using (writer.Block($"public sealed partial class {table.ClassName}Table : TableBase<{element}>, ITableUniqueValidate"))
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
                    var member = primaryKey.Properties[0].Name;
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
                    writer.Line($"this.{key.TableName} = CloneAndSortBy(this.{key.SelectorName}, {key.BuildComparer()});");
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

            foreach (var key in table.Keys)
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
            }

            writer.Line();
            using (writer.Block("void ITableUniqueValidate.ValidateUnique(ValidateResult resultSet)"))
            {
                writer.Directive("#if !DISABLE_MASTERMEMORY_VALIDATOR");
                writer.Line();
                foreach (var key in table.Keys)
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
        var keyType = key.BuildTypeName();
        var index = key.TableName;
        var comparable = ComparableName(key);
        var nullable = throwIfKeyNotFound ? "" : "?";
        var notFound = throwIfKeyNotFound ? "return ThrowKeyNotFound(key);" : "return default;";

        writer.Line(AggressiveInlining);
        using (writer.Block($"public {element}{nullable} FindBy{methodName}({keyType} key)"))
        {
            if (key.CanInlineBinarySearch)
            {
                EmitDirectHit(writer, key, "return row;");
            }
            writer.Line($"var index = FindIndexBy{methodName}(key);");
            writer.Line($"if (index >= 0) {{ return {index}[index]; }}");
            writer.Line(notFound);
        }

        writer.Line();
        writer.Line(AggressiveInlining);
        using (writer.Block($"public bool TryFindBy{methodName}({keyType} key, out {element} result)"))
        {
            if (key.CanInlineBinarySearch)
            {
                EmitDirectHit(writer, key, "result = row; return true;");
            }
            writer.Line($"var index = FindIndexBy{methodName}(key);");
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
        using (writer.Block($"int FindIndexBy{methodName}({keyType} key)"))
        {
            writer.Line($"var comparable = new {comparable}(key);");
            writer.Line($"return {search};");
        }

        writer.Line();
        using (writer.Block($"public {element}? FindClosestBy{methodName}({keyType} key, bool selectLower = true)"))
        {
            writer.Line($"return FindUniqueClosestCore({index}, new {comparable}(key), selectLower);");
        }

        writer.Line();
        using (writer.Block($"public RangeView<{element}> FindRangeBy{methodName}({keyType} min, {keyType} max, bool ascendant = true)"))
        {
            writer.Line($"return FindUniqueRangeCore({index}, new {comparable}(min), new {comparable}(max), ascendant);");
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
                if (row.{{key.Properties[0].Name}} == key) { {{onHit}} }
            }

            """);
    }

    // several rows per key: Find and FindClosest return every row of the key (of the nearest key), FindRange as above
    static void EmitNonUniqueLookups(CodeWriter writer, string element, KeyBase key)
    {
        var methodName = key.BuildMethodName();
        var keyType = key.BuildTypeName();
        var index = key.TableName;
        var comparable = ComparableName(key);

        using (writer.Block($"public RangeView<{element}> FindBy{methodName}({keyType} key)"))
        {
            writer.Line($"return FindManyCore({index}, new {comparable}(key));");
        }

        writer.Line();
        using (writer.Block($"public RangeView<{element}> FindClosestBy{methodName}({keyType} key, bool selectLower = true)"))
        {
            writer.Line($"var closest = FindClosestIndex({index}, new {comparable}(key), selectLower);");
            writer.Line($"if ((closest == -1) || (closest >= {index}.Length)) {{ return RangeView<{element}>.Empty; }}");
            writer.Line();
            writer.Line("// every row that shares the key of the nearest one");
            writer.Line($"var row = {index}[closest];");
            writer.Line($"return FindManyCore({index}, new {comparable}({key.BuildKeyAccessor("row")}));");
        }

        writer.Line();
        using (writer.Block($"public RangeView<{element}> FindRangeBy{methodName}({keyType} min, {keyType} max, bool ascendant = true)"))
        {
            writer.Line($"return FindManyRangeCore({index}, new {comparable}(min), new {comparable}(max), ascendant);");
        }
    }

    // The key of a lookup, as the searches take it. CompareTo orders the key and a row exactly as the comparer the index
    // is sorted with orders two keys; a different order here would search a sorted array by another order.
    static void EmitComparable(CodeWriter writer, string element, KeyBase key)
    {
        var name = ComparableName(key);
        var keyType = key.BuildTypeName();
        var branchless = IsBranchless(key);

        writer.Line("// the key of a lookup: negative, zero or positive as it is before, at or after a row");
        using (writer.Block($"readonly struct {name} : {(branchless ? "IBranchlessComparable" : "IComparable")}<{element}>"))
        {
            writer.Line($"readonly {keyType} key;");
            writer.Line();
            using (writer.Block($"public {name}({keyType} key)"))
            {
                writer.Line("this.key = key;");
            }

            writer.Line();
            writer.Line(AggressiveInlining);
            using (writer.Block($"public int CompareTo({element} other)"))
            {
                var properties = key.Properties;
                if (properties.Length == 1)
                {
                    writer.Line($"return {CompareExpression(key, properties[0], "key", "other." + properties[0].Name)};");
                }
                else
                {
                    // member by member, stopping at the first that differs: what the tuple's own comparison does
                    writer.Line("int c;");
                    for (var i = 0; i < properties.Length; i++)
                    {
                        var member = properties[i].Name;
                        var expression = CompareExpression(key, properties[i], "key." + member, "other." + member);
                        writer.Line((i == properties.Length - 1)
                            ? $"return {expression};"
                            : $"c = {expression}; if (c != 0) {{ return c; }}");
                    }
                }
            }

            if (branchless)
            {
                writer.Line();
                writer.Line("// (other - key - 1) is negative exactly when other <= key, and exact in a long; its sign, spread over the word, is the mask");
                writer.Line(AggressiveInlining);
                using (writer.Block($"public int AtOrAfterMask({element} other)"))
                {
                    writer.Line($"return unchecked((int)(((long)other.{key.Properties[0].Name} - key - 1) >> 63));");
                }
            }
        }
    }

    static string ComparableName(KeyBase key)
    {
        return (key is SecondaryKey secondary) ? $"SecondaryIndex{secondary.IndexNo}Comparable" : "PrimaryIndexComparable";
    }

    // "negative, zero or positive as left is before, at or after right", in the order the index is sorted by
    static string CompareExpression(KeyBase key, KeyProperty property, string left, string right)
    {
        if (property.IsComparableNumberType)
        {
            return $"({left} < {right}) ? -1 : ({left} > {right}) ? 1 : 0";
        }

        if (property.EnumUnderlyingType != null)
        {
            // Comparer<TEnum>.Default orders by the underlying value; comparing that value says the same without the call
            var type = property.EnumUnderlyingType;
            return $"(({type}){left} < ({type}){right}) ? -1 : (({type}){left} > ({type}){right}) ? 1 : 0";
        }

        if (key.IsStringType)
        {
            // a key that is one string is sorted with a StringComparer: ordinal unless [StringComparisonOption] says otherwise
            var option = key.StringComparisonOption?.Split('.').Last();
            return (option == null || option == "Ordinal")
                ? $"string.CompareOrdinal({left}, {right})"
                : $"string.Compare({left}, {right}, System.StringComparison.{option})";
        }

        // Every other type, and every member of a composite key that is not an integer or an enum (a string included: a
        // composite key is sorted with the tuple's default comparer, which compares its strings by Comparer<string>.Default).
        return $"System.Collections.Generic.Comparer<{property.FullTypeName}>.Default.Compare({left}, {right})";
    }

    // keys a search can halve its range for without a jump: unique, one integer of 32 bits or less
    static bool IsBranchless(KeyBase key)
    {
        return !key.IsNonUnique && key.IsComparableNumberType && UnsignedDistanceType(key) == "uint";
    }

    // The type the distance between two keys is computed in. A difference of sorted keys never exceeds the unsigned
    // range of their width, so unchecked subtraction followed by this cast is exact even across the sign.
    static string UnsignedDistanceType(KeyBase key)
    {
        return key.Properties[0].TypeName switch
        {
            "long" or "Int64" or "System.Int64" or "ulong" or "UInt64" or "System.UInt64" => "ulong",
            _ => "uint",
        };
    }

    static void EmitCreateMetaTable(CodeWriter writer, GenerationContext table)
    {
        var element = table.FullTypeName;

        using (writer.Block("public static MasterMemory.Meta.MetaTable CreateMetaTable()"))
        {
            writer.Line($"return new MasterMemory.Meta.MetaTable(typeof({element}), typeof({table.ClassName}Table), \"{table.MemoryTableName}\",");
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
                    using (writer.OpenScope($", {isPrimary}, {isUnique}, {key.BuildComparer()}),"))
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
