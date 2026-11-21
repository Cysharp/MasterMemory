using MasterMemory.Internal;
using MasterMemory.Tests.TestStructures;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    // Golden/v3.bin was written from this data set by MasterMemory 3.0.4 (MessagePack 3.1.3, its managed LZ4).
    // It pins the database format: the header, the Lz4Block envelope of each table, and the member layout.
    // Do not regenerate it with the current code; a database a shipped client holds was written by that version.
    public static class GoldenData
    {
        static readonly string[] FirstNames = { "Aoi", "Haruto", "Yui", "Sota", "Hina", "Ren", "Mio", "Kaito", "Sakura", "Riku", "Akari", "Yuto", "Noa", "Hinata", "Ema", "Itsuki", "Rin" };
        static readonly string[] LastNames = { "Sato", "Suzuki", "Takahashi", "Tanaka", "Watanabe", "Ito", "Yamamoto", "Nakamura", "Kobayashi", "Kato", "Yoshida", "Yamada", "Sasaki" };

        public static Sample[] Samples() => new[]
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
            new Sample { Id = 999, Age = 10, FirstName = null, LastName = "abcde" },
        };

        public static SkillMaster[] Skills() => new[]
        {
            new SkillMaster(1, 1, 10, "Fire", "small flame"),
            new SkillMaster(1, 2, 25, "Fire", "flame"),
            new SkillMaster(2, 1, 12, "氷結", "敵を凍らせる"),
            new SkillMaster(2, 2, 30, "氷結", "敵全体を凍らせる🧊"),
            new SkillMaster(3, 1, -5, "Heal", ""),
        };

        // Exp walks through every integer width MessagePack has
        public static UserLevel[] Levels() => Enumerable.Range(1, 300).Select(x => new UserLevel(x, x * x * 1000)).ToArray();

        // one row: below the 64 bytes under which Lz4Block leaves a message uncompressed
        public static ItemMaster[] Items() => new[] { new ItemMaster { ItemId = 1 } };

        public static QuestMaster[] Quests() => new[]
        {
            new QuestMaster { QuestId = 1, Name = "first", RewardItemId = 1, Cost = 10 },
            new QuestMaster { QuestId = 2, Name = "second", RewardItemId = 1, Cost = 20 },
            new QuestMaster { QuestId = 3, Name = "third", RewardItemId = 1, Cost = 100 },
        };

        public static TestMaster[] Tests() => new[]
        {
            new TestMaster(1, 100),
            new TestMaster(1, 200),
            new TestMaster(2, 300),
        };

        // large and repetitive: a table LZ4 really compresses
        public static PersonModel[] People() => Enumerable.Range(0, 5000).Select(x => new PersonModel
        {
            RandomId = (x * 7919 % 100003).ToString("D8"),
            FirstName = FirstNames[x % FirstNames.Length],
            LastName = LastNames[x % LastNames.Length],
        }).ToArray();

        public static DatabaseBuilder CreateBuilder(MessagePackFormatterFactory formatterFactory = null)
        {
            return new DatabaseBuilder(formatterFactory)
                .Append(Samples())
                .Append(Skills())
                .Append(Levels())
                .Append(Items())
                .Append(Quests())
                .Append(Tests())
                .Append(People())
                .Append(Array.Empty<ItemMasterEmptyValidate>());
        }

        public static byte[] ReadV3Binary() => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Golden", "v3.bin"));
    }

    public class CompatibilityTest
    {
        static void ShouldBeGoldenData(MemoryDatabase db)
        {
            db.SampleTable.All.Select(x => (x.Id, x.Age, x.FirstName, x.LastName)).ToArray()
                .ShouldBe(GoldenData.Samples().OrderBy(x => x.Id).Select(x => (x.Id, x.Age, x.FirstName, x.LastName)).ToArray());

            db.SkillMasterTable.All.Select(x => (x.SkillId, x.SkillLevel, x.AttackPower, x.SkillName, x.Description)).ToArray()
                .ShouldBe(GoldenData.Skills().OrderBy(x => x.SkillId).ThenBy(x => x.SkillLevel).Select(x => (x.SkillId, x.SkillLevel, x.AttackPower, x.SkillName, x.Description)).ToArray());

            db.UserLevelTable.All.Select(x => (x.Level, x.Exp)).ToArray()
                .ShouldBe(GoldenData.Levels().Select(x => (x.Level, x.Exp)).ToArray());

            db.ItemMasterTable.All.Select(x => x.ItemId).ToArray().ShouldBe(new[] { 1 });

            db.QuestMasterTable.All.Select(x => (x.QuestId, x.Name, x.RewardItemId, x.Cost)).ToArray()
                .ShouldBe(GoldenData.Quests().Select(x => (x.QuestId, x.Name, x.RewardItemId, x.Cost)).ToArray());

            db.TestMasterTable.All.Select(x => (x.TestID, x.Value)).OrderBy(x => x).ToArray()
                .ShouldBe(GoldenData.Tests().Select(x => (x.TestID, x.Value)).OrderBy(x => x).ToArray());

            db.PersonModelTable.All.Select(x => (x.RandomId, x.FirstName, x.LastName)).ToArray()
                .ShouldBe(GoldenData.People().OrderBy(x => x.RandomId, StringComparer.Ordinal).Select(x => (x.RandomId, x.FirstName, x.LastName)).ToArray());

            db.ItemMasterEmptyValidateTable.Count.ShouldBe(0);

            // a table the binary does not contain is empty, not an error
            db.FailTable.Count.ShouldBe(0);
        }

        // name -> the MessagePack bytes of the table, out of its Lz4Block envelope
        static Dictionary<string, byte[]> ReadTablePayloads(byte[] databaseBinary)
        {
            var header = DatabaseHeader.Read(databaseBinary, out var headerLength);
            var result = new Dictionary<string, byte[]>();
            foreach (var item in header)
            {
                var table = databaseBinary.AsSpan(headerLength + item.Value.offset, item.Value.count);
                if (!SerializerOptionsCache.TableProcessor.TryDecode(table, out var decoded))
                {
                    result.Add(item.Key, table.ToArray());
                    continue;
                }

                using (decoded)
                {
                    if (decoded.TryGetValue(out DecodedMessage.Buffer buffer))
                    {
                        result.Add(item.Key, System.Buffers.BuffersExtensions.ToArray(buffer.Sequence));
                    }
                    else if (decoded.TryGetValue(out DecodedMessage.SourceSlice slice))
                    {
                        result.Add(item.Key, table.Slice(slice.Offset, slice.Length).ToArray());
                    }
                }
            }
            return result;
        }

        [Fact]
        public void ReadV3Binary()
        {
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.ReadV3Binary()));
        }

        [Fact]
        public void ReadV3BinaryInParallelWithoutIntern()
        {
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.ReadV3Binary(), internString: false, maxDegreeOfParallelism: 4));
        }

        [Fact]
        public void ReadV3BinaryThroughTheJitChain()
        {
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.ReadV3Binary(), formatterFactory: MessagePackFormatterFactory.Default));
        }

        [Fact]
        public void RoundTrip()
        {
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.CreateBuilder().Build()));
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.CreateBuilder(MessagePackFormatterFactory.Default).Build(), formatterFactory: MessagePackFormatterFactory.Default));
        }

        // What v3 reads is decided by the header and by the MessagePack bytes inside each envelope. Both must be what
        // v3 wrote; the compressed bytes themselves may differ, since the LZ4 implementation is another one.
        [Fact]
        public void WritesWhatV3Wrote()
        {
            var v3 = GoldenData.ReadV3Binary();
            var current = GoldenData.CreateBuilder().Build();

            var v3Header = DatabaseHeader.Read(v3, out _);
            var currentHeader = DatabaseHeader.Read(current, out _);
            currentHeader.Keys.ToArray().ShouldBe(v3Header.Keys.ToArray()); // same tables, same order

            var v3Tables = ReadTablePayloads(v3);
            var currentTables = ReadTablePayloads(current);
            foreach (var item in v3Tables)
            {
                currentTables[item.Key].ShouldBe(item.Value, "table: " + item.Key);
            }

            // the uncompressed tables are stored verbatim, so there the stored bytes are equal as well
            foreach (var tableName in new[] { "item_master", "item_master_empty" })
            {
                currentHeader[tableName].count.ShouldBe(v3Header[tableName].count);
            }
        }

        [Fact]
        public void HeaderIsByteIdentical()
        {
            // the same tables at the same offsets: the header v3 wrote, byte for byte
            var v3 = GoldenData.ReadV3Binary();
            var header = DatabaseHeader.Read(v3, out var headerLength);

            using var stream = new MemoryStream();
            DatabaseHeader.Write(stream, header);

            stream.ToArray().ShouldBe(v3.AsSpan(0, headerLength).ToArray());
        }

        [Fact]
        public void ToDatabaseBuilder()
        {
            var db = new MemoryDatabase(GoldenData.ReadV3Binary());
            ShouldBeGoldenData(new MemoryDatabase(db.ToDatabaseBuilder().Build()));
            ShouldBeGoldenData(new MemoryDatabase(db.ToDatabaseBuilder(MessagePackFormatterFactory.Default).Build()));
        }

        [Fact]
        public void InternString()
        {
            // built at run time, so nothing but the database can have interned it
            var name = Guid.NewGuid().ToString();
            var bin = new DatabaseBuilder().Append(new[] { new Sample { Id = 1, Age = 2, FirstName = name, LastName = "last" } }).Build();

            var notInterned = new MemoryDatabase(bin, internString: false).SampleTable.FindById(1).FirstName;
            notInterned.ShouldBe(name);
            string.IsInterned(notInterned).ShouldBeNull();

            var interned = new MemoryDatabase(bin, internString: true).SampleTable.FindById(1).FirstName;
            interned.ShouldBe(name);
            ReferenceEquals(string.IsInterned(interned), interned).ShouldBeTrue();
        }

        [Fact]
        public void DumpAsJson()
        {
            var tables = MemoryDatabase.GetTableInfo(GoldenData.ReadV3Binary()).ToDictionary(x => x.TableName);

            // stored without an envelope
            tables["item_master"].DumpAsJson().ShouldBe("[{\"ItemId\":1}]");
            tables["item_master_empty"].DumpAsJson().ShouldBe("[]");

            // stored as Lz4Block
            tables["quest_master"].DumpAsJson().ShouldBe(
                "[{\"QuestId\":1,\"Name\":\"first\",\"RewardItemId\":1,\"Cost\":10}," +
                "{\"QuestId\":2,\"Name\":\"second\",\"RewardItemId\":1,\"Cost\":20}," +
                "{\"QuestId\":3,\"Name\":\"third\",\"RewardItemId\":1,\"Cost\":100}]");

            Should.Throw<InvalidOperationException>(() => MemoryDatabase.GetTableInfo(GoldenData.ReadV3Binary(), storeTableData: false)[0].DumpAsJson());
        }
    }
}
