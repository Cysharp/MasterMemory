using RuntimeUnitTestToolkit;
using System.Linq;

namespace MasterMemory.Tests
{
    public class MemoryKeyMemoryTest
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

        
        public void Unique()
        {
            var memory = CreateMemory(CreateData());

            var byIdAndAgeAndFirstNameAndLastName = memory.SecondaryIndex("AllIndex", x => MemoryKey.Create(x.Id, x.Age, x.FirstName, x.LastName));


            var byId = byIdAndAgeAndFirstNameAndLastName.UseIndex1();
            var byIdAndAge = byIdAndAgeAndFirstNameAndLastName.UseIndex12();
            var byIdAndAgeAndFirstName = byIdAndAgeAndFirstNameAndLastName.UseIndex123();


            byId.Find(8).Id.Is(8);
            byId.FindOrDefault(100).IsNull();

            byIdAndAge.Find(4, 89).Id.Is(4);
            byIdAndAge.FindOrDefault(4, 899).IsNull();
            byIdAndAge.FindOrDefault(5, 89).IsNull();

            byIdAndAgeAndFirstName.Find(6, 29, "bbb").Id.Is(6);
            byIdAndAgeAndFirstName.FindOrDefault(6, 29, "bbbz").IsNull();
        }

        
        public void Range()
        {
            var memory = CreateMemory(CreateData());

            var byFirstNameAndLastNameAndAge = memory.SecondaryIndex("FirstName.LastName.Age", x => MemoryKey.Create(x.FirstName, x.LastName, x.Age));

            var byFirstName = byFirstNameAndLastNameAndAge.UseIndex1();
            var byFirstNameAndLastName = byFirstNameAndLastNameAndAge.UseIndex12();

            byFirstName.FindMany("eee").Select(x => x.Id).OrderBy(x => x).IsCollection(1, 10);
            byFirstName.FindMany("eeee").Count.Is(0);

            byFirstNameAndLastName.FindMany("aaa", "foo").Select(x => x.Id).OrderBy(x => x).IsCollection(5);

            byFirstNameAndLastName.FindClosest("aaa", "takz").Id.Is(4);

        }
    }
}
