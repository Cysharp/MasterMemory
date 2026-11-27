using MasterMemory.Tests.TestStructures;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xunit;

namespace MasterMemory.Tests
{
    public class NullableStringKeyTest
    {
        // Codes that a culture-sensitive comparison and an ordinal one put in different orders: by culture "_" sorts
        // before digits and case is a tie-break, by ordinal it is 'B' < 'a' < "a1" < "a_b".
        static NullableStringKeyMaster[] CreateData() => new[]
        {
            new NullableStringKeyMaster { Code = "a_b", Alias = "Alpha", Value = 1 },
            new NullableStringKeyMaster { Code = "a1", Alias = "beta", Value = 2 },
            new NullableStringKeyMaster { Code = "B", Alias = "GAMMA", Value = 3 },
            new NullableStringKeyMaster { Code = "a", Alias = "delta", Value = 4 },
        };

        [Fact]
        public void PrimaryKeyIsOrdinal()
        {
            var table = new MemoryDatabase(new DatabaseBuilder().Append(CreateData()).Build()).NullableStringKeyMasterTable;

            table.All.AsReadOnlyList().Select(x => x.Code).ToArray().ShouldBe(new[] { "B", "a", "a1", "a_b" });
            foreach (var item in CreateData())
            {
                table.FindByCode(item.Code).Value.ShouldBe(item.Value);
            }
            table.TryFindByCode("A", out _).ShouldBeFalse();
        }

        [Fact]
        public void StringComparisonOptionApplies()
        {
            var table = new MemoryDatabase(new DatabaseBuilder().Append(CreateData()).Build()).NullableStringKeyMasterTable;

            table.FindByAlias("ALPHA").Value.ShouldBe(1);
            table.FindByAlias("gamma").Value.ShouldBe(3);
        }

        // the order no longer depends on where the database is built or loaded
        [Fact]
        public void BuiltAndLoadedUnderDifferentCultures()
        {
            var original = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                var bin = new DatabaseBuilder().Append(CreateData()).Build();

                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                var table = new MemoryDatabase(bin).NullableStringKeyMasterTable;

                foreach (var item in CreateData())
                {
                    table.FindByCode(item.Code).Value.ShouldBe(item.Value);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }
    }
}
