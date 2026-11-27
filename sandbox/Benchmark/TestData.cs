using System;
using System.Text;
using MasterMemory;
using MessagePack;

// the benchmarks read neither the metadata nor the validation of the database
[assembly: MasterMemoryGeneratorOptions(Namespace = "Benchmark", GenerateMetaDatabase = false, GenerateValidator = false)]

namespace Benchmark
{
    [MemoryTable("TestDoc"), MessagePackObject(true)]
    public class TestDoc
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lorem { get; set; }
    }

    /// <summary>
    /// The rows every engine is given. They come from a fixed seed: the same call returns the same rows,
    /// in every engine, every process and every run.
    /// </summary>
    public static class TestData
    {
        const int RowSeed = 12345;
        const int QuerySeed = 67890;

        static readonly string[] Words =
        {
            "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
            "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
            "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"
        };

        /// <summary>
        /// Rows in the order of their id. With <paramref name="dense"/> the ids are 1, 2, 3, ...; without, each id is
        /// 1 to 9 after the one before, so the ids have gaps. Name and Lorem are the same either way.
        /// </summary>
        public static TestDoc[] Create(int count, bool dense = false)
        {
            var random = new Random(RowSeed);
            var rows = new TestDoc[count];
            var guid = new byte[16];
            var builder = new StringBuilder();
            var id = 0;

            for (var i = 0; i < rows.Length; i++)
            {
                var gap = random.Next(1, 10);
                id += dense ? 1 : gap;

                random.NextBytes(guid);
                rows[i] = new TestDoc
                {
                    Id = id,
                    Name = new Guid(guid).ToString(),
                    Lorem = LoremIpsum(random, builder)
                };
            }

            return rows;
        }

        /// <summary>Ids of rows that exist, picked at random (with a seed of their own) and in no order.</summary>
        public static int[] CreateQueryKeys(TestDoc[] rows, int count)
        {
            var random = new Random(QuerySeed);
            var keys = new int[count];
            for (var i = 0; i < keys.Length; i++)
            {
                keys[i] = rows[random.Next(rows.Length)].Id;
            }
            return keys;
        }

        /// <summary>
        /// Asks <paramref name="find"/> for rows spread over the whole table, the first and the last among them, and
        /// throws unless each comes back with the id, name and lorem it was built with.
        /// </summary>
        public static void Verify(TestDoc[] rows, Func<int, TestDoc> find)
        {
            var step = Math.Max(1, rows.Length / 64);
            for (var i = 0; i < rows.Length; i += step)
            {
                VerifyRow(rows[i], find(rows[i].Id));
            }
            VerifyRow(rows[rows.Length - 1], find(rows[rows.Length - 1].Id));
        }

        public static void VerifyRow(TestDoc expected, TestDoc actual)
        {
            if (actual == null) throw new InvalidOperationException($"The row of id {expected.Id} came back as null");
            if (actual.Id != expected.Id) throw new InvalidOperationException($"The row of id {expected.Id} came back with id {actual.Id}");
            if (actual.Name != expected.Name) throw new InvalidOperationException($"The row of id {expected.Id} came back with name '{actual.Name}', not '{expected.Name}'");
            if (actual.Lorem != expected.Lorem) throw new InvalidOperationException($"The row of id {expected.Id} came back with a lorem of {actual.Lorem?.Length} characters, not the {expected.Lorem.Length} it has");
        }

        // Three paragraphs of three or four sentences of four to six words, about 300 characters.
        // No whitespace at either end: LiteDB trims the strings it stores, and every engine has to give back the same row.
        static string LoremIpsum(Random random, StringBuilder builder)
        {
            builder.Clear();
            for (var paragraph = 0; paragraph < 3; paragraph++)
            {
                if (paragraph > 0) builder.Append('\n');

                var sentences = random.Next(3, 5);
                for (var sentence = 0; sentence < sentences; sentence++)
                {
                    if (sentence > 0) builder.Append(' ');

                    var words = random.Next(4, 7);
                    for (var word = 0; word < words; word++)
                    {
                        if (word > 0) builder.Append(' ');
                        builder.Append(Words[random.Next(Words.Length)]);
                    }
                    builder.Append('.');
                }
            }
            return builder.ToString();
        }
    }
}
