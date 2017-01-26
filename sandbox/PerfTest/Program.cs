using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using System.IO;

// based test code -> https://github.com/mbdavid/LiteDB-Perf

namespace TestPerfLiteDB
{
    class Program
    {
        static void Test()
        {
            var dbPath = @"C:\Users\y.kawai\AppData\Local\Temp\Grani,Inc\Ivory\masterdatabase.db";
            var hugahuga = MasterMemory.Database.ReportDiagnostics(File.ReadAllBytes(dbPath))
                .OrderByDescending(x => x.Item2)
                .Select(x=> new { DbName = x.Item1, Size = x.Item2 / 1024 / 1024 + "MB", OriginalSize = x.Item2 })
                .ToArray();

            var db = MasterMemory.Database.Open(File.ReadAllBytes(dbPath), true);
        }

        static void Main(string[] args)
        {
            Test();
            return;
            /*
            RunTest("LiteDB: default", new LiteDB_Test(5000, null, new FileOptions { Journal = true, FileMode = FileMode.Shared }));
            //RunTest("LiteDB: encrypted", new LiteDB_Test(5000, "mypass", new FileOptions { Journal = true, FileMode = FileMode.Shared }));
            //RunTest("LiteDB: exclusive no journal", new LiteDB_Test(5000, null, new FileOptions { Journal = false, FileMode = FileMode.Exclusive }));
            RunTest("LiteDB: in-memory", new LiteDB_Test(5000));

            RunTest("SQLite: default", new SQLite_Test(5000, null, true));
            //RunTest("SQLite: encrypted", new SQLite_Test(5000, "mypass", true));
            //RunTest("SQLite: no journal", new SQLite_Test(5000, null, false));
            RunTest("SQLite: in-memory", new SQLite_Test(5000, null, false, true));

            // RunTest("RavenDB: in-memory", new RavenDB_Test(5000, true));

            RunTest("Dictionary", new Dictionary_Test(5000));
            RunTest("ConcurrentDictionary", new ConcurrentDictionary_Test(5000));
            RunTest("ImmutableDictionary", new ImmutableDictionary_Test(5000));

            RunTest("MasterMemory: Plain", new MasterMemory_Test(5000));
            RunTest("MasterMemory: Loaded", new MasterMemoryDatabase_Test(5000));

            Console.ReadKey();
            */
        }

        static void RunTest(string name, ITest test)
        {
            var title = name + " - " + test.Count + " records";
            Console.WriteLine(title);
            Console.WriteLine("=".PadLeft(title.Length, '='));

            test.Prepare();

            test.Run("Insert", test.Insert, true);
            test.Run("Bulk", test.Bulk, true);
            test.Run("CreateIndex", test.CreateIndex, true);
            test.Run("Query", test.Query, false);
            test.Run("Query", test.Query, false);
            test.Run("Query", test.Query, false);
            test.Run("Query", test.Query, false);

            try
            {
                Console.WriteLine("FileLength     : " + Math.Round((double)test.FileLength / (double)1024, 2).ToString().PadLeft(5, ' ') + " kb");
            }
            catch (System.IO.FileNotFoundException)
            {
            }

            test.Dispose();

            Console.WriteLine();

        }
    }
}