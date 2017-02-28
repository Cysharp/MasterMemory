using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MasterMemory.Tests
{
    public class DatabaseTest
    {
        Sample[] CreateData()
        {
            // Id = Unique, PK
            // FirstName + LastName = Unique
            var data = new[]
            {
                new Sample { Id = 5, Age = 19, FirstName = "aaa", LastName = "foo" },
                new Sample { Id = 6, Age = 29, FirstName = "bbb", LastName = "foo" },
                new Sample { Id = 7, Age = 39, FirstName = "ccc", LastName = "foo" },
                new Sample { Id = 8, Age = 49, FirstName = "ddd", LastName = "foo" },
                new Sample { Id = 1, Age = 59, FirstName = "eee", LastName = "foo" },
                new Sample { Id = 2, Age = 89, FirstName = "aaa", LastName = "bar" },
                new Sample { Id = 3, Age = 79, FirstName = "be", LastName = "de" },
                new Sample { Id = 4, Age = 89, FirstName = "aaa", LastName = "tako" },
                new Sample { Id = 9, Age = 99, FirstName = "aaa", LastName = "ika" },
                new Sample { Id = 10, Age = 9, FirstName = "eee", LastName = "baz" },
            };
            return data;
        }

        [Fact]
        public void EmptyDb()
        {
            var builder = new DatabaseBuilder();
            var emptyDb = builder.Build();

            emptyDb.MemoryCount.Is(0);
            var data = emptyDb.Save();
            data.Is((byte)144); // empty array
        }

        [Fact]
        public void SingleDb()
        {
            var builder = new DatabaseBuilder();
            builder.Add("Sample", CreateData(), x => x.Id);
            var db = builder.Build();

            var memory = db.GetMemory<int, Sample>("Sample", x => x.Id);
            memory.Find(8).Age.Is(49);

            var savedDb = db.Save();

            var newDb = Database.Open(savedDb);
            var memory2 = newDb.GetMemory<int, Sample>("Sample", x => x.Id);
            memory2.Find(8).Age.Is(49);
        }

        [Fact]
        public void MultiDb()
        {
            var builder = new DatabaseBuilder();
            builder.Add("Sample1", CreateData(), x => x.Id);
            builder.Add("Sample2", CreateData(), x => x.Id);
            builder.Add("Sample3", CreateData(), x => x.Id);
            builder.Add("Sample4", CreateData(), x => x.Id);
            builder.Add("Sample5", CreateData(), x => x.Id);
            var db = builder.Build();

            var memory = db.GetMemory<int, Sample>("Sample1", x => x.Id);
            memory.Find(8).Age.Is(49);

            var savedDb = db.Save();

            var newDb = Database.Open(savedDb);
            {
                var memory2 = newDb.GetMemory<int, Sample>("Sample1", x => x.Id);
                memory2.Find(8).Age.Is(49);
            }
            {
                var memory2 = newDb.GetMemory<int, Sample>("Sample2", x => x.Id);
                memory2.Find(8).Age.Is(49);
            }
            {
                var memory2 = newDb.GetMemory<int, Sample>("Sample3", x => x.Id);
                memory2.Find(8).Age.Is(49);
            }
            {
                var memory2 = newDb.GetMemory<int, Sample>("Sample4", x => x.Id);
                memory2.Find(8).Age.Is(49);
            }
            {
                var memory2 = newDb.GetMemory<int, Sample>("Sample5", x => x.Id);
                memory2.Find(8).Age.Is(49);
            }

            var dumper = Database.ReportDiagnostics(savedDb, true);

        }
    }
}
