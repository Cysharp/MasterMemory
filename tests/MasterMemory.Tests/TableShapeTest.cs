using MasterMemory.Tests.Fuzz.Target;
using MasterMemory.Tests.Tables;
using MasterMemory.Tests.TestStructures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xunit;

namespace MasterMemory.Tests
{
    // The tables of TestStructures/ShapeMasters.cs, through the whole of it: generated, compiled, built into a database
    // binary, loaded and searched. What the generator writes for each is in the generator's own tests; that the
    // result compiles and answers right is here.
    public class TableShapeTest
    {
        static MemoryDatabase Load(Action<DatabaseBuilder> append)
        {
            var builder = new DatabaseBuilder();
            append(builder);
            return new MemoryDatabase(builder.Build());
        }

        [Fact]
        public void PositionalRecord()
        {
            var table = Load(x => x.Append(new[]
            {
                new RecordMaster(3, "b", 30),
                new RecordMaster(1, "a", 10),
                new RecordMaster(2, "b", 20),
            })).RecordMasterTable;

            table.All.Select(x => x.Id).ToArray().ShouldBe(new[] { 1, 2, 3 });
            table.FindById(2).ShouldBe(new RecordMaster(2, "b", 20));
            table.TryFindById(4, out _).ShouldBeFalse();
            table.FindByGroup("b").Select(x => x.Level).ToArray().ShouldBe(new[] { 20, 30 });

            var meta = RecordMasterTable.CreateMetaTable();
            meta.Properties.Select(x => x.Name).ToArray().ShouldBe(new[] { "Id", "Group", "Level" });
        }

        [Fact]
        public void KeysOfABaseClass()
        {
            var table = Load(x => x.Append(new[]
            {
                new DerivedMaster { Id = 20, Tenant = 2, Name = "b" },
                new DerivedMaster { Id = 10, Tenant = 1, Name = "a" },
                new DerivedMaster { Id = 30, Tenant = 2, Name = "c" },
            })).DerivedMasterTable;

            table.FindById(20).Name.ShouldBe("b");
            table.FindByTenant(2).Select(x => x.Name).ToArray().ShouldBe(new[] { "b", "c" });
            table.FindClosestById(25, selectLower: true)!.Name.ShouldBe("b");

            var meta = DerivedMasterTable.CreateMetaTable();
            meta.Properties.Select(x => x.Name).ToArray().ShouldBe(new[] { "Id", "Tenant", "Name" });
        }

        [Fact]
        public void KeysOfAnotherPart()
        {
            var table = Load(x => x.Append(new[]
            {
                new PartialMaster { Category = 2, No = 1, Name = "c" },
                new PartialMaster { Category = 1, No = 2, Name = "b" },
                new PartialMaster { Category = 1, No = 1, Name = "a" },
            })).PartialMasterTable;

            table.All.Select(x => x.Name).ToArray().ShouldBe(new[] { "a", "b", "c" });
            table.FindByCategoryAndNo(1, 2).Name.ShouldBe("b");
            table.TryFindByCategoryAndNo(2, 2, out _).ShouldBeFalse();
        }

        [Fact]
        public void InternalTable()
        {
            var db = Load(x => x.Append(new[]
            {
                new InternalMaster { Id = 2, Group = "x" },
                new InternalMaster { Id = 1, Group = "x" },
            }));

            db.InternalMasterTable.FindById(1).Group.ShouldBe("x");
            db.InternalMasterTable.FindByGroup("x").Count.ShouldBe(2);

            // only what has the internal type in its signature is internal
            typeof(InternalMasterTable).IsPublic.ShouldBeFalse();
            typeof(MemoryDatabase).IsPublic.ShouldBeTrue();
            typeof(MemoryDatabase).GetProperty("InternalMasterTable").ShouldBeNull();
            typeof(MemoryDatabase).GetProperty("SampleTable").ShouldNotBeNull();
            typeof(DatabaseBuilder).GetMethods().Count(x => x.Name == "Append" && x.GetParameters()[0].ParameterType == typeof(IEnumerable<InternalMaster>)).ShouldBe(0);

            // and it is changed through the builder like any other table
            var builder = db.ToDatabaseBuilder();
            builder.AddOrReplace(new[] { new InternalMaster { Id = 3, Group = "y" } });
            builder.ToMemoryDatabase().InternalMasterTable.Count.ShouldBe(3);
        }

        [Fact]
        public void SpelledTheLongWay()
        {
            var table = Load(x => x.Append(new[]
            {
                new SpelledMaster { Id = 3, @class = -5, Name = "Gamma", Letter = 'z' },
                new SpelledMaster { Id = 1, @class = 7, Name = "alpha", Letter = 'a' },
                new SpelledMaster { Id = 2, @class = 300, Name = "BETA", Letter = 'M' },
            })).SpelledMasterTable;

            table.FindById(2).Name.ShouldBe("BETA");                      // an alias of int: [PrimaryKeyAttribute] MyId
            table.FindByclass(-5).Id.ShouldBe(3);                         // short, a keyword as the name, the index from a constant
            table.SortByclass.Select(x => (int)x.@class).ToArray().ShouldBe(new[] { -5, 7, 300 });
            table.FindByName("beta").Id.ShouldBe(2);                      // the option in front of its key
            table.FindByName("ALPHA").Id.ShouldBe(1);
            table.FindByLetter('M').Id.ShouldBe(2);                       // char
            table.SortByLetter.Select(x => x.Letter).ToArray().ShouldBe(new[] { 'M', 'a', 'z' });
            table.FindClosestByLetter('b', selectLower: true)!.Id.ShouldBe(1);
            table.FindRangeByclass(0, 1000).Select(x => x.Id).ToArray().ShouldBe(new[] { 1, 2 });
        }

        // ---- composite keys with strings ----

        // Codes that a comparison by culture and an ordinal one put in different orders: by culture "_" sorts before
        // digits and case is a tie-break, by ordinal it is "B" < "a" < "a1" < "a_b".
        static CompositeNameMaster[] CreateData() => new[]
        {
            new CompositeNameMaster { Code = "a_b", No = 1, Label = "Alpha", Rank = 2, Value = 1 },
            new CompositeNameMaster { Code = "a1", No = 2, Label = "beta", Rank = 1, Value = 2 },
            new CompositeNameMaster { Code = "B", No = 1, Label = "ALPHA", Rank = 2, Value = 3 },
            new CompositeNameMaster { Code = "a", No = 9, Label = "alpha", Rank = 1, Value = 4 },
            new CompositeNameMaster { Code = "a", No = 3, Label = "BETA", Rank = 1, Value = 5 },
            new CompositeNameMaster { Code = "a1", No = 1, Label = "gamma", Rank = 5, Value = 6 },
        };

        [Fact]
        public void CompositeKeyIsOrdinal()
        {
            var table = Load(x => x.Append(CreateData())).CompositeNameMasterTable;

            table.All.Select(x => (x.Code, x.No)).ToArray().ShouldBe(new[] { ("B", 1), ("a", 3), ("a", 9), ("a1", 1), ("a1", 2), ("a_b", 1) });
            table.All.Select(x => (x.Code, x.No)).ToArray().ShouldBe(CreateData().Select(x => (x.Code, x.No)).OrderBy(x => x, OrdinalTupleComparer.Create<string, int>()).ToArray());

            foreach (var item in CreateData())
            {
                table.FindByCodeAndNo(item.Code, item.No).Value.ShouldBe(item.Value);
            }
            table.TryFindByCodeAndNo("A", 3, out _).ShouldBeFalse();
            table.TryFindByCodeAndNo("b", 1, out _).ShouldBeFalse();

            table.FindClosestByCodeAndNo("a", 5, selectLower: true)!.Value.ShouldBe(5);
            table.FindClosestByCodeAndNo("a", 5, selectLower: false)!.Value.ShouldBe(4);
            table.FindClosestByCodeAndNo("C", 0, selectLower: true)!.Value.ShouldBe(3);   // "C" is between "B" and "a"
            table.FindRangeByCodeAndNo("a", 0, "a1", 1).Select(x => x.Value).ToArray().ShouldBe(new[] { 5, 4, 6 });
        }

        // [StringComparisonOption] belongs to the member it is written on, and was ignored on a composite key
        [Fact]
        public void CompositeKeyMemberWithAnOption()
        {
            var table = Load(x => x.Append(CreateData())).CompositeNameMasterTable;

            table.FindByLabelAndRank("ALPHA", 2).Select(x => x.Value).OrderBy(x => x).ToArray().ShouldBe(new[] { 1, 3 });
            table.FindByLabelAndRank("alpha", 1).Select(x => x.Value).ToArray().ShouldBe(new[] { 4 });
            table.FindByLabelAndRank("Beta", 1).Select(x => x.Value).OrderBy(x => x).ToArray().ShouldBe(new[] { 2, 5 });
            table.FindByLabelAndRank("beta", 2).Count.ShouldBe(0);
            table.SortByLabelAndRank.Select(x => x.Label.ToLowerInvariant() + x.Rank).ToArray().ShouldBe(new[] { "alpha1", "alpha2", "alpha2", "beta1", "beta1", "gamma5" });
        }

        // the order does not depend on where the database is built or loaded
        [Theory]
        [InlineData("en-US", "tr-TR")]
        [InlineData("tr-TR", "en-US")]
        [InlineData("sv-SE", "ja-JP")]
        public void CompositeKeyBuiltAndLoadedUnderDifferentCultures(string buildCulture, string loadCulture)
        {
            var original = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(buildCulture);
                var bin = new DatabaseBuilder().Append(CreateData()).Build();

                Thread.CurrentThread.CurrentCulture = new CultureInfo(loadCulture);
                var table = new MemoryDatabase(bin).CompositeNameMasterTable;

                table.All.Select(x => x.Code).ToArray().ShouldBe(new[] { "B", "a", "a", "a1", "a1", "a_b" });
                foreach (var item in CreateData())
                {
                    table.FindByCodeAndNo(item.Code, item.No).Value.ShouldBe(item.Value);
                    table.FindByLabelAndRank(item.Label, item.Rank).Select(x => x.Value).ShouldContain(item.Value);
                }

                // every writer of the table sorts by the same order: a change to a table that is there too
                var builder = new MemoryDatabase(bin).ToDatabaseBuilder();
                builder.AddOrReplace(new[] { new CompositeNameMaster { Code = "A", No = 1, Label = "x", Rank = 0, Value = 7 } });
                var rebuilt = builder.ToMemoryDatabase().CompositeNameMasterTable;
                rebuilt.All.Select(x => x.Code).ToArray().ShouldBe(new[] { "A", "B", "a", "a", "a1", "a1", "a_b" });
                rebuilt.FindByCodeAndNo("A", 1).Value.ShouldBe(7);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        // MetaIndex.Comparer is the comparer the index is sorted with
        [Fact]
        public void CompositeKeyMetaComparer()
        {
            var meta = CompositeNameMasterTable.CreateMetaTable();

            var primary = meta.Indexes[0].Comparer;
            primary.Compare(("B", 1), ("a", 1)).ShouldBeLessThan(0);
            primary.Compare(("a", 9), ("a1", 1)).ShouldBeLessThan(0);
            primary.Compare(("a", 3), ("a", 3)).ShouldBe(0);

            var secondary = meta.Indexes[1].Comparer;
            secondary.Compare(("ALPHA", 2), ("alpha", 2)).ShouldBe(0);
            secondary.Compare(("alpha", 2), ("BETA", 1)).ShouldBeLessThan(0);
        }
    }
}
