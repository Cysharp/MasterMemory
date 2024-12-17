using Microsoft.CodeAnalysis;
using System;

namespace MasterMemory.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class MasterMemoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // TODO:...
    }
}
