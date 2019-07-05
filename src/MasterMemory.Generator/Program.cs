using MasterMemory.GeneratorCore;
using MicroBatchFramework;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MasterMemory.Generator
{
    public class Program : BatchBase
    {
        static async Task Main(string[] args)
        {
            await BatchHost.CreateDefaultBuilder().RunBatchEngineAsync<Program>(args);
        }

        public void Execute()
        {
            new CodeAnalyzer(@"C:\GitHubRepositories\MasterMemory\src\MasterMemory2\");
        }
    }



    public class CodeAnalyzer
    {
        public CodeAnalyzer(string directoryPath)
        {
            foreach (var item in Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories))
            {
                var syntax = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(File.ReadAllText(item));

            }



            // syntax.GetRoot().
        }
    }


    public class GenerationContext
    {
        public string ClassFullName { get; set; }
        public string ClassName { get; set; }
        public PrimaryKey PrimaryKey { get; set; }
        public SecondaryKey SecondaryKeys { get; set; }

    }

    public class PrimaryKey
    {
        public bool IsNonUnique { get; set; }
        public KeyProperty[] Properties { get; set; }
    }

    public class SecondaryKey
    {
        public bool IsNonUnique { get; set; }
        public int IndexNo { get; set; }
        public KeyProperty[] Properties { get; set; }
    }

    public class KeyProperty
    {
        public int KeyOrder { get; set; }
        public string Name { get; set; }
        public string TypeFullName { get; set; }
    }


}
