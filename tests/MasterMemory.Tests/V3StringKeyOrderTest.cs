using MasterMemory.Tests.Fuzz.Target;
using MasterMemory.Tests.Tables;
using MasterMemory.Tests.TestStructures;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    // A table is stored in the order of its primary key and searched in that order. Where the key has a string member,
    // MasterMemory v3 could order it differently from v4. Read as it is, such a table from a database v3 built is
    // searched by an order it is not in, and lookups miss rows that are there, with no error at all: measured on a
    // table of 16 rows before this was handled, 8 of them were found.
    // The generated constructor of such a table therefore checks the order of the rows it is given, and puts them in
    // order when they are not.
    //
    // Golden/v3-string-keys.bin was written by MasterMemory 3.0.4 under the culture en-US, from the rows below. The
    // order v3 stored them in, as v3 itself reads them back:
    //   v3_composite  a/1 a/2 a_b/1 a_b/2 a-b/1 a-b/2 a1/1 a1/2 ab/1 ab/2 B/1 B/2 C/1 C/2 d/1 d/2   (by culture)
    //   v3_nullable   a a_b a-b a1 ab B C d                                                        (by culture)
    //   v3_option     B C a a-b a1 a_b ab d                                                        (ordinal: the option was not applied)
    // Do not regenerate it with the current code: the point is what v3 wrote.
    public class V3StringKeyOrderTest
    {
        static readonly string[] Codes = { "a_b", "a1", "B", "a", "C", "d", "a-b", "ab" };

        static MemoryDatabase ReadV3() => new MemoryDatabase(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Golden", "v3-string-keys.bin")));

        [Fact]
        public void CompositeKeyWithAString()
        {
            var table = ReadV3().V3CompositeMasterTable;

            table.Count.ShouldBe(16);
            table.All.Select(x => (x.Code, x.No)).ToArray().ShouldBe(table.All.Select(x => (x.Code, x.No)).OrderBy(x => x, OrdinalTupleComparer.Create<string, int>()).ToArray());
            table.All.Select(x => x.Code).Distinct().ToArray().ShouldBe(new[] { "B", "C", "a", "a-b", "a1", "a_b", "ab", "d" });

            for (var i = 0; i < Codes.Length; i++)
            {
                table.FindByCodeAndNo(Codes[i], 1).Value.ShouldBe(i * 10 + 1);
                table.FindByCodeAndNo(Codes[i], 2).Value.ShouldBe(i * 10 + 2);
                table.FindByCode(Codes[i]).Select(x => x.No).ToArray().ShouldBe(new[] { 1, 2 });
            }
            table.TryFindByCodeAndNo("A", 1, out _).ShouldBeFalse();
            table.FindRangeByCodeAndNo("a", 2, "a1", 1).Select(x => x.Code + "/" + x.No).ToArray().ShouldBe(new[] { "a/2", "a-b/1", "a-b/2", "a1/1" });
        }

        [Fact]
        public void NullableString()
        {
            var table = ReadV3().V3NullableMasterTable;

            table.All.Select(x => x.Code).ToArray().ShouldBe(new[] { "B", "C", "a", "a-b", "a1", "a_b", "ab", "d" });
            for (var i = 0; i < Codes.Length; i++)
            {
                table.FindByCode(Codes[i]).Value.ShouldBe(i);
            }
            table.FindClosestByCode("D", selectLower: true)!.Code.ShouldBe("C");
        }

        [Fact]
        public void OptionThatV3DidNotApply()
        {
            var table = ReadV3().V3OptionMasterTable;

            table.All.Select(x => x.Code).ToArray().ShouldBe(Codes.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray());
            for (var i = 0; i < Codes.Length; i++)
            {
                table.FindByCode(Codes[i]).Value.ShouldBe(i);
                table.FindByCode(Codes[i].ToUpperInvariant()).Value.ShouldBe(i);
            }
        }

        // what was read from v3 is written back in the order of v4, and is then used as it is
        [Fact]
        public void RebuiltInTheOrderOfV4()
        {
            var rebuilt = new MemoryDatabase(ReadV3().ToDatabaseBuilder().Build());

            rebuilt.V3CompositeMasterTable.All.Select(x => x.Code).Distinct().ToArray().ShouldBe(new[] { "B", "C", "a", "a-b", "a1", "a_b", "ab", "d" });
            rebuilt.V3NullableMasterTable.FindByCode("a_b").Value.ShouldBe(0);
            rebuilt.V3OptionMasterTable.FindByCode("AB").Value.ShouldBe(7);
        }

        // Rows in order are used as they are: the array is the one handed in. Rows out of order are put in order in a
        // copy, and the array handed in is left as it was.
        [Fact]
        public void OnlyRowsOutOfOrderAreCopied()
        {
            var sorted = Codes.OrderBy(x => x, StringComparer.Ordinal).Select((c, i) => new V3NullableMaster { Code = c, Value = i }).ToArray();
            new V3NullableMasterTable(sorted).GetRawDataUnsafe().ShouldBeSameAs(sorted);

            var unsorted = Codes.Select((c, i) => new V3NullableMaster { Code = c, Value = i }).ToArray();
            var table = new V3NullableMasterTable(unsorted);
            table.GetRawDataUnsafe().ShouldNotBeSameAs(unsorted);
            unsorted.Select(x => x.Code).ToArray().ShouldBe(Codes);
            table.All.Select(x => x.Code).ToArray().ShouldBe(Codes.OrderBy(x => x, StringComparer.Ordinal).ToArray());

            new V3NullableMasterTable(Array.Empty<V3NullableMaster>()).Count.ShouldBe(0);
        }
    }
}
