using MasterMemory.GeneratorCore;
using Microsoft.CodeAnalysis;
using System;

namespace MasterMemory.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class MasterMemoryGenerator : IIncrementalGenerator
{
    // TODO: property
    public string? UsingNamespace { get; set; }
    public string? InputDirectory { get; set; }
    public string? OutputDirectory { get; set; }
    public string? PrefixClassName { get; set; }
    public bool AddImmutableConstructor { get; set; }
    public bool ReturnNullIfKeyNotFound { get; set; } = true;
    public bool ForceOverwrite { get; set; }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // TODO:...

        new CodeGenerator().GenerateFile(UsingNamespace, InputDirectory, OutputDirectory, PrefixClassName, AddImmutableConstructor, !ReturnNullIfKeyNotFound, ForceOverwrite, x => Console.WriteLine(x));

    }
}
