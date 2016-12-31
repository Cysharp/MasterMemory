using System;
using MasterMemory;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ZeroFormatter;

namespace MasterMemory.Tests
{
    public class Sample
    {
        public int Id { get; set; }
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public override string ToString()
        {
            return $"{Id} {Age} {FirstName} {LastName}";
        }
    }

    public class MemoryTest
    {
        [Fact]
        public void Test()
        {
            // Id = Unique, PK
            // FirstName + LastName = Unique
            var data = new[]
            {
                new Sample { Id = 5, Age = 19, FirstName = "aaa", LastName = "foo" },
                new Sample { Id = 6, Age = 29, FirstName = "bbb", LastName = "foo" },
                new Sample { Id = 7, Age = 39, FirstName = "ccc", LastName = "foo" },
                new Sample { Id = 8, Age = 19, FirstName = "ddd", LastName = "foo" },
                new Sample { Id = 1, Age = 29, FirstName = "eee", LastName = "foo" },
                new Sample { Id = 2, Age = 39, FirstName = "aaa", LastName = "bar" },
                new Sample { Id = 3, Age = 19, FirstName = "bbb", LastName = "bar" },
                new Sample { Id = 4, Age = 29, FirstName = "ccc", LastName = "bar" },
                new Sample { Id = 9, Age = 39, FirstName = "ddd", LastName = "baz" },
                new Sample { Id = 10, Age = 9, FirstName = "eee", LastName = "baz" },
            };

            var memory = new Memory<Sample, int>("test", data, x => x.Id);


            memory.Find(3).Age.Is(19);

            //var huga = memory.FindRange(5, 8);

            var secondary1 = memory.DynamicIndex("FirstName", x => x.FirstName);

            // secondary1.Find("FIrstName);

            var secondary = memory.DynamicIndex("FirstName.LastName", x => KeyTuple.Create(x.FirstName, x.LastName));

            var foobar = secondary.FindRange(KeyTuple.Create("bbb", "foo"));
            // foobar.Id.Is(6);


            //memory.FindRange(4, 7



        }
    }
}