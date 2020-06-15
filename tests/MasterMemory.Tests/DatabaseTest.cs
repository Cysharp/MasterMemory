using FluentAssertions;
using MessagePack;
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
        public DatabaseTest()
        {
            MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions.WithResolver(MessagePackResolver.Instance);
        }

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
        public void SingleDb()
        {
            var builder = new DatabaseBuilder();
            builder.Append(CreateData());

            var bin = builder.Build();
            var db = new MemoryDatabase(bin);
            db.SampleTable.FindById(8).Age.Should().Be(49);

            var tableInfo = MemoryDatabase.GetTableInfo(bin);
            tableInfo[0].TableName.Should().Be("s_a_m_p_l_e");
        }

        [Fact]
        public void All()
        {
            var builder = new DatabaseBuilder();
            builder.Append(CreateData());

            var bin = builder.Build();
            var db = new MemoryDatabase(bin);

            db.SampleTable.All.Select(x => x.Id).ToArray().Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            db.SampleTable.AllReverse.Select(x => x.Id).ToArray().Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.Reverse());
            db.SampleTable.SortByAge.Select(x => x.Id).OrderBy(x => x).ToArray().Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

         [Fact]
        public void Ranges()
        {
            var builder = new DatabaseBuilder();
            builder.Append(CreateData());

            var bin = builder.Build();
            var db = new MemoryDatabase(bin);

            db.SampleTable.FindRangeByAge(2,2).Select(x=>x.Id).ToArray().Should().BeEquivalentTo( new int[] {} );     
            db.SampleTable.FindRangeByAge(30,50).Select(x=>x.Id).ToArray().Should().BeEquivalentTo( new int[] { 7, 8 } );     
            db.SampleTable.FindRangeByAge(100,100).Select(x=>x.Id).ToArray().Should().BeEquivalentTo( new int[] {} );     
        }


        [Fact]
        public void EmptyAll()
        {
            {
                var builder = new DatabaseBuilder();
                builder.Append(new Sample[] { });

                var bin = builder.Build();
                var db = new MemoryDatabase(bin);

                db.SampleTable.All.Any().Should().BeFalse();
            }
            {
                var builder = new DatabaseBuilder();
                builder.Append(new Sample[] { }.Select(x => x));

                var bin = builder.Build();
                var db = new MemoryDatabase(bin);

                db.SampleTable.All.Any().Should().BeFalse();
            }
        }

        [Fact]
        public void WithNull()
        {
            var builder = new DatabaseBuilder();
            builder.Append(new Sample[] {new Sample
            {
                Age = 10,
                FirstName = null,
                Id = 999,
                LastName = "abcde"
            } });

            var bin = builder.Build();
            var db = new MemoryDatabase(bin);

            var sample = db.SampleTable.FindById(999);
            sample.Age.Should().Be(10);
            sample.FirstName.Should().BeNull();
            sample.LastName.Should().Be("abcde");
        }
    }
}
