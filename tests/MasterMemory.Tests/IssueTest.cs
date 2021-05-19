using FluentAssertions;
using MasterMemory.Tests.TestStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MasterMemory.Tests
{
    public class IssueTest
    {
        //[Fact]
        //public void Issue49()
        //{
        //    var builder = new DatabaseBuilder().Append(new[]
        //    {
        //        new PersonModel { FirstName = "realname", LastName="reallast" },
        //        new PersonModel { FirstName = "fakefirst", LastName="fakelast" },
        //    });

        //    var data = builder.Build();
        //    var database = new MemoryDatabase(data);

        //    var entries = database.PersonModelTable.FindClosestByFirstNameAndLastName(("real", "real"), false);
        //    var firstEntry = entries.FirstOrDefault();

        //    var firstIs = firstEntry.FirstName;

        //}

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
        public void Issue57()
        {
            var builder = new DatabaseBuilder();
            builder.Append(CreateData());

            var bin = builder.Build();
            var db = new MemoryDatabase(bin);

            db.SampleTable.FindRangeByAge(2, 2).Select(x => x.Id).ToArray().Should().BeEquivalentTo(new int[] { });
            db.SampleTable.FindRangeByAge(30, 50).Select(x => x.Id).ToArray().Should().BeEquivalentTo(new int[] { 7, 8 });
            db.SampleTable.FindRangeByAge(100, 100).Select(x => x.Id).ToArray().Should().BeEquivalentTo(new int[] { });
        }

    }
}
