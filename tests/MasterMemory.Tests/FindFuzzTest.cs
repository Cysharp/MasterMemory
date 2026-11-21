using MasterMemory.Tests.Fuzz.Target;
using System;
using System.Collections.Generic;
using Xunit;

namespace MasterMemory.Tests
{
    // The fuzz targets of the lookups (tests/MasterMemory.Tests.Fuzz.Target), run as an ordinary test: every seed, and
    // inputs from a fixed random sequence. Each target builds a table from its input, asks every kind of lookup, and
    // throws when an answer differs from a linear pass over the rows.
    // This is the blind tier, on every test run. The coverage-guided tier is tests/MasterMemory.Tests.Fuzz/fuzz.ps1.
    public class FindFuzzTest
    {
        public static IEnumerable<object[]> TargetNames()
        {
            foreach (var target in FindFuzzTargets.Targets)
            {
                yield return new object[] { target.Name };
            }
        }

        static Action<byte[]> Find(string name)
        {
            foreach (var target in FindFuzzTargets.Targets)
            {
                if (target.Name == name) return target.Run;
            }
            throw new ArgumentException(name);
        }

        static void Run(Action<byte[]> run, string name, byte[] input)
        {
            try
            {
                run(input);
            }
            catch (Exception ex)
            {
                throw new Xunit.Sdk.XunitException($"{name} failed on input {Convert.ToHexString(input)}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        [Theory]
        [MemberData(nameof(TargetNames))]
        public void Seeds(string name)
        {
            var run = Find(name);
            foreach (var seed in FindFuzzTargets.SeedCorpus(name))
            {
                Run(run, name, seed);
            }
        }

        // The target that loads its input as a database, on databases damaged the ways a file gets damaged: cut short,
        // a bit flipped, bytes overwritten, a length field set to an extreme, bytes missing or added in the middle.
        // Random bytes alone seldom get past the header; these reach the tables.
        [Fact]
        public void DamagedDatabases()
        {
            var random = new Random(20261121);
            foreach (var seed in DatabaseFuzzTargets.SeedCorpus())
            {
                for (var i = 0; i < 4000; i++)
                {
                    var input = Damage(seed, random);
                    if (i % 5 == 0) input = Damage(input, random);
                    Run(DatabaseFuzzTargets.Load, DatabaseFuzzTargets.Name, input);
                }
            }
        }

        static readonly int[] Extremes = { 0, 1, -1, int.MaxValue, int.MinValue, 0x7FFFFFF0, 0x00010000, 255, 65536 };

        static byte[] Damage(byte[] source, Random random)
        {
            var data = (byte[])source.Clone();
            if (data.Length == 0) return data;

            switch (random.Next(6))
            {
                case 0: // cut short
                    Array.Resize(ref data, random.Next(0, data.Length));
                    break;
                case 1: // one bit
                    data[random.Next(data.Length)] ^= (byte)(1 << random.Next(8));
                    break;
                case 2: // a few bytes
                    for (var n = random.Next(1, 4); n > 0; n--) data[random.Next(data.Length)] = (byte)random.Next(256);
                    break;
                case 3: // four bytes that may be a length, an offset or a count: an extreme, in the byte order MessagePack writes
                    if (data.Length >= 4)
                    {
                        var at = random.Next(data.Length - 3);
                        var value = Extremes[random.Next(Extremes.Length)];
                        data[at] = (byte)(value >> 24); data[at + 1] = (byte)(value >> 16); data[at + 2] = (byte)(value >> 8); data[at + 3] = (byte)value;
                    }
                    break;
                case 4: // bytes missing from the middle
                    {
                        var at = random.Next(data.Length);
                        var length = Math.Min(random.Next(1, 9), data.Length - at);
                        var result = new byte[data.Length - length];
                        Array.Copy(data, 0, result, 0, at);
                        Array.Copy(data, at + length, result, at, data.Length - at - length);
                        data = result;
                    }
                    break;
                default: // bytes added in the middle
                    {
                        var at = random.Next(data.Length + 1);
                        var added = new byte[random.Next(1, 9)];
                        random.NextBytes(added);
                        var result = new byte[data.Length + added.Length];
                        Array.Copy(data, 0, result, 0, at);
                        Array.Copy(added, 0, result, at, added.Length);
                        Array.Copy(data, at, result, at + added.Length, data.Length - at);
                        data = result;
                    }
                    break;
            }
            return data;
        }

        [Theory]
        [MemberData(nameof(TargetNames))]
        public void RandomInputs(string name)
        {
            var run = Find(name);
            var random = new Random(20261121);

            for (var i = 0; i < 3000; i++)
            {
                // short inputs (tables of a few rows) as often as long ones
                var input = new byte[random.Next(0, (i % 2 == 0) ? 24 : 330)];
                random.NextBytes(input);

                // half of them with every id one above the last: the table the primary key reads without searching
                if (i % 4 < 2)
                {
                    for (var b = 2; b < input.Length; b += 4) input[b] &= 0xFC;
                }

                Run(run, name, input);
            }
        }
    }
}
