using MasterMemory.Internal;
using System;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    // BinarySearch.Find and FindBranchless read their elements without bounds checks, so they are held to the BCL's
    // own search on every length and every key, hits and misses, including the ends.
    public class ComparableSearchTest
    {
        readonly struct IntComparable : IBranchlessComparable<int>
        {
            readonly int key;
            public IntComparable(int key) => this.key = key;
            public int CompareTo(int other) => key.CompareTo(other);
            public int AtOrAfterMask(int other) => (int)(((long)other - key - 1) >> 63); // as the generated code computes it
        }

        // a comparison is only required to return a sign: the extremes must work as -1 and 1 do
        readonly struct ExtremeComparable : IBranchlessComparable<int>
        {
            readonly int key;
            public ExtremeComparable(int key) => this.key = key;
            public int CompareTo(int other) => (key < other) ? int.MinValue : (key > other) ? int.MaxValue : 0;
            public int AtOrAfterMask(int other) => (key >= other) ? -1 : 0;
        }

        readonly struct OrdinalComparable : IBranchlessComparable<string>
        {
            readonly string key;
            public OrdinalComparable(string key) => this.key = key;
            public int CompareTo(string other) => string.CompareOrdinal(key, other);
            public int AtOrAfterMask(string other) => (string.CompareOrdinal(key, other) >= 0) ? -1 : 0;
        }

        [Fact]
        public void EveryLengthAndKey()
        {
            for (var length = 0; length <= 130; length++)
            {
                var array = Enumerable.Range(0, length).Select(x => x * 2 + 10).ToArray(); // even numbers from 10

                for (var key = 7; key <= length * 2 + 13; key++)
                {
                    var expected = Array.BinarySearch(array, key);

                    BinarySearch.Find(array, new IntComparable(key)).ShouldBe(expected, $"Find, length {length}, key {key}");
                    BinarySearch.Find(array, new ExtremeComparable(key)).ShouldBe(expected, $"Find, length {length}, key {key}");

                    var expectedIndex = (expected >= 0) ? expected : -1;
                    BinarySearch.FindBranchless(array, new IntComparable(key)).ShouldBe(expectedIndex, $"FindBranchless, length {length}, key {key}");
                    BinarySearch.FindBranchless(array, new ExtremeComparable(key)).ShouldBe(expectedIndex, $"FindBranchless, length {length}, key {key}");
                }
            }
        }

        [Fact]
        public void ReferenceTypeElements()
        {
            var array = Enumerable.Range(0, 300).Select(x => "key_" + x.ToString("D4")).ToArray();

            for (var i = 0; i < array.Length; i++)
            {
                var key = new string(array[i].ToCharArray()); // equal, not the same instance
                BinarySearch.Find(array, new OrdinalComparable(key)).ShouldBe(i);
                BinarySearch.FindBranchless(array, new OrdinalComparable(key)).ShouldBe(i);
            }

            foreach (var missing in new[] { "", "a", "key_", "key_0000x", "key_0150_", "key_9999", "z" })
            {
                BinarySearch.Find(array, new OrdinalComparable(missing)).ShouldBe(Array.BinarySearch(array, missing, StringComparer.Ordinal));
                BinarySearch.FindBranchless(array, new OrdinalComparable(missing)).ShouldBe(-1);
            }
        }

        [Fact]
        public void LargeArray()
        {
            var array = Enumerable.Range(0, 1_000_003).Select(x => x * 3).ToArray();
            var random = new Random(7);

            for (var i = 0; i < 100_000; i++)
            {
                var key = random.Next(-5, array.Length * 3 + 5);
                var expected = Array.BinarySearch(array, key);

                BinarySearch.Find(array, new IntComparable(key)).ShouldBe(expected);
                BinarySearch.FindBranchless(array, new IntComparable(key)).ShouldBe((expected >= 0) ? expected : -1);
            }
        }
    }
}
