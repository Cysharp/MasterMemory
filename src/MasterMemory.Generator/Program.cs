using ConsoleAppFramework;
using MasterMemory.GeneratorCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MasterMemory.Generator
{
    public class Program : ConsoleAppBase
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder()
                .ConfigureLogging(x => x.ReplaceToSimpleConsole())
                .RunConsoleAppFrameworkAsync<Program>(args);
        }

        public void Execute(
            [Option("i", "Input file directory(search recursive).")]string inputDirectory,
            [Option("o", "Output file directory.")]string outputDirectory,
            [Option("n", "Namespace of generated files.")]string usingNamespace,
            [Option("p", "Prefix of class names.")]string prefixClassName = "",
            [Option("c", "Add immutable constructor to MemoryTable class.")]bool addImmutableConstructor = false,
            [Option("t", "Return null if key not found on unique find method.")]bool returnNullIfKeyNotFound = false,
            [Option("f", "Overwrite generated files if the content is unchanged.")]bool forceOverwrite = false)
        {
            var sw = Stopwatch.StartNew();
            Console.WriteLine("Start MasterMemory CodeGeneration");

            new CodeGenerator().GenerateFile(usingNamespace, inputDirectory, outputDirectory, prefixClassName, addImmutableConstructor, !returnNullIfKeyNotFound, forceOverwrite, x => Console.WriteLine(x));

            Console.WriteLine("Complete MasterMemory CodeGeneration, elapsed:" + sw.Elapsed);
        }
    }
}