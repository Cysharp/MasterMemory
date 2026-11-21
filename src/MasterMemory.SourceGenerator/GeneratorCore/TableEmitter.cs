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
            // a unique numeric primary key: the search is written out, comparing with < and > instead of going through
            // a selector delegate and a comparer
            var nullable = throwIfKeyNotFound ? "" : "?";
            var notFound = throwIfKeyNotFound ? "return ThrowKeyNotFound(key);" : "return default;";
            var member = key.Properties[0].Name;

            writer.Lines($$"""
                [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                public {{returnType}}{{nullable}} FindBy{{methodName}}({{keyType}} key)
                {
                    var lo = 0;
                    var hi = data.Length - 1;
                    while (lo <= hi)
                    {
                        var mid = (int)(((uint)hi + (uint)lo) >> 1);
                        var selected = data[mid].{{member}};
                        var found = (selected < key) ? -1 : (selected > key) ? 1 : 0;
                        if (found == 0) { return data[mid]; }
                        if (found < 0) { lo = mid + 1; }
                        else { hi = mid - 1; }
                    }
                    {{notFound}}
                }

                [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                public bool TryFindBy{{methodName}}({{keyType}} key, out {{returnType}} result)
                {
                    var lo = 0;
                    var hi = data.Length - 1;
                    while (lo <= hi)
                    {
                        var mid = (int)(((uint)hi + (uint)lo) >> 1);
                        var selected = data[mid].{{member}};
                        var found = (selected < key) ? -1 : (selected > key) ? 1 : 0;
                        if (found == 0) { result = data[mid]; return true; }
                        if (found < 0) { lo = mid + 1; }
                        else { hi = mid - 1; }
                    }
                    result = default!;
                    return false;
                }
                """);
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
