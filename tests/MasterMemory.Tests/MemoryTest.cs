using System.Collections.Generic;
using System.Linq;
using Xunit;
using ZeroFormatter;

namespace MasterMemory.Tests
{
    [ZeroFormattable]
    public class Sample
    {
        [Index(0)]
        public virtual int Id { get; set; }
        [Index(1)]
        public virtual int Age { get; set; }
        [Index(2)]
        public virtual string FirstName { get; set; }
        [Index(3)]
        public virtual string LastName { get; set; }

        public override string ToString()
        {
            return $"{Id} {Age} {FirstName} {LastName}";
        }
    }

    public class MemoryTest
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

        Memory<int, Sample> CreateMemory(Sample[] data)
        {
            return new Memory<int, Sample>(data, x => x.Id);
        }

        [Fact]
        public void Count()
        {
            var data = CreateData();
            var memory = CreateMemory(data);

            memory.Count.Is(data.Length);
        }

        [Fact]
        public void Find()
        {
            var data = CreateData();
            var memory = CreateMemory(data);

            foreach (var item in data)
            {
                var f = memory.Find(item.Id);
                item.Id.Is(f.Id);
            }

            Assert.Throws<KeyNotFoundException>(() => memory.Find(110));
            memory.FindOrDefault(110).IsNull();

            var view = memory.ToDictionaryView();
            foreach (var item in data)
            {
                var f = view[item.Id];
                item.Id.Is(f.Id);
            }
        }

        [Fact]
        public void MultiKeyFind()
        {
            var data = CreateData();
            var memory = CreateMemory(data);
            var secondary = memory.SecondaryIndex("FirstName.LastName", x => KeyTuple.Create(x.FirstName, x.LastName));

            foreach (var item in data)
            {
                var f = secondary.Find(KeyTuple.Create(item.FirstName, item.LastName));
                item.Id.Is(f.Id);
            }

            Assert.Throws<KeyNotFoundException>(() => secondary.Find(KeyTuple.Create("aaa", "___")));
            Assert.Throws<KeyNotFoundException>(() => secondary.Find(KeyTuple.Create("___", "foo")));

            secondary.FindOrDefault(KeyTuple.Create("aaa", "___")).IsNull();
            secondary.FindOrDefault(KeyTuple.Create("___", "foo")).IsNull();
        }

        [Fact]
        public void FindClosest()
        {
            var data = CreateData();
            var memory = CreateMemory(data);

            var secondary = memory.SecondaryIndex("Age", x => x.Age);
            {
                secondary.FindClosest(56, true).Age.Is(49);
                secondary.FindClosest(56, false).Age.Is(59);
            }
            {
                // first
                for (int i = 0; i < 9; i++)
                {
                    secondary.FindClosest(i, selectLower: true).Age.Is(9);
                }

                var lastAge = 9;
                foreach (var item in data.OrderBy(x => x.Age))
                {
                    for (int i = lastAge + 1; i < item.Age; i++)
                    {
                        secondary.FindClosest(i, selectLower: true).Age.Is(lastAge);
                    }

                    lastAge = item.Age;
                }

                // last
                for (int i = 99; i < 120; i++)
                {
                    secondary.FindClosest(i, selectLower: true).Age.Is(99);
                }
            }
            {
                // first
                for (int i = 0; i < 9; i++)
                {
                    secondary.FindClosest(i, selectLower: false).Age.Is(9);
                }

                var xss = data.OrderBy(x => x.Age).ToArray();
                for (int j = 1; j < xss.Length - 1; j++)
                {
                    var item = xss[j];
                    for (int i = xss[j - 1].Age + 1; i < item.Age; i++)
                    {
                        secondary.FindClosest(i, selectLower: false).Age.Is(xss[j].Age);
                    }
                }

                // last
                for (int i = 99; i < 120; i++)
                {
                    secondary.FindClosest(i, selectLower: false).Age.Is(99);
                }
            }
        }

        [Fact]
        public void FindClosestMultiKey()
        {
            var data = CreateData();
            var memory = CreateMemory(data);

            // Age of aaa
            //new Sample { Id = 5, Age = 19, FirstName = "aaa", LastName = "foo" },
            //new Sample { Id = 2, Age = 89, FirstName = "aaa", LastName = "bar" },
            //new Sample { Id = 4, Age = 89, FirstName = "aaa", LastName = "tako" },
            //new Sample { Id = 9, Age = 99, FirstName = "aaa", LastName = "ika" },

            var secondary = memory.SecondaryIndex("FirstName.Age", x => KeyTuple.Create(x.FirstName, x.Age));

            secondary.FindClosest(KeyTuple.Create("aaa", 10), true).Age.Is(19);
            secondary.FindClosest(KeyTuple.Create("aaa", 92), true).Age.Is(89);
            secondary.FindClosest(KeyTuple.Create("aaa", 120), true).Age.Is(99);
            secondary.FindClosest(KeyTuple.Create("aaa", 10), false).Age.Is(19);
            secondary.FindClosest(KeyTuple.Create("aaa", 73), false).Age.Is(89);
            secondary.FindClosest(KeyTuple.Create("aaa", 120), false).Age.Is(99);
        }

        [Fact]
        public void FindMany()
        {
            var data = CreateData();
            var memory = CreateMemory(data);

            var secondary = memory.SecondaryIndex("FirstName", x => x.FirstName);

            secondary.FindMany("aaa").OrderBy(x => x.Id).Select(x => x.Id).Is(2, 4, 5, 9);

            var view = secondary.ToLookupView();
            view["aaa"].OrderBy(x => x.Id).Select(x => x.Id).Is(2, 4, 5, 9);
        }

        [Fact]
        public void FindManyMultiKey()
        {
            var data = CreateData();
            var memory = CreateMemory(data);

            var secondary = memory.SecondaryIndex("FirstName.Age", x => KeyTuple.Create(x.FirstName, x.Age));

            secondary.FindMany(KeyTuple.Create("aaa", 89)).Select(x => x.Id).Is(2, 4);
            secondary.FindMany(KeyTuple.Create("aaa", 89), false).Select(x => x.Id).Is(4, 2);
        }

        [Fact]
        public void Empty()
        {
            var memory = new Memory<int, int>(Enumerable.Empty<int>(), x => x);
            memory.FindAll().Count.Is(0);
        }
    }
}