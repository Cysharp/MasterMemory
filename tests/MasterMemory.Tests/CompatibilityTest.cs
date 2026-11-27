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
    //
    // v3 compressed every table with LZ4, whatever it was asked. v4 writes a table as it is and leaves compression to
    // the options the application passes, so a database of v3 is read with options that have the LZ4 processor.
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

        // Options with LZ4 from the MessagePack.LZ4 package: what a database of v3 is read with, and
        // what a builder writes the format of v3 with.
        public static readonly MemoryDatabaseOptions Lz4 = new MemoryDatabaseOptions { MessageProcessor = new Lz4BlockProcessor() };

        // MessagePack's chain for the JIT, which also serves types through reflection
        public static readonly MemoryDatabaseOptions Jit = new MemoryDatabaseOptions { FormatterFactory = MessagePackFormatterFactory.Default };

        public static DatabaseBuilder CreateBuilder(MemoryDatabaseOptions options = null)
        {
            return new DatabaseBuilder(options)
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
            db.SampleTable.All.AsReadOnlyList().Select(x => (x.Id, x.Age, x.FirstName, x.LastName)).ToArray()
                .ShouldBe(GoldenData.Samples().OrderBy(x => x.Id).Select(x => (x.Id, x.Age, x.FirstName, x.LastName)).ToArray());

            db.SkillMasterTable.All.AsReadOnlyList().Select(x => (x.SkillId, x.SkillLevel, x.AttackPower, x.SkillName, x.Description)).ToArray()
                .ShouldBe(GoldenData.Skills().OrderBy(x => x.SkillId).ThenBy(x => x.SkillLevel).Select(x => (x.SkillId, x.SkillLevel, x.AttackPower, x.SkillName, x.Description)).ToArray());

            db.UserLevelTable.All.AsReadOnlyList().Select(x => (x.Level, x.Exp)).ToArray()
                .ShouldBe(GoldenData.Levels().Select(x => (x.Level, x.Exp)).ToArray());

            db.ItemMasterTable.All.AsReadOnlyList().Select(x => x.ItemId).ToArray().ShouldBe(new[] { 1 });

            db.QuestMasterTable.All.AsReadOnlyList().Select(x => (x.QuestId, x.Name, x.RewardItemId, x.Cost)).ToArray()
                .ShouldBe(GoldenData.Quests().Select(x => (x.QuestId, x.Name, x.RewardItemId, x.Cost)).ToArray());

            db.TestMasterTable.All.AsReadOnlyList().Select(x => (x.TestID, x.Value)).OrderBy(x => x).ToArray()
                .ShouldBe(GoldenData.Tests().Select(x => (x.TestID, x.Value)).OrderBy(x => x).ToArray());

            db.PersonModelTable.All.AsReadOnlyList().Select(x => (x.RandomId, x.FirstName, x.LastName)).ToArray()
                .ShouldBe(GoldenData.People().OrderBy(x => x.RandomId, StringComparer.Ordinal).Select(x => (x.RandomId, x.FirstName, x.LastName)).ToArray());

            db.ItemMasterEmptyValidateTable.Count.ShouldBe(0);

            // a table the binary does not contain is empty, not an error
            db.FailTable.Count.ShouldBe(0);
        }

        static readonly Lz4BlockProcessor Lz4Processor = new Lz4BlockProcessor();

        // name -> the MessagePack bytes of the table, out of its Lz4Block envelope if it is in one
        static Dictionary<string, byte[]> ReadTablePayloads(byte[] databaseBinary)
        {
            var header = DatabaseHeader.Read(databaseBinary, out var headerLength);
            var result = new Dictionary<string, byte[]>();
            foreach (var item in header)
            {
                var table = databaseBinary.AsSpan(headerLength + item.Value.offset, item.Value.count);
                if (!Lz4Processor.TryDecode(table, out var decoded))
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
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.ReadV3Binary(), GoldenData.Lz4));
        }

        [Fact]
        public void ReadV3BinaryInParallel()
        {
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.ReadV3Binary(), GoldenData.Lz4, maxDegreeOfParallelism: 4));
        }

        [Fact]
        public void ReadV3BinaryThroughTheJitChain()
        {
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.ReadV3Binary(), new MemoryDatabaseOptions { FormatterFactory = MessagePackFormatterFactory.Default, MessageProcessor = new Lz4BlockProcessor() }));
        }

        // Read with nothing that decompresses, a database of v3 fails at its first compressed table. What it fails with
        // names the table and says what to pass; the formatter of the rows would say only that an ext is where an
        // array should be.
        [Fact]
        public void V3BinaryWithoutAProcessorSaysWhatIsMissing()
        {
            var v3 = GoldenData.ReadV3Binary();

            static void ShouldExplain(Exception ex)
            {
                ex.ShouldBeOfType<InvalidOperationException>();
                ex.Message.ShouldContain("is compressed with LZ4");
                ex.Message.ShouldContain("MessagePack.LZ4");
                ex.Message.ShouldContain("new MemoryDatabaseOptions { MessageProcessor = new Lz4BlockProcessor() }");
            }

            ShouldExplain(Should.Throw<InvalidOperationException>(() => new MemoryDatabase(v3)));
            ShouldExplain(Should.Throw<InvalidOperationException>(() => new MemoryDatabase(v3, new MemoryDatabaseOptions { InternString = false })));
            ShouldExplain(Should.Throw<InvalidOperationException>(() => new MemoryDatabase(new ReadOnlySpan<byte>(v3))));
            ShouldExplain(Should.Throw<InvalidOperationException>(() => new MemoryDatabase(v3, new MemoryDatabaseOptions { FormatterFactory = MessagePackFormatterFactory.DefaultAot })));

            // tables read at the same time: each failure is this one
            var parallel = Should.Throw<AggregateException>(() => new MemoryDatabase(v3, maxDegreeOfParallelism: 4));
            parallel.InnerExceptions.Count.ShouldBeGreaterThan(0);
            foreach (var inner in parallel.InnerExceptions)
            {
                ShouldExplain(inner);
            }

            // a table of v3 that was too small to compress is not mistaken for one that is
            TableSerialization.LooksCompressed(ReadStoredTable(v3, "item_master")).ShouldBeFalse();
            TableSerialization.LooksCompressed(ReadStoredTable(v3, "quest_master")).ShouldBeTrue();
        }

        static byte[] ReadStoredTable(byte[] databaseBinary, string tableName)
        {
            var (offset, count) = DatabaseHeader.Read(databaseBinary, out var headerLength)[tableName];
            return databaseBinary.AsSpan(headerLength + offset, count).ToArray();
        }

        [Fact]
        public void RoundTrip()
        {
            // as a database is by default: the tables as they are
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.CreateBuilder().Build()));

            // compressed, when the builder and the database are both given the processor
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.CreateBuilder(GoldenData.Lz4).Build(), GoldenData.Lz4));

            // a database that was written without a processor is read with one: a processor leaves a message
            // that is not its own as it is
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.CreateBuilder().Build(), GoldenData.Lz4));

            // the JIT chain
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.CreateBuilder(GoldenData.Jit).Build(), GoldenData.Jit));
        }

        // Compression is the application's to choose: nothing is compressed unless the builder is given a processor.
        [Fact]
        public void TablesAreWrittenAsTheyAreUnlessAProcessorIsGiven()
        {
            var plain = GoldenData.CreateBuilder().Build();
            var compressed = GoldenData.CreateBuilder(GoldenData.Lz4).Build();

            foreach (var table in DatabaseHeader.Read(plain, out _).Keys)
            {
                TableSerialization.LooksCompressed(ReadStoredTable(plain, table)).ShouldBeFalse(table);
            }

            // the table of 5,000 people is what LZ4 is for; the one of a single row is left as it is by LZ4 too
            TableSerialization.LooksCompressed(ReadStoredTable(compressed, "people")).ShouldBeTrue();
            TableSerialization.LooksCompressed(ReadStoredTable(compressed, "item_master")).ShouldBeFalse();
            compressed.Length.ShouldBeLessThan(plain.Length / 2);

            // what is inside the envelopes is what the plain database holds: the same MessagePack bytes
            var plainTables = ReadTablePayloads(plain);
            foreach (var item in ReadTablePayloads(compressed))
            {
                plainTables[item.Key].ShouldBe(item.Value, "table: " + item.Key);
            }
        }
        // The binary as a part of a larger buffer, which is what a ReadOnlyMemory or a span usually is: a rented array,
        // a file read into a buffer that is reused. What is around it is not zero, so reading past either end shows.
        static byte[] InsideLargerBuffer(byte[] binary, out int offset)
        {
            offset = 37;
            var buffer = new byte[binary.Length + 100];
            new Random(1).NextBytes(buffer);
            binary.CopyTo(buffer, offset);
            return buffer;
        }

        [Fact]
        public void ReadFromMemory()
        {
            var binary = GoldenData.CreateBuilder().Build();
            var buffer = InsideLargerBuffer(binary, out var offset);

            ShouldBeGoldenData(new MemoryDatabase(new ReadOnlyMemory<byte>(buffer, offset, binary.Length)));
            ShouldBeGoldenData(new MemoryDatabase(new ReadOnlyMemory<byte>(buffer, offset, binary.Length), new MemoryDatabaseOptions { InternString = false }, maxDegreeOfParallelism: 4));
            ShouldBeGoldenData(new MemoryDatabase(new Memory<byte>(buffer, offset, binary.Length)));
        }

        [Fact]
        public void ReadFromSpan()
        {
            var binary = GoldenData.CreateBuilder().Build();
            var buffer = InsideLargerBuffer(binary, out var offset);

            ShouldBeGoldenData(new MemoryDatabase(new ReadOnlySpan<byte>(buffer, offset, binary.Length)));
            ShouldBeGoldenData(new MemoryDatabase(new ReadOnlySpan<byte>(buffer, offset, binary.Length), new MemoryDatabaseOptions { InternString = false }));
            ShouldBeGoldenData(new MemoryDatabase(buffer.AsSpan(offset, binary.Length), GoldenData.Jit));

            // a database of v3 the same way
            var v3 = GoldenData.ReadV3Binary();
            ShouldBeGoldenData(new MemoryDatabase(new ReadOnlySpan<byte>(InsideLargerBuffer(v3, out offset), offset, v3.Length), GoldenData.Lz4));
        }

        // The database does not keep what it was read from: the buffer can be reused as soon as the constructor returns.
        [Fact]
        public void TheBinaryIsNotKept()
        {
            var binary = GoldenData.CreateBuilder().Build();
            var fromSpan = new MemoryDatabase(binary.AsSpan());
            var fromMemory = new MemoryDatabase(binary.AsMemory(), maxDegreeOfParallelism: 4);
            var fromArray = new MemoryDatabase(binary);

            Array.Clear(binary, 0, binary.Length);

            ShouldBeGoldenData(fromSpan);
            ShouldBeGoldenData(fromMemory);
            ShouldBeGoldenData(fromArray);
        }

        // The header is read from a span with MessagePackPrimitives, whatever the binary is given as. What it reads from
        // a part of a larger buffer is what it reads from the array itself, and it stops where the header ends.
        [Fact]
        public void HeaderFromArrayMemoryAndSpan()
        {
            foreach (var binary in new[] { GoldenData.CreateBuilder().Build(), GoldenData.ReadV3Binary() })
            {
                var buffer = InsideLargerBuffer(binary, out var offset);

                var fromArray = DatabaseHeader.Read(binary, out var lengthFromArray);
                var fromMemory = DatabaseHeader.Read(new ReadOnlyMemory<byte>(buffer, offset, binary.Length), out var lengthFromMemory);
                var fromSpan = DatabaseHeader.Read(new ReadOnlySpan<byte>(buffer, offset, binary.Length), out var lengthFromSpan);

                fromArray.Count.ShouldBeGreaterThan(0);
                fromMemory.OrderBy(x => x.Key).ShouldBe(fromArray.OrderBy(x => x.Key));
                fromSpan.OrderBy(x => x.Key).ShouldBe(fromArray.OrderBy(x => x.Key));
                lengthFromMemory.ShouldBe(lengthFromArray);
                lengthFromSpan.ShouldBe(lengthFromArray);

                // every table lies inside what follows the header
                foreach (var item in fromArray)
                {
                    DatabaseHeader.ThrowIfOutside(item.Key, item.Value.offset, item.Value.count, binary.Length - lengthFromArray);
                }
            }
        }

        // A header that is not one is an InvalidOperationException that says what is wrong, at every place it can break.
        [Fact]
        public void DamagedHeader()
        {
            static string Rejected(params byte[] header) => Should.Throw<InvalidOperationException>(() => DatabaseHeader.Read(header, out _)).Message;

            Rejected().ShouldContain("ends where the number of tables should be");
            Rejected(0x92).ShouldContain("where the number of tables should be is something else");   // an array, not a map
            Rejected(0xDE, 0x00).ShouldContain("ends where the number of tables should be");            // map16 cut short
            Rejected(0xDF, 0x7F, 0xFF, 0xFF, 0xFF).ShouldContain("lists 2147483647 tables");             // more tables than bytes
            Rejected(0x81, 0xC0, 0x92, 0x00, 0x00).ShouldContain("table name is nil");
            Rejected(0x81, 0x01, 0x92, 0x00, 0x00).ShouldContain("where a table name should be is something else");
            Rejected(0x81, 0xA1, (byte)'t', 0x93, 0x00, 0x00, 0x00).ShouldContain("must be [offset, count]");
            Rejected(0x81, 0xA1, (byte)'t', 0x92, 0xA0, 0x00).ShouldContain("where the offset of the table 't' should be is something else");
            Rejected(0x81, 0xA1, (byte)'t', 0x92, 0x00, 0xCB, 0, 0, 0, 0).ShouldContain("where the length of the table 't' should be is something else"); // a float64
            Rejected(0x81, 0xA1, (byte)'t', 0x92, 0x00, 0xD2, 0, 0).ShouldContain("ends where the length of the table 't' should be");

            // the header of a real database, cut at every length: rejected each time, and never by an exception from inside
            var binary = GoldenData.CreateBuilder().Build();
            DatabaseHeader.Read(binary, out var headerLength);
            for (var length = 0; length < headerLength; length++)
            {
                Should.Throw<InvalidOperationException>(() => DatabaseHeader.Read(new ReadOnlySpan<byte>(binary, 0, length), out _));
            }
            DatabaseHeader.Read(new ReadOnlySpan<byte>(binary, 0, headerLength), out var exact).Count.ShouldBeGreaterThan(0);
            exact.ShouldBe(headerLength);
        }

        [Fact]
        public void TableInfoFromSpan()
        {
            var binary = GoldenData.CreateBuilder().Build();
            var buffer = InsideLargerBuffer(binary, out var offset);

            var fromArray = MemoryDatabase.GetTableInfo(binary);
            var fromSpan = MemoryDatabase.GetTableInfo(new ReadOnlySpan<byte>(buffer, offset, binary.Length));

            fromSpan.Select(x => (x.TableName, x.Size)).ShouldBe(fromArray.Select(x => (x.TableName, x.Size)));
            fromSpan.Select(x => x.DumpAsJson()).ShouldBe(fromArray.Select(x => x.DumpAsJson()));

            Should.Throw<InvalidOperationException>(() => MemoryDatabase.GetTableInfo(new ReadOnlySpan<byte>(buffer, offset, binary.Length), storeTableData: false)[0].DumpAsJson());
        }

        [Fact]
        public void NullBinary()
        {
            Should.Throw<ArgumentNullException>(() => new MemoryDatabase((byte[])null!));
            Should.Throw<ArgumentNullException>(() => MemoryDatabase.GetTableInfo((byte[])null!));
        }

        // the same rows as a database with no binary in between, and the tables of a database through a builder
        [Fact]
        public void StraightFromRows()
        {
            ShouldBeGoldenData(GoldenData.CreateBuilder().ToMemoryDatabase());
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.ReadV3Binary(), GoldenData.Lz4).ToDatabaseBuilder().ToMemoryDatabase());
        }

        // What v3 reads is decided by the header and by the MessagePack bytes inside each envelope. Both must be what
        // v3 wrote; the compressed bytes themselves may differ, since the LZ4 implementation is another one. Given the
        // LZ4 processor, a builder of v4 writes the format of v3.
        [Fact]
        public void WritesWhatV3Wrote()
        {
            var v3 = GoldenData.ReadV3Binary();
            var current = GoldenData.CreateBuilder(GoldenData.Lz4).Build();

            // and without it, the same MessagePack bytes that v3 put into its envelopes, bare
            var bare = ReadTablePayloads(GoldenData.CreateBuilder().Build());
            foreach (var item in ReadTablePayloads(v3))
            {
                bare[item.Key].ShouldBe(item.Value, "table: " + item.Key);
            }

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

        // The header is written with MessagePackPrimitives. MessagePackWriter, which wrote it before, is the reference
        // here: the same bytes for every shape of header, at each place where the form of a token changes (15 / 16
        // tables, names of 31 / 32 and 255 / 256 bytes), and the header read back is the header that was written.
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(300)]
        public void HeaderIsWhatMessagePackWriterWrites(int tables)
        {
            var nameLengths = new[] { 0, 1, 31, 32, 255, 256, 70000 };
            var numbers = new[] { 0, 1, -1, 127, 128, 65535, 65536, int.MaxValue, int.MinValue };

            var header = new Dictionary<string, (int offset, int count)>();
            for (var i = 0; i < tables; i++)
            {
                // a prefix that makes each name its own, then padding to the length, with characters of one to three bytes
                var name = (i + "_テーブルé_").PadRight(nameLengths[i % nameLengths.Length], 'x');
                header.Add(name, (numbers[i % numbers.Length], numbers[(i + 4) % numbers.Length]));
            }

            using var expected = new MemoryStream();
            using (var writer = new MessagePackWriter())
            {
                writer.WriteMapHeader(header.Count);
                foreach (var item in header)
                {
                    writer.WriteString(item.Key);
                    writer.WriteArrayHeader(2);
                    writer.WriteForcedInt32(item.Value.offset);
                    writer.WriteForcedInt32(item.Value.count);
                }
                writer.WriteTo(expected);
            }

            using var actual = new MemoryStream();
            DatabaseHeader.Write(actual, header);
            actual.ToArray().ShouldBe(expected.ToArray());

            var read = DatabaseHeader.Read(actual.ToArray().Concat(new byte[tables * 4]).ToArray(), out var headerLength);
            read.ShouldBe(header);
            headerLength.ShouldBe((int)actual.Length);
        }

        [Fact]
        public void ToDatabaseBuilder()
        {
            var db = new MemoryDatabase(GoldenData.ReadV3Binary(), GoldenData.Lz4);

            // the builder of a database writes with the options the database was read with: here, compressed again
            db.Options.ShouldBeSameAs(GoldenData.Lz4);
            db.ToDatabaseBuilder().Options.ShouldBeSameAs(GoldenData.Lz4);
            var compressed = db.ToDatabaseBuilder().Build();
            TableSerialization.LooksCompressed(ReadStoredTable(compressed, "people")).ShouldBeTrue();
            ShouldBeGoldenData(new MemoryDatabase(compressed, GoldenData.Lz4));

            // or with the options it is given
            var plain = db.ToDatabaseBuilder(MemoryDatabaseOptions.Default).Build();
            TableSerialization.LooksCompressed(ReadStoredTable(plain, "people")).ShouldBeFalse();
            ShouldBeGoldenData(new MemoryDatabase(plain));
            ShouldBeGoldenData(new MemoryDatabase(db.ToDatabaseBuilder(GoldenData.Jit).Build(), GoldenData.Jit));

            // the options go on from one to the next: database, builder, database, builder
            var again = db.ToDatabaseBuilder().ToMemoryDatabase();
            again.Options.ShouldBeSameAs(GoldenData.Lz4);
            TableSerialization.LooksCompressed(ReadStoredTable(again.ToDatabaseBuilder().Build(), "people")).ShouldBeTrue();

            // with none given, the default ones
            new MemoryDatabase(plain).Options.ShouldBeSameAs(MemoryDatabaseOptions.Default);
            new DatabaseBuilder().Options.ShouldBeSameAs(MemoryDatabaseOptions.Default);
            new DatabaseBuilder(null).ToMemoryDatabase().Options.ShouldBeSameAs(MemoryDatabaseOptions.Default);
        }

        // Strings are interned by MessagePack's StringInterningFormatterFactory, into a pool that is the database's
        // while it is loaded: within a database a string is one object, wherever it occurs.
        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        public void InternString(int maxDegreeOfParallelism)
        {
            // built at run time, so that it is no literal of this assembly
            var name = Guid.NewGuid().ToString();
            var bin = new DatabaseBuilder()
                .Append(new[]
                {
                    new Sample { Id = 1, Age = 2, FirstName = name, LastName = "last" },
                    new Sample { Id = 2, Age = 3, FirstName = name, LastName = name },
                })
                .Append(new[] { new QuestMaster { QuestId = 1, Name = name, RewardItemId = 1, Cost = 1 } })
                .Build();

            var plain = new MemoryDatabase(bin, new MemoryDatabaseOptions { InternString = false }, maxDegreeOfParallelism: maxDegreeOfParallelism);
            plain.SampleTable.FindById(1).FirstName.ShouldBe(name);
            ReferenceEquals(plain.SampleTable.FindById(1).FirstName, plain.SampleTable.FindById(2).FirstName).ShouldBeFalse();

            var interned = new MemoryDatabase(bin, maxDegreeOfParallelism: maxDegreeOfParallelism);
            var first = interned.SampleTable.FindById(1).FirstName;
            first.ShouldBe(name);
            first.ShouldNotBeSameAs(name);

            // the same string in two rows, in two members of a row, and in two tables
            interned.SampleTable.FindById(2).FirstName.ShouldBeSameAs(first);
            interned.SampleTable.FindById(2).LastName.ShouldBeSameAs(first);
            interned.QuestMasterTable.FindByQuestId(1).Name.ShouldBeSameAs(first);

            // from a span as well
            var fromSpan = new MemoryDatabase(new ReadOnlySpan<byte>(bin));
            fromSpan.SampleTable.FindById(2).LastName.ShouldBeSameAs(fromSpan.SampleTable.FindById(1).FirstName);
        }

        // The pool is emptied when the database is loaded, so it keeps no string alive: the strings of a database go
        // with the database. A second database, loaded afterwards, has strings of its own. (Until v3 the strings went
        // into the runtime's pool with string.Intern, where they stayed for the life of the process.)
        [Fact]
        public void InternedStringsBelongToTheirDatabase()
        {
            var name = Guid.NewGuid().ToString();
            var bin = new DatabaseBuilder().Append(new[] { new Sample { Id = 1, Age = 2, FirstName = name, LastName = name } }).Build();

            var first = new MemoryDatabase(bin).SampleTable.FindById(1);
            var second = new MemoryDatabase(bin).SampleTable.FindById(1);

            first.LastName.ShouldBeSameAs(first.FirstName);
            second.LastName.ShouldBeSameAs(second.FirstName);
            second.FirstName.ShouldNotBeSameAs(first.FirstName);

            string.IsInterned(first.FirstName).ShouldBeNull();
        }

        // A string that the pool has already is found by its characters and not allocated again (.NET 9 and later), so
        // a table in which values repeat is loaded with less garbage when its strings are interned than when they are not.
        [Fact]
        public void InterningARepeatedStringAllocatesItOnce()
        {
            static long Allocated(Action action)
            {
                var before = GC.GetAllocatedBytesForCurrentThread();
                action();
                return GC.GetAllocatedBytesForCurrentThread() - before;
            }

            const int Rows = 5000;
            var repeated = new string('x', 200); // 400 bytes of characters in each row that is not interned
            var bin = new DatabaseBuilder()
                .Append(Enumerable.Range(0, Rows).Select(i => new Sample { Id = i, Age = i, FirstName = repeated, LastName = repeated }))
                .Build();

            // once each before measuring: the formatters are made on the first load
            new MemoryDatabase(bin);
            new MemoryDatabase(bin, new MemoryDatabaseOptions { InternString = false });

            MemoryDatabase? interned = null, plain = null;
            var withInterning = Allocated(() => interned = new MemoryDatabase(bin));
            var without = Allocated(() => plain = new MemoryDatabase(bin, new MemoryDatabaseOptions { InternString = false }));

            interned!.SampleTable.Count.ShouldBe(Rows);
            plain!.SampleTable.Count.ShouldBe(Rows);

            // two strings of 200 characters for each row are what the load without interning allocates on top
            var strings = (long)Rows * 2 * (200 * sizeof(char));
            (without - withInterning).ShouldBeGreaterThan(strings * 9 / 10, $"with interning {withInterning:N0} bytes, without {without:N0} bytes");
        }
        // The formatter of a table array is put into MessagePack's registry of generated formatters by the generated
        // code, when a database or a builder is first made. From then on a table is read and written with any options:
        // MessagePack's own, or what an application built around them. MasterMemory adds no factory to the chain.
        [Fact]
        public void TableArraysAreRegisteredWithMessagePack()
        {
            new DatabaseBuilder(); // the registration happens on the way to the first builder or database

            var rows = new[] { new Sample { Id = 1, Age = 2, FirstName = "a", LastName = "b" } };

            // the options of MessagePack itself, which MasterMemory never saw
            var bytes = MessagePackSerializer.Serialize(rows, MessagePackSerializerOptions.DefaultAot);
            MessagePackSerializer.Deserialize<Sample[]>(bytes, MessagePackSerializerOptions.DefaultAot)!.Single().LastName.ShouldBe("b");

            // a chain an application put together around the default one
            var own = new MessagePackSerializerOptions(new[] { MessagePackFormatterFactory.DefaultAot });
            MessagePackSerializer.Deserialize<Sample[]>(MessagePackSerializer.Serialize(rows, own), own)!.Length.ShouldBe(1);

            // every table of the database, not the first alone
            MessagePackSerializer.Serialize(GoldenData.Skills(), MessagePackSerializerOptions.DefaultAot).Length.ShouldBeGreaterThan(0);
            MessagePackSerializer.Serialize(GoldenData.People(), MessagePackSerializerOptions.DefaultAot).Length.ShouldBeGreaterThan(0);
        }

        // How the tables are read and written is in one object, which the database and the builder are both given.
        // Null is the default. Interning is MasterMemory's to add, and it is there whatever factory the options have:
        // nobody has to put MessagePack's StringInterningFormatterFactory into a chain of their own.
        [Fact]
        public void OptionsAreGivenToTheDatabaseAndTheBuilderAlike()
        {
            var binary = GoldenData.CreateBuilder().Build();
            var compressed = GoldenData.CreateBuilder(GoldenData.Lz4).Build();
            var aot = new MemoryDatabaseOptions { FormatterFactory = MessagePackFormatterFactory.DefaultAot };

            // on each kind of memory
            ShouldBeGoldenData(new MemoryDatabase(binary, aot));
            ShouldBeGoldenData(new MemoryDatabase(binary, aot, maxDegreeOfParallelism: 4));
            ShouldBeGoldenData(new MemoryDatabase(new ReadOnlyMemory<byte>(binary), GoldenData.Jit, 2));
            ShouldBeGoldenData(new MemoryDatabase(new ReadOnlySpan<byte>(binary), aot));
            ShouldBeGoldenData(new MemoryDatabase(new ReadOnlyMemory<byte>(compressed), GoldenData.Lz4, 2));
            ShouldBeGoldenData(new MemoryDatabase(new ReadOnlySpan<byte>(compressed), GoldenData.Lz4));

            // null is the default
            ShouldBeGoldenData(new MemoryDatabase(binary, null));
            ShouldBeGoldenData(new MemoryDatabase(binary, null, 4));
            ShouldBeGoldenData(new MemoryDatabase(new ReadOnlySpan<byte>(binary), null));
            ShouldBeGoldenData(new MemoryDatabase(GoldenData.CreateBuilder(null).Build()));

            // strings are interned with a factory of the application's as they are without one, with a processor beside
            // it or not, and are not when InternString says so
            var name = Guid.NewGuid().ToString();
            var rows = new[] { new Sample { Id = 1, Age = 2, FirstName = name, LastName = name } };

            foreach (var factory in new MessagePackFormatterFactory[] { null!, MessagePackFormatterFactory.DefaultAot, MessagePackFormatterFactory.Default })
            {
                foreach (var processor in new MessagePackMessageProcessor[] { null!, new Lz4BlockProcessor() })
                {
                    var options = new MemoryDatabaseOptions { FormatterFactory = factory, MessageProcessor = processor };
                    var bin = new DatabaseBuilder(options).Append(rows).Build();

                    var interned = new MemoryDatabase(bin, options).SampleTable.FindById(1);
                    interned.LastName.ShouldBe(name);
                    interned.LastName.ShouldBeSameAs(interned.FirstName);

                    var plain = new MemoryDatabase(bin, options with { InternString = false }).SampleTable.FindById(1);
                    plain.LastName.ShouldBe(name);
                    plain.LastName.ShouldNotBeSameAs(plain.FirstName);
                }
            }
        }

        // Options say what they say and nothing of what they hold: a copy made by `with` is equal to one that was
        // written out, and starts without the formatters the original has made.
        [Fact]
        public void OptionsAreValues()
        {
            MemoryDatabaseOptions.Default.InternString.ShouldBeTrue();
            MemoryDatabaseOptions.Default.ShareInternedStrings.ShouldBeFalse();
            MemoryDatabaseOptions.Default.FormatterFactory.ShouldBeNull();
            MemoryDatabaseOptions.Default.MessageProcessor.ShouldBeNull();
            new MemoryDatabaseOptions().ShouldBe(MemoryDatabaseOptions.Default);

            var processor = new Lz4BlockProcessor();
            var used = new MemoryDatabaseOptions { MessageProcessor = processor };
            var binary = GoldenData.CreateBuilder(used).Build(); // used has made its formatters now
            ShouldBeGoldenData(new MemoryDatabase(binary, used));

            used.ShouldBe(new MemoryDatabaseOptions { MessageProcessor = processor });
            used.GetHashCode().ShouldBe(new MemoryDatabaseOptions { MessageProcessor = processor }.GetHashCode());
            used.ShouldNotBe(MemoryDatabaseOptions.Default);
            (used with { InternString = false }).ShouldNotBe(used);

            // the copy has the processor and another factory, and reads with both
            var copy = used with { FormatterFactory = MessagePackFormatterFactory.Default };
            copy.MessageProcessor.ShouldBeSameAs(processor);
            ShouldBeGoldenData(new MemoryDatabase(binary, copy));

            // and a copy without the processor does not go on reading with the one the original has
            Should.Throw<InvalidOperationException>(() => new MemoryDatabase(binary, used with { MessageProcessor = null }));
        }

        // The pool of interned strings is each database's own by default, so that a database that is dropped takes its
        // strings with it. Options that say ShareInternedStrings have one pool for every database read with them: a
        // database that is loaded again has the string objects of the one before it.
        [Fact]
        public void InternedStringsAreSharedBetweenDatabasesWhenTheOptionsSaySo()
        {
            var name = Guid.NewGuid().ToString();
            var bin = new DatabaseBuilder().Append(new[] { new Sample { Id = 1, Age = 2, FirstName = name, LastName = name } }).Build();
            static string NameOf(MemoryDatabase db) => db.SampleTable.FindById(1).FirstName;

            // by default: one object within a database, another in the next
            var own = new MemoryDatabaseOptions();
            NameOf(new MemoryDatabase(bin, own)).ShouldNotBeSameAs(NameOf(new MemoryDatabase(bin, own)));

            var shared = new MemoryDatabaseOptions { ShareInternedStrings = true };
            var first = new MemoryDatabase(bin, shared);
            var second = new MemoryDatabase(bin, shared);
            var fromSpan = new MemoryDatabase(new ReadOnlySpan<byte>(bin), shared);
            NameOf(first).ShouldBe(name);
            NameOf(second).ShouldBeSameAs(NameOf(first));
            NameOf(fromSpan).ShouldBeSameAs(NameOf(first));
            second.SampleTable.FindById(1).LastName.ShouldBeSameAs(NameOf(first));

            // the pool is of those options: others that say the same have their own
            NameOf(new MemoryDatabase(bin, new MemoryDatabaseOptions { ShareInternedStrings = true })).ShouldNotBeSameAs(NameOf(first));
            NameOf(new MemoryDatabase(bin, shared with { MessageProcessor = new Lz4BlockProcessor() })).ShouldNotBeSameAs(NameOf(first));

            // databases loaded at the same time share it as well
            var databases = new MemoryDatabase[8];
            System.Threading.Tasks.Parallel.For(0, databases.Length, i => databases[i] = new MemoryDatabase(bin, shared, maxDegreeOfParallelism: 2));
            foreach (var database in databases)
            {
                NameOf(database).ShouldBeSameAs(NameOf(first));
            }

            // nothing to share where nothing is interned
            var plain = shared with { InternString = false };
            NameOf(new MemoryDatabase(bin, plain)).ShouldNotBeSameAs(NameOf(new MemoryDatabase(bin, plain)));
        }
        // Databases that are loaded at the same time each have a pool of their own: a string is one object within each
        // of them, and they neither share strings nor wait for one another.
        [Fact]
        public void DatabasesLoadedAtTheSameTimeInternOnTheirOwn()
        {
            var name = Guid.NewGuid().ToString();
            var bin = new DatabaseBuilder()
                .Append(Enumerable.Range(1, 2000).Select(i => new Sample { Id = i, Age = i, FirstName = name, LastName = name + (i % 7) }))
                .Build();

            var databases = new MemoryDatabase[8];
            System.Threading.Tasks.Parallel.For(0, databases.Length, i => databases[i] = new MemoryDatabase(bin, maxDegreeOfParallelism: 2));

            foreach (var database in databases)
            {
                var first = database.SampleTable.FindById(1).FirstName;
                foreach (var row in database.SampleTable.All)
                {
                    row.FirstName.ShouldBeSameAs(first);
                }
                database.SampleTable.FindById(8).LastName.ShouldBeSameAs(database.SampleTable.FindById(1).LastName);
            }

            databases.Select(x => x.SampleTable.FindById(1).FirstName).Distinct(ReferenceComparer.Instance).Count().ShouldBe(databases.Length);
        }

        sealed class ReferenceComparer : IEqualityComparer<string>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            public bool Equals(string? x, string? y) => ReferenceEquals(x, y);

            public int GetHashCode(string obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
        [Fact]
        public void DumpAsJson()
        {
            var tables = MemoryDatabase.GetTableInfo(GoldenData.ReadV3Binary()).ToDictionary(x => x.TableName);

            // stored without an envelope: needs nothing, and is the same with a processor
            tables["item_master"].DumpAsJson().ShouldBe("[{\"ItemId\":1}]");
            tables["item_master_empty"].DumpAsJson().ShouldBe("[]");
            tables["item_master"].DumpAsJson(GoldenData.Lz4).ShouldBe("[{\"ItemId\":1}]");

            // stored as Lz4Block: read with the processor
            const string quests =
                "[{\"QuestId\":1,\"Name\":\"first\",\"RewardItemId\":1,\"Cost\":10}," +
                "{\"QuestId\":2,\"Name\":\"second\",\"RewardItemId\":1,\"Cost\":20}," +
                "{\"QuestId\":3,\"Name\":\"third\",\"RewardItemId\":1,\"Cost\":100}]";
            tables["quest_master"].DumpAsJson(GoldenData.Lz4).ShouldBe(quests);

            // and without one, what is missing is said
            Should.Throw<InvalidOperationException>(() => tables["quest_master"].DumpAsJson()).Message.ShouldContain("is compressed with LZ4");
            Should.Throw<InvalidOperationException>(() => tables["quest_master"].DumpAsJson(null)).Message.ShouldContain("quest_master");

            // a database of v4 has it bare
            MemoryDatabase.GetTableInfo(GoldenData.CreateBuilder().Build()).Single(x => x.TableName == "quest_master").DumpAsJson().ShouldBe(quests);

            Should.Throw<InvalidOperationException>(() => MemoryDatabase.GetTableInfo(GoldenData.ReadV3Binary(), storeTableData: false)[0].DumpAsJson());
        }    }
}
