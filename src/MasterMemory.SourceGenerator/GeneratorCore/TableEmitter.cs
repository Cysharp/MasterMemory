using MasterMemory.SourceGenerator;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// Emits one table class: the secondary indexes (each a copy of the rows sorted by that key), the Find / TryFind /
/// FindClosest / FindRange methods of every key, the uniqueness validation and the metadata of the table.
/// </summary>
static class TableEmitter
{
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
                EmitFind(writer, element, key, throwIfKeyNotFound);

                var arguments = $"{key.TableName}, {key.SelectorName}, {key.BuildComparer()}";

                writer.Line();
                using (writer.Block($"public {key.BuildReturnTypeNameForClosest(element)} FindClosestBy{key.BuildMethodName()}({key.BuildTypeName()} key, bool selectLower = true)"))
                {
                    writer.Line($"return {key.BuildFindPrefix()}ClosestCore({arguments}, key, selectLower);");
                }

                writer.Line();
                using (writer.Block($"public RangeView<{element}> FindRangeBy{key.BuildMethodName()}({key.BuildTypeName()} min, {key.BuildTypeName()} max, bool ascendant = true)"))
                {
                    writer.Line($"return {key.BuildFindPrefix()}RangeCore({arguments}, min, max, ascendant);");
                }
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

    static void EmitFind(CodeWriter writer, string element, KeyBase key, bool throwIfKeyNotFound)
    {
        var returnType = key.BuildReturnTypeName(element);
        var methodName = key.BuildMethodName();
        var keyType = key.BuildTypeName();

        if (key.CanInlineBinarySearch)
        {
            // a unique numeric primary key: the lookup is written out against the member itself, instead of going through
            // a selector delegate and a comparer
            var nullable = throwIfKeyNotFound ? "" : "?";
            var notFound = throwIfKeyNotFound ? "return ThrowKeyNotFound(key);" : "return default;";
            var member = key.Properties[0].Name;

            var unsigned = UnsignedDistanceType(key);

            // The search itself is TableBase's, SpanHelpers.BinarySearch of the BCL in shape: it is given a struct that
            // compares the key to a row, which compiles the comparison into the loop. For a key of 32 bits or less the
            // struct also answers "at or after this row?" as a mask, by arithmetic alone (the difference is exact in a
            // long); that is what lets the search halve its range without a conditional jump on small tables. A 64-bit
            // key has no wider type to subtract in, and is searched with jumps at every size.
            var branchless = unsigned == "uint";
            var comparable = branchless ? "IBranchlessComparable" : "IComparable";
            var search = branchless
                ? "(data.Length <= BranchlessSearchMaxLength) ? FindIndexByBranchlessSearch(data, comparable) : FindIndexByBinarySearch(data, comparable)"
                : "FindIndexByBinarySearch(data, comparable)";

            // the direct hit of a gapless table first, then the search
            writer.Lines($$"""
                [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                public {{returnType}}{{nullable}} FindBy{{methodName}}({{keyType}} key)
                {
                    var offset = unchecked(({{unsigned}})(key - primaryIndexFirstKey));
                    if (offset < ({{unsigned}})primaryIndexDenseLength)
                    {
                        var row = data[(int)offset];
                        if (row.{{member}} == key) { return row; }
                    }

                    var index = FindIndexBy{{methodName}}(key);
                    if (index >= 0) { return data[index]; }
                    {{notFound}}
                }

                [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                public bool TryFindBy{{methodName}}({{keyType}} key, out {{returnType}} result)
                {
                    var offset = unchecked(({{unsigned}})(key - primaryIndexFirstKey));
                    if (offset < ({{unsigned}})primaryIndexDenseLength)
                    {
                        var row = data[(int)offset];
                        if (row.{{member}} == key) { result = row; return true; }
                    }

                    var index = FindIndexBy{{methodName}}(key);
                    if (index >= 0) { result = data[index]; return true; }
                    result = default!;
                    return false;
                }

                // the index of the row with the key, negative when there is none
                [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                int FindIndexBy{{methodName}}({{keyType}} key)
                {
                    var comparable = new PrimaryIndexComparable(key);
                    return {{search}};
                }

                // the primary key of a lookup, as the search wants it: negative, zero or positive as the key is before, at or after a row
                readonly struct PrimaryIndexComparable : {{comparable}}<{{element}}>
                {
                    readonly {{keyType}} key;

                    public PrimaryIndexComparable({{keyType}} key)
                    {
                        this.key = key;
                    }

                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                    public int CompareTo({{element}} other)
                    {
                        return (key < other.{{member}}) ? -1 : (key > other.{{member}}) ? 1 : 0;
                    }
                """);
            if (branchless)
            {
                writer.Indent();
                writer.Line();
                writer.Lines($$"""
                    // (other - key - 1) is negative exactly when other <= key; its sign, spread over the word, is the mask
                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                    public int AtOrAfterMask({{element}} other)
                    {
                        return unchecked((int)(((long)other.{{member}} - key - 1) >> 63));
                    }
                    """);
                writer.Unindent();
            }
            writer.Line("}");
            return;
        }

        var arguments = $"{key.TableName}, {key.SelectorName}, {key.BuildComparer()}";

        if (key.IsNonUnique)
        {
            // several rows per key: a range, empty when there is none, so nothing to throw and no TryFind
            using (writer.Block($"public {returnType} FindBy{methodName}({keyType} key)"))
            {
                writer.Line($"return {key.BuildFindPrefix()}Core({arguments}, key);");
            }
            return;
        }

        var core = key.BuildFindPrefix() + "Core" + (key.IsIntType ? "Int" : "");

        using (writer.Block($"public {returnType}{(throwIfKeyNotFound ? "" : "?")} FindBy{methodName}({keyType} key)"))
        {
            writer.Line($"return {core}({arguments}, key, {(throwIfKeyNotFound ? "true" : "false")});");
        }
        writer.Line();
        using (writer.Block($"public bool TryFindBy{methodName}({keyType} key, out {returnType} result)"))
        {
            writer.Line($"return Try{core}({arguments}, key, out result);");
        }
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
