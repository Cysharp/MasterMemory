using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MasterMemory.GeneratorCore;

/// <summary>
/// What reading one [MemoryTable] class gives: the table, or what is wrong with it. Location is the class name, for the
/// diagnostics that need two tables to be decided (the same class name, the same table name).
/// </summary>
internal sealed record TableParseResult(GenerationContext? Table, LocationInfo? Location, EquatableArray<DiagnosticInfo> Diagnostics);

/// <summary>
/// Reads a [MemoryTable] class into a <see cref="GenerationContext"/>.
/// </summary>
/// <remarks>
/// Everything is read from symbols, not from the text of the declaration. So an attribute is the same one however it
/// is written (PrimaryKey, PrimaryKeyAttribute, MasterMemory.PrimaryKey, through an alias), its arguments are the
/// values the compiler computed (a constant, a named argument), a key is found wherever the property is declared
/// (another part of a partial class, a base class, the parameter list of a record), and a type is what it is rather
/// than how it is spelled (an alias of int is an int). Code that does not compile yet, as it is while being typed,
/// reads as "this attribute is not there": the compiler reports the error, and the other tables stay generated.
/// </remarks>
internal static class TableParser
{
    // Generated code names every type in full (global::Namespace.Type), so it does not depend on the using directives
    // of the file a table is declared in: those can sit inside #if, and a file-scoped namespace has none to copy.
    static readonly SymbolDisplayFormat FullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    // System.StringComparison, by value
    static readonly string[] StringComparisonNames = ["CurrentCulture", "CurrentCultureIgnoreCase", "InvariantCulture", "InvariantCultureIgnoreCase", "Ordinal", "OrdinalIgnoreCase"];

    public static TableParseResult Parse(GeneratorAttributeSyntaxContext source, CancellationToken cancellationToken)
    {
        var type = (INamedTypeSymbol)source.TargetSymbol;
        var location = LocationInfo.From((source.TargetNode as BaseTypeDeclarationSyntax)?.Identifier.GetLocation() ?? type.Locations.FirstOrDefault());

        if (type.TypeKind != TypeKind.Class || source.Attributes.Length == 0)
        {
            // [MemoryTable] is for classes (a record is one); on anything else the compiler says so
            return new TableParseResult(null, location, new EquatableArray<DiagnosticInfo>([]));
        }

        var diagnostics = new List<DiagnosticInfo>();
        var table = ParseTable(type, source.Attributes[0], location, diagnostics, cancellationToken);
        return new TableParseResult((diagnostics.Count == 0) ? table : null, location, new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
    }

    static GenerationContext? ParseTable(INamedTypeSymbol type, AttributeData memoryTableAttribute, LocationInfo? location, List<DiagnosticInfo> diagnostics, CancellationToken cancellationToken)
    {
        var className = type.Name;
        var fullTypeName = type.ToDisplayString(FullyQualifiedFormat);

        for (var t = type; t != null; t = t.ContainingType)
        {
            if (t.IsGenericType)
            {
                diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.GenericTable, location, fullTypeName));
                return null;
            }
        }

        var accessibility = GetAccessibility(type);
        if (accessibility == null)
        {
            diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InaccessibleTable, location, fullTypeName));
            return null;
        }

        var tableName = (TryGetConstant<string>(memoryTableAttribute, 0, out var name) && name != null) ? name : className;

        var primaryKey = new KeyDraft("PrimaryKey");
        var secondaryKeys = new List<(int IndexNo, KeyDraft Key)>(); // in the order they are first seen
        var columns = new List<Property>();

        foreach (var property in GetProperties(type))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var propertyLocation = LocationInfo.From(property.Locations.FirstOrDefault()) ?? location;
            var isIgnored = false;
            var hasKey = false;

            // [StringComparisonOption] says how the property is compared, in every key it is a member of. It is one
            // per property, so it does not matter which attribute list it is in: before the key, after it, or in a
            // list of its own (which is how the README has always shown it, and where it used to be dropped).
            var hasStringComparison = false;
            string? stringComparison = null;
            foreach (var attribute in property.GetAttributes())
            {
                if (GetAttributeName(attribute) == "MasterMemory.StringComparisonOptionAttribute"
                    && TryGetConstant<int>(attribute, 0, out var comparison) && (uint)comparison < (uint)StringComparisonNames.Length)
                {
                    hasStringComparison = true;
                    stringComparison = StringComparisonNames[comparison];
                }
            }
            var isString = property.Type.SpecialType == SpecialType.System_String;

            // For [NonUnique] an attribute list is the unit: [SecondaryKey(0), NonUnique][SecondaryKey(1)] makes the
            // property a member of a shared key 0 and of a unique key 1.
            foreach (var list in GroupByAttributeList(property, cancellationToken))
            {
                KeyAttribute? primary = null;
                var secondaries = new List<KeyAttribute>();
                var isNonUnique = false;

                foreach (var attribute in list)
                {
                    switch (GetAttributeName(attribute))
                    {
                        case "MasterMemory.PrimaryKeyAttribute":
                            // keyOrder is optional; an argument that does not compile reads as 0
                            primary = new KeyAttribute(0, TryGetConstant<int>(attribute, 0, out var primaryKeyOrder) ? primaryKeyOrder : 0);
                            break;
                        case "MasterMemory.SecondaryKeyAttribute":
                            if (TryGetConstant<int>(attribute, 0, out var indexNo)) // without it, it is still being typed
                            {
                                secondaries.Add(new KeyAttribute(indexNo, TryGetConstant<int>(attribute, 1, out var secondaryKeyOrder) ? secondaryKeyOrder : 0));
                            }
                            break;
                        case "MasterMemory.NonUniqueAttribute":
                            isNonUnique = true;
                            break;

                        case "MessagePack.IgnoreMemberAttribute":
                        case "System.Runtime.Serialization.IgnoreDataMemberAttribute":
                            isIgnored = true;
                            break;
                        default:
                            break;
                    }
                }

                if (secondaries.Count > 1)
                {
                    diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.DuplicateSecondaryKey, propertyLocation, className, property.Name));
                    continue;
                }

                if (primary == null && secondaries.Count == 0)
                {
                    if (isNonUnique)
                    {
                        // before, it was dropped without a word and the key stayed unique
                        diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.NonUniqueWithoutKey, propertyLocation, className, property.Name));
                    }
                    continue;
                }

                if (!hasKey)
                {
                    hasKey = true;
                    if (!IsReadableKey(property))
                    {
                        diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.InvalidKeyProperty, propertyLocation, className, property.Name));
                    }
                }

                if (primary != null)
                {
                    primaryKey.Add(CreateKeyProperty(property, primary.Value.KeyOrder, isString ? stringComparison : null), property, isNonUnique);
                }
                foreach (var secondary in secondaries)
                {
                    var index = secondaryKeys.FindIndex(x => x.IndexNo == secondary.IndexNo);
                    if (index == -1)
                    {
                        secondaryKeys.Add((secondary.IndexNo, new KeyDraft($"SecondaryKey({secondary.IndexNo})")));
                        index = secondaryKeys.Count - 1;
                    }
                    secondaryKeys[index].Key.Add(CreateKeyProperty(property, secondary.KeyOrder, isString ? stringComparison : null), property, isNonUnique);
                }
            }

            if (hasStringComparison && !(hasKey && isString))
            {
                var reason = !hasKey ? "not a key" : "of type " + property.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.StringComparisonOptionOnNonStringKey, propertyLocation, className, property.Name, reason));
            }

            if (!isIgnored && IsColumn(property))
            {
                columns.Add(new Property { Name = property.Name });
            }
        }

        if (primaryKey.Properties.Count == 0)
        {
            diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.RequirePrimaryKey, location, className));
            return null;
        }

        primaryKey.ReportUndefinedOrder(className, location, diagnostics);
        foreach (var (_, key) in secondaryKeys)
        {
            key.ReportUndefinedOrder(className, location, diagnostics);
        }

        return new GenerationContext
        {
            ClassName = className,
            FullTypeName = fullTypeName,
            Accessibility = accessibility,
            MemoryTableName = tableName,
            MemoryTableNameLiteral = SymbolDisplay.FormatLiteral(tableName, quote: true),
            PrimaryKey = new PrimaryKey { IsNonUnique = primaryKey.IsNonUnique, Properties = primaryKey.SortedProperties() },
            SecondaryKeys = secondaryKeys.Select(x => new SecondaryKey { IndexNo = x.IndexNo, IsNonUnique = x.Key.IsNonUnique, Properties = x.Key.SortedProperties() }).ToArray(),
            Properties = columns.ToArray(),
        };
    }

    readonly record struct KeyAttribute(int IndexNo, int KeyOrder);

    // a key while its properties are being collected
    sealed class KeyDraft(string displayName)
    {
        public List<KeyProperty> Properties { get; } = new List<KeyProperty>();
        public bool IsNonUnique { get; private set; }
        readonly List<IPropertySymbol> symbols = new List<IPropertySymbol>();

        public void Add(KeyProperty property, IPropertySymbol symbol, bool isNonUnique)
        {
            Properties.Add(property);
            symbols.Add(symbol);
            if (isNonUnique) IsNonUnique = true; // one member saying so is enough, as it always was
        }

        // By keyOrder, and members at one keyOrder in the order they are declared in (base classes first): keyOrder is
        // often left out, as in [PrimaryKey] on two properties, and the key is then (first, second).
        public EquatableArray<KeyProperty> SortedProperties()
        {
            return Properties.OrderBy(x => x.KeyOrder).ToArray(); // a stable sort
        }

        // The one case in which "the order they are declared in" does not exist: members at one keyOrder in different
        // files of a partial class, which the compiler is free to read in any order.
        public void ReportUndefinedOrder(string className, LocationInfo? location, List<DiagnosticInfo> diagnostics)
        {
            foreach (var group in Enumerable.Range(0, Properties.Count).GroupBy(i => Properties[i].KeyOrder).Where(x => x.Count() > 1))
            {
                var undefined = group
                    .GroupBy(i => symbols[i].ContainingType, SymbolEqualityComparer.Default)
                    .Any(sameType => sameType.Select(i => symbols[i].Locations.FirstOrDefault()?.SourceTree).Distinct().Count() > 1);
                if (undefined)
                {
                    diagnostics.Add(DiagnosticInfo.Create(DiagnosticDescriptors.DuplicateKeyOrder, location, displayName, className, string.Join(", ", group.Select(i => Properties[i].Name)), group.Key.ToString()));
                }
            }
        }
    }

    static KeyProperty CreateKeyProperty(IPropertySymbol property, int keyOrder, string? stringComparison)
    {
        var type = property.Type;
        var kind = type.SpecialType switch
        {
            SpecialType.System_String => KeyTypeKind.String,
            SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_UInt16
                or SpecialType.System_Char or SpecialType.System_Int32 or SpecialType.System_UInt32 => KeyTypeKind.Integer32,
            SpecialType.System_Int64 or SpecialType.System_UInt64 => KeyTypeKind.Integer64,
            _ => KeyTypeKind.Other,
        };

        // an enum, or Nullable<an enum>
        var isNullableEnum = false;
        var enumType = type as INamedTypeSymbol;
        if (enumType != null && enumType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && enumType.TypeArguments.Length == 1)
        {
            enumType = enumType.TypeArguments[0] as INamedTypeSymbol;
            isNullableEnum = enumType?.TypeKind == TypeKind.Enum;
        }
        var enumUnderlyingType = (enumType?.TypeKind == TypeKind.Enum) ? enumType.EnumUnderlyingType?.ToDisplayString(FullyQualifiedFormat) : null;

        return new KeyProperty
        {
            KeyOrder = keyOrder,
            Name = property.Name,
            Identifier = EscapeIdentifier(property.Name),
            FullTypeName = type.ToDisplayString(FullyQualifiedFormat),
            Kind = kind,
            EnumUnderlyingType = enumUnderlyingType,
            IsNullableEnum = isNullableEnum && enumUnderlyingType != null,
            StringComparison = stringComparison,
        };
    }

    // Every property of the type, base classes first. A property that overrides or hides one of a base class takes its
    // place, unless it carries no attribute of its own: then the attributes of the base declaration are what counts.
    static List<IPropertySymbol> GetProperties(INamedTypeSymbol type)
    {
        var chain = new List<INamedTypeSymbol>();
        for (var t = type; t != null && t.SpecialType != SpecialType.System_Object; t = t.BaseType)
        {
            chain.Add(t);
        }

        var result = new List<IPropertySymbol>();
        for (var i = chain.Count - 1; i >= 0; i--)
        {
            foreach (var member in chain[i].GetMembers())
            {
                if (member is not IPropertySymbol property || property.IsIndexer)
                {
                    continue;
                }

                var index = result.FindIndex(x => x.Name == property.Name);
                if (index == -1)
                {
                    result.Add(property);
                }
                else if (property.GetAttributes().Length != 0)
                {
                    result[index] = property;
                }
            }
        }
        return result;
    }

    // The attributes of a property, list by list. One that has no syntax (a base class from another assembly) cannot
    // be told apart by list, and they are taken together.
    static List<List<AttributeData>> GroupByAttributeList(IPropertySymbol property, CancellationToken cancellationToken)
    {
        var groups = new List<(SyntaxNode? List, List<AttributeData> Attributes)>();
        foreach (var attribute in property.GetAttributes())
        {
            var list = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).Parent;
            var index = groups.FindIndex(x => ReferenceEquals(x.List, list));
            if (index == -1)
            {
                groups.Add((list, new List<AttributeData>()));
                index = groups.Count - 1;
            }
            groups[index].Attributes.Add(attribute);
        }
        return groups.Select(x => x.Attributes).ToList();
    }

    static string? GetAttributeName(AttributeData attribute)
    {
        var type = attribute.AttributeClass;
        if (type == null || type.ContainingType != null)
        {
            return null;
        }
        var @namespace = type.ContainingNamespace;
        return (@namespace == null || @namespace.IsGlobalNamespace) ? type.Name : @namespace.ToDisplayString() + "." + type.Name;
    }

    // a constructor argument as the compiler computed it; false when it is missing or does not compile
    static bool TryGetConstant<T>(AttributeData attribute, int index, out T? value)
    {
        var arguments = attribute.ConstructorArguments;
        if (!arguments.IsDefault && index < arguments.Length)
        {
            var argument = arguments[index];
            if (argument.Kind != TypedConstantKind.Array && argument.Kind != TypedConstantKind.Error && argument.Value is T typed)
            {
                value = typed;
                return true;
            }
        }
        value = default;
        return false;
    }

    // "public" or "internal" as generated code in the same assembly can use the type; null when it cannot at all
    static string? GetAccessibility(INamedTypeSymbol type)
    {
        var result = "public";
        for (var t = type; t != null; t = t.ContainingType)
        {
            switch (t.DeclaredAccessibility)
            {
                case Accessibility.Public:
                    break;
                case Accessibility.Internal:
                case Accessibility.ProtectedOrInternal:
                    result = "internal";
                    break;
                default:
                    return null;
            }
        }
        return result;
    }

    static bool IsAccessible(Accessibility accessibility)
    {
        return accessibility == Accessibility.Public || accessibility == Accessibility.Internal || accessibility == Accessibility.ProtectedOrInternal;
    }

    static bool IsReadableKey(IPropertySymbol property)
    {
        return !property.IsStatic && property.GetMethod != null && IsAccessible(property.DeclaredAccessibility) && IsAccessible(property.GetMethod.DeclaredAccessibility);
    }

    // what the metadata lists: the public instance properties
    static bool IsColumn(IPropertySymbol property)
    {
        return !property.IsStatic && property.GetMethod != null && property.DeclaredAccessibility == Accessibility.Public;
    }

    static string EscapeIdentifier(string name)
    {
        return (SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None) ? "@" + name : name;
    }
}
