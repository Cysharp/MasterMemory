using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

public class VerifyHelper(ITestOutputHelper output, string idPrefix)
{
    // Diagnostics Verify

    public void Ok([StringSyntax("C#-test")] string code, [CallerArgumentExpression("code")] string? codeExpr = null)
    {
        output.WriteLine(codeExpr);

        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator(code);
        foreach (var item in diagnostics)
        {
            output.WriteLine(item.ToString());
        }
        OutputGeneratedCode(compilation);

        diagnostics.Length.Should().Be(0);
    }

    public void Verify(int id, [StringSyntax("C#-test")] string code, string diagnosticsCodeSpan, [CallerArgumentExpression("code")] string? codeExpr = null)
    {
        output.WriteLine(codeExpr);

        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator(code);
        foreach (var item in diagnostics)
        {
            output.WriteLine(item.ToString());
        }
        OutputGeneratedCode(compilation);

        diagnostics.Length.Should().Be(1);
        diagnostics[0].Id.Should().Be(idPrefix + id.ToString("000"));

        var text = GetLocationText(diagnostics[0], compilation.SyntaxTrees);
        text.Should().Be(diagnosticsCodeSpan);
    }

    public (string, string)[] Verify([StringSyntax("C#-test")] string code, [CallerArgumentExpression("code")] string? codeExpr = null)
    {
        output.WriteLine(codeExpr);

        var (compilation, diagnostics) = CSharpGeneratorRunner.RunGenerator(code);
        OutputGeneratedCode(compilation);
        return diagnostics.Select(x => (x.Id, GetLocationText(x, compilation.SyntaxTrees))).ToArray();
    }

    // Execute and check stdout result

    public void Execute([StringSyntax("C#-test")] string code, string args, string expected, [CallerArgumentExpression("code")] string? codeExpr = null)
    {
        output.WriteLine(codeExpr);

        var (compilation, diagnostics, stdout) = CSharpGeneratorRunner.CompileAndExecute(code, args == "" ? [] : args.Split(' '));
        foreach (var item in diagnostics)
        {
            output.WriteLine(item.ToString());
        }
        OutputGeneratedCode(compilation);

        stdout.Should().Be(expected);
    }

    public string Error([StringSyntax("C#-test")] string code, string args, [CallerArgumentExpression("code")] string? codeExpr = null)
    {
        output.WriteLine(codeExpr);

        var (compilation, diagnostics, stdout) = CSharpGeneratorRunner.CompileAndExecute(code, args == "" ? [] : args.Split(' '));
        foreach (var item in diagnostics)
        {
            output.WriteLine(item.ToString());
        }
        OutputGeneratedCode(compilation);

        return stdout;
    }

    string GetLocationText(Diagnostic diagnostic, IEnumerable<SyntaxTree> syntaxTrees)
    {
        var location = diagnostic.Location;

        var textSpan = location.SourceSpan;
        var sourceTree = location.SourceTree;
        if (sourceTree == null)
        {
            var lineSpan = location.GetLineSpan();
            if (lineSpan.Path == null) return "";

            sourceTree = syntaxTrees.FirstOrDefault(x => x.FilePath == lineSpan.Path);
            if (sourceTree == null) return "";
        }

        var text = sourceTree.GetText().GetSubText(textSpan).ToString();
        return text;
    }

    void OutputGeneratedCode(Compilation compilation)
    {
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            // only shows ConsoleApp.Run/Builder generated code
            if (!syntaxTree.FilePath.Contains("g.cs")) continue;
            output.WriteLine(syntaxTree.ToString());
        }
    }
}