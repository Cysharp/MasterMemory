using System.Buffers;
using System.Runtime.CompilerServices;
using MasterMemory.Meta;
using MessagePack;

namespace MasterMemory.AotTests
{
    /// <summary>
    /// What an application does with MasterMemory, as checks that say what went wrong: building a database, loading it
    /// from an array, memory and a span, every kind of lookup, the views, changing a database through its builder,
    /// validation, the metadata and an importer that goes by it. Each check stands alone, so one that fails does not
    /// hide the others.
    /// </summary>
    public static class AotChecks
    {
        /// <summary>Runs every check, writes a line for each, and returns how many failed.</summary>
        public static int Run(TextWriter output)
        {
            var checks = new CheckRunner(output);
            var binary = CreateBuilder().Build();
            var database = new MemoryDatabase(binary);

            checks.Run("load: array, memory in parallel, span", () =>
            {
                Equal(5, new MemoryDatabase(binary).MonsterTable.Count, "array");
                Equal(5, new MemoryDatabase(new ReadOnlyMemory<byte>(binary), new MemoryDatabaseOptions { InternString = false }, maxDegreeOfParallelism: 4).MonsterTable.Count, "memory");
                Equal(5, new MemoryDatabase(new ReadOnlySpan<byte>(binary)).MonsterTable.Count, "span");

                // a part of a rented buffer, with something else around it
                var rented = ArrayPool<byte>.Shared.Rent(binary.Length + 64);
                try
                {
                    rented.AsSpan().Fill(0xC1);
                    binary.CopyTo(rented, 32);
                    var fromRented = new MemoryDatabase(rented.AsSpan(32, binary.Length));
                    Equal("Dragon", fromRented.MonsterTable.FindByMonsterId(77).Name, "a span of a rented buffer");
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            });

            checks.Run("options: one object for the database and the builder, interning whatever the factory", () =>
            {
                // MessagePack's chain for Native AOT, given by the application: the formatter of each table array is
                // in MessagePack's registry, where the generated code put it, and is found through the chain
                var options = new MemoryDatabaseOptions { FormatterFactory = MessagePackFormatterFactory.DefaultAot };
                Equal("Dragon", new MemoryDatabase(binary, options).MonsterTable.FindByMonsterId(77).Name, "DefaultAot");
                Equal(5, new MemoryDatabase(new ReadOnlyMemory<byte>(binary), options, maxDegreeOfParallelism: 4).ItemTable.Count, "memory, in parallel");
                Equal(3, new MemoryDatabase(new ReadOnlySpan<byte>(binary), options).StageTable.Count, "span");
                Equal(5, new MemoryDatabase(new DatabaseBuilder(options).Append(database.MonsterTable.GetRawDataUnsafe()).Build(), options).MonsterTable.Count, "written and read with the same options");

                // a table array through the serializer itself
                var monsters = MessagePackSerializer.Deserialize<Monster[]>(
                    MessagePackSerializer.Serialize(database.MonsterTable.GetRawDataUnsafe(), MessagePackSerializerOptions.DefaultAot),
                    MessagePackSerializerOptions.DefaultAot)!;
                Equal(5, monsters.Length, "a table array, serialized with MessagePack's own options");

                // interning is MasterMemory's to add, in front of whatever factory the options have
                var interned = new MemoryDatabase(binary, options).MonsterTable;
                Equal(true, ReferenceEquals(interned.FindByMonsterId(10).Note, interned.FindByMonsterId(20).Note), "interned with a factory given");
                var plain = new MemoryDatabase(binary, options with { InternString = false }).MonsterTable;
                Equal(false, ReferenceEquals(plain.FindByMonsterId(10).Note, plain.FindByMonsterId(20).Note), "not interned when InternString says so");

                // each database has its own pool, unless the options say to share it
                Equal(false, ReferenceEquals(interned.FindByMonsterId(10).Note, new MemoryDatabase(binary, options).MonsterTable.FindByMonsterId(10).Note), "a pool of its own");
                var shared = options with { ShareInternedStrings = true };
                Equal(true, ReferenceEquals(new MemoryDatabase(binary, shared).MonsterTable.FindByMonsterId(10).Note, new MemoryDatabase(binary, shared).MonsterTable.FindByMonsterId(20).Note), "a pool of the options");

                // a builder made from a database writes with the options the database was read with
                Equal(true, ReferenceEquals(options, new MemoryDatabase(binary, options).ToDatabaseBuilder().Options), "the options go on to the builder");
            });

            checks.Run("compression: plugged in with the options, and missed when it is not", () =>
            {
                var lz4 = new MemoryDatabaseOptions { MessageProcessor = new Lz4BlockProcessor() };

                // enough rows that LZ4 has something to do
                var items = Enumerable.Range(1, 2000).Select(i => new Item { ItemId = i, Name = "item of the same long name", Price = i % 10 }).ToArray();
                var plainBinary = new DatabaseBuilder().Append(items).Build();
                var compressedBinary = new DatabaseBuilder(lz4).Append(items).Build();
                Equal(true, compressedBinary.Length < plainBinary.Length / 2, $"compressed {compressedBinary.Length} bytes, plain {plainBinary.Length} bytes");

                Equal(2000, new MemoryDatabase(compressedBinary, lz4).ItemTable.Count, "written and read with the processor");
                Equal(2000, new MemoryDatabase(plainBinary, lz4).ItemTable.Count, "written without, read with");
                Equal("item of the same long name", new MemoryDatabase(new ReadOnlySpan<byte>(compressedBinary), lz4).ItemTable.FindByItemId(1999).Name, "from a span");

                // without the processor the database says what it is missing
                try
                {
                    new MemoryDatabase(compressedBinary);
                    throw new CheckFailedException("a compressed database was read without a processor");
                }
                catch (InvalidOperationException ex)
                {
                    Contains(ex.Message, "is compressed with LZ4", "what is wrong");
                    Contains(ex.Message, "Lz4BlockProcessor", "what to pass");
                }

                Contains(MemoryDatabase.GetTableInfo(compressedBinary).Single().DumpAsJson(lz4), "item of the same long name", "a compressed table as JSON");
            });

            checks.Run("lookup: primary key with gaps", () =>
            {
                var table = database.MonsterTable;
                Equal("Salamander", table.FindByMonsterId(35).Name, "FindBy");
                Equal(true, table.TryFindByMonsterId(40, out var harpy) && harpy!.Name == "Harpy", "TryFindBy of a key that is there");
                Equal(false, table.TryFindByMonsterId(36, out _), "TryFindBy of a key that is not");
                Equal(35, table.FindClosestByMonsterId(36, selectLower: true)!.MonsterId, "FindClosestBy, lower");
                Equal(40, table.FindClosestByMonsterId(36, selectLower: false)!.MonsterId, "FindClosestBy, upper");
                Sequence(new[] { 20, 35, 40 }, Ids(table.FindRangeByMonsterId(20, 40)), "FindRangeBy");
                Throws<KeyNotFoundException>(() => table.FindByMonsterId(36), "FindBy of a key that is not there");
            });

            checks.Run("lookup: primary key without a gap", () =>
            {
                var table = database.ItemTable;
                Equal("Potion", table.FindByItemId(1).Name, "the first");
                Equal("Elixir", table.FindByItemId(5).Name, "the last");
                Equal(false, table.TryFindByItemId(6, out _), "after the last");
                Equal(false, table.TryFindByItemId(0, out _), "before the first");
                Equal(5, table.AllReverse.First.ItemId, "AllReverse");
            });

            checks.Run("lookup: string keys", () =>
            {
                Equal(77, database.MonsterTable.FindByName("dragon").MonsterId, "a key that ignores case");
                Equal("Hello", database.MessageTable.FindByKey("greeting").Text, "a string primary key");
                Equal(false, database.MessageTable.TryFindByKey("Greeting", out _), "a string primary key is ordinal");
            });

            checks.Run("lookup: enum key and composite keys", () =>
            {
                var table = database.MonsterTable;
                Sequence(new[] { 20, 35, 77 }, Ids(table.FindByElement(Element.Fire)), "rows of one enum key, in the order of the primary key");
                Equal(0, table.FindByElement((Element)99).Count, "an enum key that no row has");
                Sequence(new[] { 35, 40 }, Ids(table.FindByRankAndLevel(2, 5)), "a key of two members");
                Sequence(new[] { 10, 20 }, Ids(table.FindByRank(1)), "the head of that key");

                Equal("Gate", database.StageTable.FindByChapterAndNumber(2, 1).Title, "a primary key of two members");
                Equal(2, database.StageTable.FindByChapter(1).Count, "the head of the primary key");
            });

            checks.Run("views: foreach, span, reverse, list and LINQ", () =>
            {
                var fire = database.MonsterTable.FindByElement(Element.Fire);

                var hp = 0;
                foreach (var monster in fire) hp += monster.MaxHp;
                Equal(610, hp, "foreach");

                var span = fire.AsSpan();
                Equal(3, span.Length, "AsSpan");
                Equal(77, span[2].MonsterId, "the last of the span");

                Sequence(new[] { 77, 35, 20 }, Ids(fire.Reverse), "Reverse");
                Equal(20, fire.FirstOrDefault()!.MonsterId, "FirstOrDefault");
                Equal(3, fire.ToArray().Length, "ToArray");

                var names = fire.AsReadOnlyList().Where(x => x.MaxHp > 50).Select(x => x.Name).ToArray();
                Sequence(new[] { "Salamander", "Dragon" }, names, "LINQ over AsReadOnlyList");
                Equal(77, fire.Reverse.AsReadOnlyList().OrderByDescending(x => x.Level).First().MonsterId, "LINQ over the reverse");
                Equal(5, database.MonsterTable.SortByName.AsReadOnlyList().Count(), "Count() of a sorted view");
            });

            checks.Run("strings are interned when asked", () =>
            {
                var interned = new MemoryDatabase(binary).MonsterTable;
                Equal(true, ReferenceEquals(interned.FindByMonsterId(10).Note, interned.FindByMonsterId(20).Note), "the same string in two rows is one object");
            });

            checks.Run("builder: change a loaded database", () =>
            {
                var changed = database.ToDatabaseBuilder()
                    .AddOrReplace(new[]
                    {
                        new Monster { MonsterId = 20, Name = "Hobgoblin", Element = Element.Fire, Rank = 2, Level = 4, MaxHp = 45, DropItemId = 2 },
                        new Monster { MonsterId = 99, Name = "Phoenix", Element = Element.Fire, Rank = 3, Level = 8, MaxHp = 300, DropItemId = 5 },
                    })
                    .RemoveMonster(10)
                    .ToMemoryDatabase();

                Sequence(new[] { 20, 35, 40, 77, 99 }, Ids(changed.MonsterTable.All), "the rows after the change");
                Equal("Hobgoblin", changed.MonsterTable.FindByMonsterId(20).Name, "the row that was replaced");
                Sequence(new[] { 20, 35, 77, 99 }, Ids(changed.MonsterTable.FindByElement(Element.Fire)), "an index of the changed table");
                Equal(5, changed.ItemTable.Count, "a table that was not changed");

                Equal("Goblin", database.MonsterTable.FindByMonsterId(20).Name, "the database it was made from is as it was");

                // through the binary and back
                var reloaded = new MemoryDatabase(changed.ToDatabaseBuilder().Build());
                Sequence(new[] { 20, 35, 40, 77, 99 }, Ids(reloaded.MonsterTable.All), "the changed database, written and read");

                using var stream = new MemoryStream();
                CreateBuilder().WriteToStream(stream);
                Sequence(binary, stream.ToArray(), "WriteToStream writes what Build returns");
            });

            checks.Run("validate: a database that is right", () =>
            {
                var result = database.Validate();
                Equal(false, result.IsValidationFailed, "no failure: " + result.FormatFailedResults());
            });

            checks.Run("validate: every kind of failure is found and described", () =>
            {
                var wrong = new DatabaseBuilder()
                    .Append(new[]
                    {
                        new Item { ItemId = 1, Name = "Potion", Price = 10 },
                        new Item { ItemId = 3, Name = "Ether", Price = 30 }, // not sequential
                    })
                    .Append(new[]
                    {
                        new Monster { MonsterId = 1, Name = "Ghost", Element = Element.Wind, Rank = 1, Level = 3, MaxHp = 0, DropItemId = 1 },   // no hit points
                        new Monster { MonsterId = 2, Name = "Ghost", Element = Element.Wind, Rank = 1, Level = 3, MaxHp = 5, DropItemId = 42 },  // same name and level, an item that is not there
                        new Monster { MonsterId = 3, Name = "", Element = Element.Fire, Rank = 12, Level = 0, MaxHp = 5, DropItemId = 1 },        // no name, rank and level out of range
                    })
                    .ToMemoryDatabase();

                var result = wrong.Validate();
                var messages = result.FailedResults.Select(x => x.Message).ToArray();
                var all = string.Join("\n", messages);

                Equal(true, result.IsValidationFailed, "failed");
                Contains(all, "MaxHp = 0", "the expression that failed, with the value of the member it read");
                Contains(all, "PK(MonsterId) = 1", "the primary key of the row");
                Contains(all, "the level starts at 1", "a delegate with a message");
                Contains(all, "a monster has a name", "an action with a message");
                Contains(all, "Rank", "an action that is an expression");
                Contains(all, "DropItemId", "a reference to a row that is not there");
                Contains(all, "Unique failed", "a value that two rows share");
                Contains(all, "one wind monster per level", "Unique over a part of the table");
                Contains(all, "Sequential", "a gap in a sequence");
            });

            checks.Run("metadata: tables, properties and indexes", () =>
            {
                var meta = MemoryDatabase.GetMetaDatabase();
                Equal(4, meta.Count, "tables");

                var monster = meta.GetTableInfo("monster")!;
                Equal(typeof(Monster), monster.DataType, "the row type");
                Sequence(
                    new[] { "MonsterId", "Name", "Element", "Rank", "Level", "MaxHp", "DropItemId", "Note" },
                    monster.Properties.Select(x => x.Name),
                    "properties");
                Equal("drop_item_id", monster.Properties[6].NameSnakeCase, "a property name in snake case");
                Equal(typeof(Element), monster.Properties[2].PropertyInfo.PropertyType, "the PropertyInfo of a property");
                Equal(4, monster.Indexes.Count, "indexes");
                Equal(true, monster.Indexes[0].IsPrimaryIndex && monster.Indexes[0].IsUnique, "the first is the primary key");
                Sequence(new[] { "Rank", "Level" }, monster.Indexes.Single(x => x.IndexProperties.Count == 2).IndexProperties.Select(x => x.Name), "the index of two members");
            });

            checks.Run("importer: rows made from the metadata, appended by their type", () =>
            {
                // what a tool that reads CSV does: a row type it knows only as a Type, rows made without their
                // constructor, properties set by PropertyInfo, and the rows handed over as objects
                var table = MemoryDatabase.GetMetaDatabase().GetTableInfo("item")!;
                var source = new[]
                {
                    new Dictionary<string, string> { ["item_id"] = "1", ["name"] = "Herb", ["price"] = "5" },
                    new Dictionary<string, string> { ["item_id"] = "2", ["name"] = "Antidote", ["price"] = "8" },
                };

                var rows = new List<object>();
                foreach (var values in source)
                {
                    var row = RuntimeHelpers.GetUninitializedObject(table.DataType);
                    foreach (var property in table.Properties)
                    {
                        property.PropertyInfo.SetValue(row, Parse(property.PropertyInfo.PropertyType, values[property.NameSnakeCase]));
                    }
                    rows.Add(row);
                }

                var builder = new DatabaseBuilder();
                builder.AppendDynamic(table.DataType, rows);
                var imported = new MemoryDatabase(builder.Build());

                Equal(2, imported.ItemTable.Count, "rows");
                Equal("Antidote", imported.ItemTable.FindByItemId(2).Name, "a string property");
                Equal(8L, imported.ItemTable.FindByItemId(2).Price, "a long property");

                Throws<InvalidOperationException>(() => new DatabaseBuilder().AppendDynamic(typeof(string), new List<object>()), "a type that is no table");
            });

            checks.Run("table info: the layout of a binary, and a table as JSON", () =>
            {
                var tables = MemoryDatabase.GetTableInfo(binary);
                Sequence(new[] { "item", "message", "monster", "stage" }, tables.Select(x => x.TableName).OrderBy(x => x, StringComparer.Ordinal), "tables");
                Contains(tables.Single(x => x.TableName == "monster").DumpAsJson(), "Salamander", "a table dumped as JSON");

                var fromSpan = MemoryDatabase.GetTableInfo(new ReadOnlySpan<byte>(binary), storeTableData: false);
                Equal(4, fromSpan.Length, "from a span");
            });

            checks.Run("a binary that is no database is rejected", () =>
            {
                Throws<InvalidOperationException>(() => new MemoryDatabase(new byte[] { 0x92, 0x01, 0x02 }), "an array, not a header");
                Throws<InvalidOperationException>(() => new MemoryDatabase(binary.AsSpan(0, 10)), "a database cut short");
                Throws<ArgumentNullException>(() => new MemoryDatabase((byte[])null!), "null");
            });

            output.WriteLine($"{checks.Passed} passed, {checks.Failed} failed");
            return checks.Failed;
        }

        static DatabaseBuilder CreateBuilder()
        {
            return new DatabaseBuilder()
                .Append(new[]
                {
                    new Monster { MonsterId = 10, Name = "Slime", Element = Element.Water, Rank = 1, Level = 1, MaxHp = 10, DropItemId = 1, Note = "common" },
                    new Monster { MonsterId = 20, Name = "Goblin", Element = Element.Fire, Rank = 1, Level = 2, MaxHp = 30, DropItemId = 2, Note = "common" },
                    new Monster { MonsterId = 35, Name = "Salamander", Element = Element.Fire, Rank = 2, Level = 5, MaxHp = 80, DropItemId = 3 },
                    new Monster { MonsterId = 40, Name = "Harpy", Element = Element.Wind, Rank = 2, Level = 5, MaxHp = 60, DropItemId = 2 },
                    new Monster { MonsterId = 77, Name = "Dragon", Element = Element.Fire, Rank = 3, Level = 9, MaxHp = 500, DropItemId = 5, Note = "boss" },
                })
                .Append(new[]
                {
                    new Item { ItemId = 1, Name = "Potion", Price = 10 },
                    new Item { ItemId = 2, Name = "Hi-Potion", Price = 50 },
                    new Item { ItemId = 3, Name = "Ether", Price = 120 },
                    new Item { ItemId = 4, Name = "Phoenix Down", Price = 300 },
                    new Item { ItemId = 5, Name = "Elixir", Price = 5_000_000_000 },
                })
                .Append(new[]
                {
                    new Stage { Chapter = 1, Number = 1, Title = "Meadow" },
                    new Stage { Chapter = 1, Number = 2, Title = "Forest" },
                    new Stage { Chapter = 2, Number = 1, Title = "Gate" },
                })
                .Append(new[]
                {
                    new Message { Key = "greeting", Text = "Hello" },
                    new Message { Key = "farewell", Text = "Goodbye" },
                });
        }

        static object Parse(Type type, string value)
        {
            if (type == typeof(string)) return value;
            if (type == typeof(int)) return int.Parse(value);
            if (type == typeof(long)) return long.Parse(value);
            throw new NotSupportedException("no parser for " + type);
        }

        static int[] Ids(RangeView<Monster> view)
        {
            var ids = new int[view.Count];
            for (var i = 0; i < ids.Length; i++) ids[i] = view[i].MonsterId;
            return ids;
        }

        static int[] Ids(ReverseRangeView<Monster> view)
        {
            var ids = new int[view.Count];
            for (var i = 0; i < ids.Length; i++) ids[i] = view[i].MonsterId;
            return ids;
        }

        // ---- the checks themselves

        sealed class CheckFailedException(string message) : Exception(message);

        sealed class CheckRunner(TextWriter output)
        {
            public int Passed { get; private set; }
            public int Failed { get; private set; }

            public void Run(string name, Action check)
            {
                try
                {
                    check();
                    Passed++;
                    output.WriteLine("ok      " + name);
                }
                catch (Exception ex)
                {
                    Failed++;
                    output.WriteLine("FAILED  " + name);
                    output.WriteLine("        " + ex.GetType().FullName + ": " + ex.Message);
                }
            }
        }

        static void Equal<T>(T expected, T actual, string what)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new CheckFailedException($"{what}: expected {expected}, was {actual}");
            }
        }

        static void Sequence<T>(IEnumerable<T> expected, IEnumerable<T> actual, string what)
        {
            var e = expected.ToArray();
            var a = actual.ToArray();
            if (!e.AsSpan().SequenceEqual(a))
            {
                throw new CheckFailedException($"{what}: expected [{string.Join(", ", e)}], was [{string.Join(", ", a)}]");
            }
        }

        static void Contains(string text, string part, string what)
        {
            if (!text.Contains(part, StringComparison.Ordinal))
            {
                throw new CheckFailedException($"{what}: \"{part}\" is not in:\n{text}");
            }
        }

        static void Throws<TException>(Action action, string what)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            catch (Exception ex)
            {
                throw new CheckFailedException($"{what}: expected {typeof(TException).Name}, was {ex.GetType().Name}: {ex.Message}");
            }
            throw new CheckFailedException($"{what}: expected {typeof(TException).Name}, and nothing was thrown");
        }
    }

    static class Program
    {
        static int Main()
        {
            Console.WriteLine(IsNativeAot()
                ? "MasterMemory checks, compiled ahead of time (Native AOT)"
                : "MasterMemory checks, on a runtime with a JIT");
            return AotChecks.Run(Console.Out);
        }

        // An assembly that was compiled into a native executable has no file of its own. That is what IL3000 warns of,
        // and exactly what is asked here. (RuntimeFeature.IsDynamicCodeSupported does not tell the two apart: a project
        // with PublishAot switches it off for its runs on the JIT too, so that they behave like the native build.)
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("SingleFile", "IL3000", Justification = "An empty location is the answer that is wanted.")]
        static bool IsNativeAot()
        {
            return typeof(Program).Assembly.Location.Length == 0;
        }
    }
}
