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
            foreach (var seed in FindFuzzTargets.SeedCorpus())
            {
                Run(run, name, seed);
            }
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
