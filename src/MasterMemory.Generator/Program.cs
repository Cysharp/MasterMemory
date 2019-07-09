using MasterMemory.GeneratorCore;
using MicroBatchFramework;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MasterMemory.Generator
{
    public class Program : BatchBase
    {
        static async Task Main(string[] args)
        {
            await BatchHost.CreateDefaultBuilder().RunBatchEngineAsync<Program>(args);
        }

        public void Execute(
            [Option("i", "Input file directory(search recursive).")]string inputDirectory,
            [Option("o", "Output file directory.")]string outputDirectory,
            [Option("n", "Namespace of generated files.")]string usingNamespace)
        {
            var sw = Stopwatch.StartNew();
            Console.WriteLine("Start MasterMemory CodeGeneration");

            new CodeGenerator().GenerateFile(usingNamespace, inputDirectory, outputDirectory, x => Console.WriteLine(x));

            Console.WriteLine("Complete MasterMemory CodeGeneration, elapsed:" + sw.Elapsed);
        }
    }
}