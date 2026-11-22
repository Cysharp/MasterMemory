using MasterMemory.Internal;
using MasterMemory.Tests.TestStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    // DatabaseBuilder holds tables as rows: rows go in (Append), a table that is there is changed (ReplaceAll,
    // AddOrReplace, Remove***), and what comes out is the database binary (Build) or the database itself
    // (ToMemoryDatabase). A builder made from a database (ToDatabaseBuilder) starts with the tables of that database.
    public class DatabaseBuilderTest
    {
        static int serial;

        // a row that no other row is equal to, whatever its key
        static Sample NewSample(int id)
        {
            var n = ++serial;
            return new Sample(id, n % 90, "first" + n, "last" + n);
        }

        static string[] Rows(MemoryDatabase database) => database.SampleTable.All.Select(x => x.ToString()).ToArray();

        // ---- out: the database, and the binary ----

        [Fact]
        public void ADatabaseStraightFromRows()
        {
            var builder = GoldenData.CreateBuilder();
            var direct = builder.ToMemoryDatabase();
            var loaded = new MemoryDatabase(builder.Build(), internString: false);

            // table by table, what the binary holds: the same rows in the same order
            var fromDirect = MemoryDatabase.GetTableInfo(direct.ToDatabaseBuilder().Build()).ToDictionary(x => x.TableName, x => x.DumpAsJson());
            var fromLoaded = MemoryDatabase.GetTableInfo(loaded.ToDatabaseBuilder().Build()).ToDictionary(x => x.TableName, x => x.DumpAsJson());
            fromDirect.Keys.OrderBy(x => x).ToArray().ShouldBe(fromLoaded.Keys.OrderBy(x => x).ToArray());
            foreach (var item in fromLoaded)
            {
                fromDirect[item.Key].ShouldBe(item.Value, "table: " + item.Key);
            }

            // and the indexes are there
            direct.SampleTable.FindById(8).Age.ShouldBe(49);
            direct.SampleTable.FindByAge(89).Select(x => x.Id).OrderBy(x => x).ToArray().ShouldBe(new[] { 2, 4 });
            direct.SampleTable.SortByAge.Select(x => x.Age).ToArray().ShouldBe(loaded.SampleTable.SortByAge.Select(x => x.Age).ToArray());
            direct.SkillMasterTable.FindBySkillIdAndSkillLevel(2, 2).AttackPower.ShouldBe(30);
            direct.PersonModelTable.All.Select(x => x.RandomId).ToArray().ShouldBe(loaded.PersonModelTable.All.Select(x => x.RandomId).ToArray());

            // the rows are the ones that were handed in, not copies of them
            var rows = GoldenData.Samples();
            var table = new DatabaseBuilder().Append(rows).ToMemoryDatabase().SampleTable;
            table.FindById(7).ShouldBeSameAs(rows.Single(x => x.Id == 7));
        }

        [Fact]
        public void TheBinary()
        {
            var builder = GoldenData.CreateBuilder();
            var binary = builder.Build();

            // again, and to a stream: the same bytes
            builder.Build().ShouldBe(binary);
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(1); // somewhere in the stream, not at its start
                builder.WriteToStream(stream);
                stream.ToArray().Skip(1).ToArray().ShouldBe(binary);
            }

            // making a database in between changes nothing about it
            builder.ToMemoryDatabase();
            builder.Build().ShouldBe(binary);
        }

        [Fact]
        public void TablesAreWrittenInTheOrderTheyWereSetFirst()
        {
            var builder = new DatabaseBuilder()
                .Append(new[] { new UserLevel(1, 100) })
                .Append(GoldenData.Samples())
                .Append(GoldenData.Skills());
            DatabaseHeader.Read(builder.Build(), out _).Keys.ToArray().ShouldBe(new[] { "UserLevel", "s_a_m_p_l_e", "skillmaster" });

            // a change to a table leaves it where it is
            builder.ReplaceAll(new[] { new UserLevel(2, 200) }).RemoveSample(5);
            DatabaseHeader.Read(builder.Build(), out _).Keys.ToArray().ShouldBe(new[] { "UserLevel", "s_a_m_p_l_e", "skillmaster" });
        }

        [Fact]
        public void ATableThatWasNeverSet()
        {
            var builder = new DatabaseBuilder().Append(GoldenData.Samples());

            // changing nothing in a table that is not there does not put it there
            builder.AddOrReplace(Array.Empty<UserLevel>()).RemoveUserLevel(1, 2, 3);

            MemoryDatabase.GetTableInfo(builder.Build()).Select(x => x.TableName).ToArray().ShouldBe(new[] { "s_a_m_p_l_e" });
            builder.ToMemoryDatabase().UserLevelTable.Count.ShouldBe(0);

            // rows do
            builder.AddOrReplace(new[] { new UserLevel(3, 300) });
            MemoryDatabase.GetTableInfo(builder.Build()).Select(x => x.TableName).ToArray().ShouldBe(new[] { "s_a_m_p_l_e", "UserLevel" });
            builder.ToMemoryDatabase().UserLevelTable.FindByLevel(3).Exp.ShouldBe(300);

            // and so does a table set to no rows, as before
            var empty = new DatabaseBuilder().Append(Array.Empty<UserLevel>());
            MemoryDatabase.GetTableInfo(empty.Build()).Select(x => x.TableName).ToArray().ShouldBe(new[] { "UserLevel" });
        }

        // ---- in: Append ----

        [Fact]
        public void AppendIsForATableThatIsNotThereYet()
        {
            var builder = new DatabaseBuilder().Append(GoldenData.Samples());
            Should.Throw<InvalidOperationException>(() => builder.Append(GoldenData.Samples())).Message.ShouldContain("s_a_m_p_l_e");

            // a builder made from a database has every table
            var fromDatabase = builder.ToMemoryDatabase().ToDatabaseBuilder();
            Should.Throw<InvalidOperationException>(() => fromDatabase.Append(GoldenData.Samples()));
            Should.Throw<InvalidOperationException>(() => fromDatabase.Append(new[] { new UserLevel(1, 100) })).Message.ShouldContain("ReplaceAll");

            // no rows at all is not a table, as before
            new DatabaseBuilder().Append((IEnumerable<Sample>)null).Append(GoldenData.Samples()).ToMemoryDatabase().SampleTable.Count.ShouldBe(11);
        }

        [Fact]
        public void WhatIsNotARow()
        {
            var builder = new DatabaseBuilder();
            var withNull = new[] { NewSample(1), null, NewSample(3) };

            Should.Throw<ArgumentException>(() => builder.Append(withNull)).Message.ShouldContain("Index:1");
            Should.Throw<ArgumentException>(() => builder.ReplaceAll(withNull));
            Should.Throw<ArgumentException>(() => builder.AddOrReplace(withNull));

            Should.Throw<ArgumentNullException>(() => builder.ReplaceAll((IEnumerable<Sample>)null));
            Should.Throw<ArgumentNullException>(() => builder.AddOrReplace((IEnumerable<Sample>)null));
            Should.Throw<ArgumentNullException>(() => builder.RemoveSample(null));

            // nothing of it was taken
            builder.ToMemoryDatabase().SampleTable.Count.ShouldBe(0);
            MemoryDatabase.GetTableInfo(builder.Build()).ShouldBeEmpty();
        }

        // ---- changes ----

        [Fact]
        public void ReplaceAll()
        {
            var builder = new DatabaseBuilder().Append(GoldenData.Samples());
            var rows = new[] { NewSample(30), NewSample(10), NewSample(20) };

            Rows(builder.ReplaceAll(rows).ToMemoryDatabase()).ShouldBe(new[] { rows[1].ToString(), rows[2].ToString(), rows[0].ToString() });
            builder.ReplaceAll(Array.Empty<Sample>()).ToMemoryDatabase().SampleTable.Count.ShouldBe(0);

            // of a table that is not there: sets it
            new DatabaseBuilder().ReplaceAll(rows.ToList()).ToMemoryDatabase().SampleTable.Count.ShouldBe(3);

            // any sequence, read once
            var enumerated = 0;
            IEnumerable<Sample> Lazy() { enumerated++; yield return rows[0]; yield return rows[1]; }
            builder.ReplaceAll(Lazy()).ToMemoryDatabase().SampleTable.All.Select(x => x.Id).ToArray().ShouldBe(new[] { 10, 30 });
            enumerated.ShouldBe(1);
        }

        [Fact]
        public void AddOrReplace()
        {
            var first = new[] { NewSample(1), NewSample(3), NewSample(5) };
            var builder = new DatabaseBuilder().Append(first);

            // before, between and after the rows that are there, and in place of one
            var changes = new[] { NewSample(6), NewSample(0), NewSample(3), NewSample(2) };
            Rows(builder.AddOrReplace(changes).ToMemoryDatabase()).ShouldBe(new[] { changes[1], first[0], changes[3], changes[2], first[2], changes[0] }.Select(x => x.ToString()).ToArray());

            // every row replaced
            var all = new[] { NewSample(6), NewSample(5), NewSample(3), NewSample(2), NewSample(1), NewSample(0) };
            Rows(builder.AddOrReplace(all).ToMemoryDatabase()).ShouldBe(all.Reverse().Select(x => x.ToString()).ToArray());

            // nothing
            Rows(builder.AddOrReplace(Array.Empty<Sample>()).ToMemoryDatabase()).ShouldBe(all.Reverse().Select(x => x.ToString()).ToArray());
        }

        // Rows are taken as if they had been given one by one: of two with one key, the later one stays. Before, two
        // rows for a key that was there threw an ArgumentException from inside, and two rows for a new key were both
        // added, to a key that is unique.
        [Fact]
        public void AddOrReplaceWithOneKeyTwice()
        {
            var builder = new DatabaseBuilder().Append(new[] { NewSample(1), NewSample(2) });

            var rows = new[] { NewSample(2), NewSample(9), NewSample(2), NewSample(9), NewSample(9), NewSample(2), NewSample(4) };
            var table = builder.AddOrReplace(rows).ToMemoryDatabase().SampleTable;

            table.All.Select(x => x.Id).ToArray().ShouldBe(new[] { 1, 2, 4, 9 });
            table.FindById(2).ShouldBeSameAs(rows[5]);
            table.FindById(9).ShouldBeSameAs(rows[4]);

            // many of one key, in an order the sort is free to shuffle
            var many = Enumerable.Range(0, 500).Select(i => NewSample(i % 7)).ToArray();
            var after = new DatabaseBuilder().AddOrReplace(many).ToMemoryDatabase().SampleTable;
            after.Count.ShouldBe(7);
            for (var id = 0; id < 7; id++)
            {
                after.FindById(id).ShouldBeSameAs(many.Last(x => x.Id == id), "id " + id);
            }

            // a key that is in the table twice already (nothing stops that: ReplaceAll takes any rows) is one row afterwards
            var twice = new DatabaseBuilder().ReplaceAll(new[] { NewSample(1), NewSample(1), NewSample(2) });
            var one = NewSample(1);
            Rows(twice.AddOrReplace(new[] { one }).ToMemoryDatabase()).Take(1).ToArray().ShouldBe(new[] { one.ToString() });
            twice.ToMemoryDatabase().SampleTable.Count.ShouldBe(2);
        }

        [Fact]
        public void Remove()
        {
            var rows = Enumerable.Range(1, 10).Select(NewSample).ToArray();
            DatabaseBuilder Builder() => new DatabaseBuilder().Append(rows);
            int[] Ids(DatabaseBuilder builder) => builder.ToMemoryDatabase().SampleTable.All.Select(x => x.Id).ToArray();

            Ids(Builder().RemoveSample(1)).ShouldBe(Enumerable.Range(2, 9).ToArray());
            Ids(Builder().RemoveSample(10)).ShouldBe(Enumerable.Range(1, 9).ToArray());
            Ids(Builder().RemoveSample(7, 3, 5)).ShouldBe(new[] { 1, 2, 4, 6, 8, 9, 10 });
            Ids(Builder().RemoveSample(new[] { 3, 3, 3, 100, -1, 0, 11 })).ShouldBe(new[] { 1, 2, 4, 5, 6, 7, 8, 9, 10 });
            Ids(Builder().RemoveSample()).ShouldBe(Enumerable.Range(1, 10).ToArray());
            Ids(Builder().RemoveSample(Enumerable.Range(-5, 30).ToArray())).ShouldBeEmpty();
            Ids(Builder().RemoveSample(2).RemoveSample(2).RemoveSample(3)).ShouldBe(new[] { 1, 4, 5, 6, 7, 8, 9, 10 });

            // every row of a key that is in the table twice
            Ids(new DatabaseBuilder().ReplaceAll(new[] { NewSample(1), NewSample(2), NewSample(2), NewSample(3) }).RemoveSample(2)).ShouldBe(new[] { 1, 3 });

            // the keys that were handed in are left as they were
            var keys = new[] { 9, 1, 5 };
            Builder().RemoveSample(keys);
            keys.ShouldBe(new[] { 9, 1, 5 });
        }

        [Fact]
        public void CompositeKey()
        {
            var builder = new DatabaseBuilder().Append(GoldenData.Skills()); // (1,1) (1,2) (2,1) (2,2) (3,1)
            (int, int)[] Keys() => builder.ToMemoryDatabase().SkillMasterTable.All.Select(x => (x.SkillId, x.SkillLevel)).ToArray();

            builder.AddOrReplace(new[] { new SkillMaster(2, 1, 999, "changed", ""), new SkillMaster(1, 3, 5, "new", ""), new SkillMaster(0, 9, 5, "first", "") });
            Keys().ShouldBe(new[] { (0, 9), (1, 1), (1, 2), (1, 3), (2, 1), (2, 2), (3, 1) });
            builder.ToMemoryDatabase().SkillMasterTable.FindBySkillIdAndSkillLevel(2, 1).AttackPower.ShouldBe(999);

            // a composite key is given as a tuple here: what is passed is a list of keys
            builder.RemoveSkillMaster((1, 2), (3, 1), (2, 9));
            Keys().ShouldBe(new[] { (0, 9), (1, 1), (1, 3), (2, 1), (2, 2) });
            builder.RemoveSkillMaster(new[] { (0, 9) });
            Keys().ShouldBe(new[] { (1, 1), (1, 3), (2, 1), (2, 2) });
        }

        // the order is the one of the table: ordinal for a string, with null first
        [Fact]
        public void StringKey()
        {
            static V3NullableMaster Row(string code, int value) => new V3NullableMaster { Code = code, Value = value };
            var builder = new DatabaseBuilder().Append(new[] { Row("a", 1), Row("B", 2), Row("a_b", 3), Row("a1", 4) });
            string[] Codes() => builder.ToMemoryDatabase().V3NullableMasterTable.All.Select(x => x.Code ?? "(null)").ToArray();

            Codes().ShouldBe(new[] { "B", "a", "a1", "a_b" });

            builder.AddOrReplace(new[] { Row("A", 5), Row(null, 6), Row("a1", 7), Row("b", 8) });
            Codes().ShouldBe(new[] { "(null)", "A", "B", "a", "a1", "a_b", "b" });
            builder.ToMemoryDatabase().V3NullableMasterTable.FindByCode("a1").Value.ShouldBe(7);

            builder.RemoveV3NullableMaster(new string[] { "a", null, "C" });
            Codes().ShouldBe(new[] { "A", "B", "a1", "a_b", "b" });

            // a key with [StringComparisonOption]: "AB" is the row "ab"
            var option = new DatabaseBuilder().Append(new[] { new V3OptionMaster { Code = "ab", Value = 1 }, new V3OptionMaster { Code = "C", Value = 2 } });
            option.AddOrReplace(new[] { new V3OptionMaster { Code = "AB", Value = 3 } }).RemoveV3OptionMaster("c");
            option.ToMemoryDatabase().V3OptionMasterTable.All.Select(x => (x.Code, x.Value)).ToArray().ShouldBe(new[] { ("AB", 3) });
        }

        // a row cannot be addressed by a key that several rows share: such a table is set as a whole
        [Fact]
        public void PrimaryKeyThatIsNotUnique()
        {
            var builder = new DatabaseBuilder().Append(GoldenData.Tests());
            builder.ReplaceAll(new[] { new TestMaster(2, 1), new TestMaster(1, 2), new TestMaster(2, 3) });
            builder.ToMemoryDatabase().TestMasterTable.FindByTestID(2).Count.ShouldBe(2);

            var methods = typeof(DatabaseBuilder).GetMethods();
            methods.Count(x => x.Name == "ReplaceAll" && x.GetParameters()[0].ParameterType == typeof(IEnumerable<TestMaster>)).ShouldBe(1);
            methods.Count(x => x.Name == "AddOrReplace" && x.GetParameters()[0].ParameterType == typeof(IEnumerable<TestMaster>)).ShouldBe(0);
            methods.Count(x => x.Name == "RemoveTestMaster").ShouldBe(0);
            methods.Count(x => x.Name == "AddOrReplace" && x.GetParameters()[0].ParameterType == typeof(IEnumerable<Sample>)).ShouldBe(1);
            methods.Count(x => x.Name == "RemoveSample").ShouldBe(1);
        }

        // ---- what is shared, and what is never written to ----

        [Fact]
        public void TablesThatWereNotChangedAreTheTablesOfTheDatabase()
        {
            var original = new MemoryDatabase(GoldenData.CreateBuilder().Build());
            var builder = original.ToDatabaseBuilder();

            var same = builder.ToMemoryDatabase();
            same.ShouldNotBeSameAs(original);
            same.SampleTable.ShouldBeSameAs(original.SampleTable);
            same.PersonModelTable.ShouldBeSameAs(original.PersonModelTable);
            same.TestMasterTable.ShouldBeSameAs(original.TestMasterTable);

            // a change that changed nothing
            builder.RemoveSample(123456).AddOrReplace(Array.Empty<Sample>());
            builder.ToMemoryDatabase().SampleTable.ShouldBeSameAs(original.SampleTable);

            // a change: that table is another one, with the rows that stayed being the same rows
            var changed = builder.RemoveSample(5).ToMemoryDatabase();
            changed.SampleTable.ShouldNotBeSameAs(original.SampleTable);
            changed.SampleTable.FindById(6).ShouldBeSameAs(original.SampleTable.FindById(6));
            changed.SkillMasterTable.ShouldBeSameAs(original.SkillMasterTable);
            changed.PersonModelTable.ShouldBeSameAs(original.PersonModelTable);

            // and it is made once for the rows as they are
            builder.ToMemoryDatabase().SampleTable.ShouldBeSameAs(changed.SampleTable);
        }

        [Fact]
        public void NothingThatWasHandedOutOrInIsWrittenTo()
        {
            var rows = GoldenData.Samples();
            var rowsBefore = rows.ToArray();
            var builder = new DatabaseBuilder().Append(rows);
            rows.ShouldBe(rowsBefore); // not sorted in place

            var first = builder.ToMemoryDatabase();
            var held = first.SampleTable.GetRawDataUnsafe();
            var heldBefore = held.ToArray();

            builder.AddOrReplace(new[] { NewSample(7), NewSample(100) }).RemoveSample(1, 2, 3);
            var second = builder.ToMemoryDatabase();
            builder.ReplaceAll(new[] { NewSample(1) });

            held.ShouldBe(heldBefore);
            first.SampleTable.All.Select(x => x.Id).ToArray().ShouldBe(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 999 });
            first.SampleTable.FindById(7).FirstName.ShouldBe("ccc");
            second.SampleTable.All.Select(x => x.Id).ToArray().ShouldBe(new[] { 4, 5, 6, 7, 8, 9, 10, 100, 999 });
            builder.ToMemoryDatabase().SampleTable.All.Select(x => x.Id).ToArray().ShouldBe(new[] { 1 });
        }

        // ---- every change against a dictionary ----

        [Fact]
        public void RandomChangesAgainstAModel()
        {
            var random = new Random(20261122);
            for (var round = 0; round < 300; round++)
            {
                var model = new SortedDictionary<int, Sample>();
                var builder = new DatabaseBuilder();
                var earlier = new List<(MemoryDatabase Database, string[] Rows)>();

                for (var step = 0; step < 15; step++)
                {
                    var context = $"round {round}, step {step}";
                    var rows = Enumerable.Range(0, random.Next(0, 9)).Select(_ => NewSample(random.Next(0, 24))).ToArray();
                    switch (random.Next(0, 5))
                    {
                        case 0: // every row of the table; one row a key, which a dictionary cannot be otherwise
                            rows = rows.GroupBy(x => x.Id).Select(x => x.First()).ToArray();
                            builder.ReplaceAll(rows);
                            model.Clear();
                            foreach (var row in rows) model[row.Id] = row;
                            break;
                        case 1:
                        case 2:
                            builder.AddOrReplace(rows);
                            foreach (var row in rows) model[row.Id] = row;
                            break;
                        case 3:
                            var keys = Enumerable.Range(0, random.Next(0, 9)).Select(_ => random.Next(-2, 26)).ToArray();
                            builder.RemoveSample(keys);
                            foreach (var key in keys) model.Remove(key);
                            break;
                        default: // through the binary and back
                            builder = new MemoryDatabase(builder.Build(), internString: false).ToDatabaseBuilder();
                            break;
                    }

                    var database = builder.ToMemoryDatabase();
                    var expected = model.Values.Select(x => x.ToString()).ToArray();
                    Rows(database).ShouldBe(expected, context);
                    for (var id = -2; id < 26; id++)
                    {
                        database.SampleTable.TryFindById(id, out var found).ShouldBe(model.ContainsKey(id), context);
                        if (found != null) found.ToString().ShouldBe(model[id].ToString(), context);
                    }
                    database.SampleTable.SortByAge.Select(x => x.Age).ToArray().ShouldBe(model.Values.Select(x => x.Age).OrderBy(x => x).ToArray(), context);

                    earlier.Add((database, expected));
                }

                // every database made on the way is what it was when it was made
                foreach (var (database, expectedRows) in earlier)
                {
                    Rows(database).ShouldBe(expectedRows, $"round {round}");
                }
            }
        }
    }
}
