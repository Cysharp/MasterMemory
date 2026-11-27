using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MasterMemory;

/// <summary>
/// A diagnostic as the incremental pipeline carries it: compared by value, and holding no syntax tree. It becomes a
/// <see cref="Diagnostic"/> only where it is reported.
/// </summary>
internal sealed record DiagnosticInfo(DiagnosticDescriptor Descriptor, LocationInfo? Location, EquatableArray<string> MessageArguments)
{
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, LocationInfo? location, params string[] messageArguments)
    {
        return new DiagnosticInfo(descriptor, location, new EquatableArray<string>(messageArguments));
    }

    public Diagnostic ToDiagnostic()
    {
        return Diagnostic.Create(Descriptor, Location?.ToLocation() ?? Microsoft.CodeAnalysis.Location.None, MessageArguments.ToArray());
    }
}

/// <summary>Where a diagnostic points, without the syntax tree a <see cref="Location"/> holds on to.</summary>
internal readonly record struct LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public Location ToLocation()
    {
        return Location.Create(FilePath, TextSpan, LineSpan);
    }

    public static LocationInfo? From(Location? location)
    {
        if (location?.SourceTree == null)
        {
            return null;
        }
        return new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }
}

internal static class DiagnosticDescriptors
{
    const string Category = "GenerateMasterMemory";

    public static DiagnosticDescriptor Create(int id, string message)
    {
        return Create(id, message, message);
    }

    public static DiagnosticDescriptor Create(int id, string title, string messageFormat)
    {
        return new DiagnosticDescriptor(
            id: "MAM" + id.ToString("000"),
            title: title,
            messageFormat: messageFormat,
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }

    public static DiagnosticDescriptor RequirePrimaryKey { get; } = Create(
        1,
        "MemoryTable does not found PrimaryKey property, Type:{0}.");

    // MAM002 (a second [PrimaryKey] on one property) is retired: the attribute does not allow it, so the compiler says so.

    public static DiagnosticDescriptor DuplicateSecondaryKey { get; } = Create(
        3,
        "Duplicate SecondaryKey, doesn't allow to add multiple attribute in same attribute list:{0}.{1}");

    public static DiagnosticDescriptor DuplicateClassName { get; } = Create(
        4,
        "MemoryTable class name must be unique, generated code is named after it: {0}Table. Rename one of: {1}.");

    public static DiagnosticDescriptor DuplicateTableName { get; } = Create(
        5,
        "MemoryTable name must be unique, it is the name of the table in the database binary: \"{0}\" is the name of {1}.");

    public static DiagnosticDescriptor DuplicateKeyOrder { get; } = Create(
        6,
        "Properties of a key that are declared in different files of a partial class need different keyOrder, the order they are compared in is otherwise not defined: {0} of {1} has {2} at keyOrder {3}.");

    public static DiagnosticDescriptor NonUniqueWithoutKey { get; } = Create(
        7,
        "[NonUnique] applies to the [PrimaryKey] or [SecondaryKey] in the same attribute list, and the list it is in has none: {0}.{1}. Write them in one list, as in [SecondaryKey(0), NonUnique].");

    public static DiagnosticDescriptor StringComparisonOptionOnNonStringKey { get; } = Create(
        8,
        "[StringComparisonOption] is for a key of type string: {0}.{1} is {2}.");

    public static DiagnosticDescriptor GenericTable { get; } = Create(
        9,
        "MemoryTable cannot be a generic type, or nested in one: {0}.");

    public static DiagnosticDescriptor InaccessibleTable { get; } = Create(
        10,
        "MemoryTable must be public or internal, and so must every type it is nested in, generated code refers to it from another class: {0}.");

    public static DiagnosticDescriptor InvalidKeyProperty { get; } = Create(
        11,
        "A key must be an instance property with a getter that is public or internal, generated code reads it from another class: {0}.{1}.");

    // a warning: the project builds as before, with code generated that the symbol used to take out
    public static DiagnosticDescriptor ObsoleteSymbol { get; } = new DiagnosticDescriptor(
        id: "MAM012",
        title: "The symbol is no longer read by MasterMemory",
        messageFormat: "The symbol {0} is no longer read by MasterMemory and the code it took out is generated. Set {1} = false in [assembly: MasterMemoryGeneratorOptions] instead, and remove the symbol.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
