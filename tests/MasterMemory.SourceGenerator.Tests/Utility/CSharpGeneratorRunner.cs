using MasterMemory.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

public static class CSharpGeneratorRunner
{
    static Compilation baseCompilation = default!;

    [ModuleInitializer]
    public static void InitializeCompilation()
    {
        // Add project namespace.
        var globalUsings = """
global using System;
global using System.Threading;
global using System.Threading.Tasks;
global using System.ComponentModel.DataAnnotations;
global using MasterMemory;
""";

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Select(x => MetadataReference.CreateFromFile(x.Location))
            .Concat(GetSelfReferences());

        var compilation = CSharpCompilation.Create("generatortest",
            references: references,
            syntaxTrees: [CSharpSyntaxTree.ParseText(globalUsings, path: "GlobalUsings.cs")],
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication, allowUnsafe: true)); // .exe

        baseCompilation = compilation;

        static IEnumerable<MetadataReference> GetSelfReferences()
        {
            // MasterMemory.Annotations
            yield return MetadataReference.CreateFromFile(typeof(MasterMemory.MemoryTableAttribute).Assembly.Location);
        }
    }

    public static (Compilation, ImmutableArray<Diagnostic>) RunGenerator([StringSyntax("C#-test")] string source, string[]? preprocessorSymbols = null, AnalyzerConfigOptionsProvider? options = null)
    {
        if (preprocessorSymbols == null)
        {
            preprocessorSymbols = new[] { "NET8_0_OR_GREATER" };
        }
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp12, preprocessorSymbols: preprocessorSymbols); // 12

        // Create SourceGenerator
        var driver = CSharpGeneratorDriver.Create(new MasterMemoryGenerator()).WithUpdatedParseOptions(parseOptions);
        if (options != null)
        {
            driver = (Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver)driver.WithUpdatedAnalyzerConfigOptions(options);
        }

        var compilation = baseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, parseOptions));

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);
        return (newCompilation, diagnostics);
    }

    public static (Compilation, ImmutableArray<Diagnostic>, string) CompileAndExecute(string source, string[] args, string[]? preprocessorSymbols = null, AnalyzerConfigOptionsProvider? options = null)
    {
        var (compilation, diagnostics) = RunGenerator(source, preprocessorSymbols, options);

        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);
        if (!emitResult.Success)
        {
            throw new InvalidOperationException("Emit Failed\r\n" + string.Join("\r\n", emitResult.Diagnostics.Select(x => x.ToString())));
        }

        ms.Position = 0;

        // capture stdout log
        // modify global stdout so can't run in parallel unit-test
        var originalOut = Console.Out;
        try
        {
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // load and invoke Main(args)
            var loadContext = new AssemblyLoadContext("source-generator", isCollectible: true); // isCollectible to support Unload
            var assembly = loadContext.LoadFromStream(ms);
            assembly.EntryPoint!.Invoke(null, new object[] { args });
            loadContext.Unload();

            return (compilation, diagnostics, stringWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    public static (string Key, string Reasons)[][] GetIncrementalGeneratorTrackedStepsReasons(string keyPrefixFilter, params string[] sources)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp12); // 12
        var driver = CSharpGeneratorDriver.Create(
            [new MasterMemoryGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true))
            .WithUpdatedParseOptions(parseOptions);

        var generatorResults = sources
            .Select(source =>
            {
                var compilation = baseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, parseOptions));
                driver = driver.RunGenerators(compilation);
                return driver.GetRunResult().Results[0];
            })
            .ToArray();

        var reasons = generatorResults
            .Select(x => x.TrackedSteps
                .Where(x => x.Key.StartsWith(keyPrefixFilter) || x.Key == "SourceOutput")
                .Select(x =>
                {
                    if (x.Key == "SourceOutput")
                    {
                        var values = x.Value.Where(x => x.Inputs[0].Source.Name?.StartsWith(keyPrefixFilter) ?? false);
                        return (
                            x.Key,
                            Reasons: string.Join(", ", values.SelectMany(x => x.Outputs).Select(x => x.Reason).ToArray())
                        );
                    }
                    else
                    {
                        return (
                            Key: x.Key.Substring(keyPrefixFilter.Length),
                            Reasons: string.Join(", ", x.Value.SelectMany(x => x.Outputs).Select(x => x.Reason).ToArray())
                        );
                    }
                })
                .OrderBy(x => x.Key)
                .ToArray())
            .ToArray();

        return reasons;
    }
}
