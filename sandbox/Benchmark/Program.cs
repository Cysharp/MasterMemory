using System;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    class Program
    {
        // dotnet run -c Release -- --filter *                 every benchmark, measured until the results are stable
        // dotnet run -c Release -- --filter * --job short     a quick look: three iterations each
        // dotnet run -c Release -- --filter *Lookup*          one class
        // dotnet run -c Release -- size                       the size of the database each engine stores
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "size")
            {
                SizeReport.Run();
                return;
            }

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }

    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            // No job is named here, so a run uses BenchmarkDotNet's default job: it warms up and iterates until the
            // results are stable and reports their error. "--job short" on the command line replaces it.
            AddExporter(MarkdownExporter.GitHub);
            AddExporter(CsvExporter.Default);
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }

    /// <summary>The directory the databases of one benchmark are written to.</summary>
    static class TempDirectory
    {
        public static string Create()
        {
            return Directory.CreateTempSubdirectory("mastermemory_benchmark_").FullName;
        }

        public static void Delete(string path)
        {
            if (path == null) return;
            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                // a file an engine has not let go of yet: it stays in the temp directory
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public static long SizeOf(string path)
        {
            if (File.Exists(path)) return new FileInfo(path).Length;

            var size = 0L;
            foreach (var file in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
            {
                size += file.Length;
            }
            return size;
        }
    }
}
