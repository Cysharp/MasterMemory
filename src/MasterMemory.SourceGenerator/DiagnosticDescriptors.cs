using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace MasterMemory;

internal sealed class DiagnosticReporter : IEquatable<DiagnosticReporter>
{
    List<Diagnostic>? diagnostics;

    public bool HasDiagnostics => diagnostics != null && diagnostics.Count != 0;

    public void ReportDiagnostic(DiagnosticDescriptor diagnosticDescriptor, Location location, params object?[]? messageArgs)
    {
        var diagnostic = Diagnostic.Create(diagnosticDescriptor, location, messageArgs);
        if (diagnostics == null)
        {
            diagnostics = new();
        }
        diagnostics.Add(diagnostic);
    }

    public void ReportToContext(SourceProductionContext context)
    {
        if (diagnostics != null)
        {
            foreach (var item in diagnostics)
            {
                context.ReportDiagnostic(item);
            }
        }
    }

    public bool Equals(DiagnosticReporter other)
    {
        // if error, always false and otherwise ignore
        if (diagnostics == null && other.diagnostics == null)
        {
            return true;
        }

        return false;
    }
}

internal static class DiagnosticDescriptors
{
    const string Category = "GenerateMasterMemory";

    public static void ReportDiagnostic(this SourceProductionContext context, DiagnosticDescriptor diagnosticDescriptor, Location location, params object?[]? messageArgs)
    {
        var diagnostic = Diagnostic.Create(diagnosticDescriptor, location, messageArgs);
        context.ReportDiagnostic(diagnostic);
    }

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

    public static DiagnosticDescriptor DuplicatePrimaryKey { get; } = Create(
        2,
        "Duplicate PrimaryKey:{0}.{1}");

    public static DiagnosticDescriptor DuplicateSecondaryKey { get; } = Create(
        3,
        "Duplicate SecondaryKey, doesn't allow to add multiple attribute in same attribute list:{0}.{1}");
}
