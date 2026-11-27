using MasterMemory.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    public class BinarySearchTest
    {
        readonly struct IntKey : IComparable<int>
        {
            readonly int key;
            public IntKey(int key) => this.key = key;
            public int CompareTo(int other) => key.CompareTo(other);
        }

        sealed class Bound
        {
            public int id;
            public int bound;
        }

        readonly struct BoundKey : IComparable<Bound>
        {
            readonly int key;
            public BoundKey(int key) => this.key = key;
            public int CompareTo(Bound other) => key.CompareTo(other.bound);
        }

        [Fact]
        public void Find()
        {
            var rand = new Random();
            for (int iii = 0; iii < 30; iii++)
            {
                var seed = Enumerable.Range(1, 10);
                var randomSeed = seed.Where(x => rand.Next() % 2 == 0);

                var array = randomSeed.Concat(randomSeed).Concat(randomSeed).Concat(randomSeed)
                    .OrderBy(x => x)
                    .ToArray();

                for (int i = 1; i <= 10; i++)
                {
                    var firstIndex = Array.IndexOf(array, i);
                    var lastIndex = Array.LastIndexOf(array, i);

                    var f = MasterMemory.Tests.Legacy.LegacyBinarySearch.FindFirst(array, i, x => x, Comparer<int>.Default);
                    var s = BinarySearch.Find(array, new IntKey(i));
                    var l = BinarySearch.LowerBound(array, new IntKey(i));
                    var u = BinarySearch.UpperBound(array, new IntKey(i));

                    // not found
                    if (firstIndex == -1)
                    {
                        f.ShouldBe(-1);
                        s.ShouldBeLessThan(0);
                        l.ShouldBe(-1);
                        u.ShouldBe(-1);
                        continue;
                    }

                    array[f].ShouldBe(i);
                    array[s].ShouldBe(i);
                    array[l].ShouldBe(i);
                    array[u].ShouldBe(i);

                    s.ShouldBe(f); // the same halving, so the same one of several equal elements
                    l.ShouldBe(firstIndex);
                    u.ShouldBe(lastIndex);
                }
            }

            // and empty
            var emptyArray = Enumerable.Empty<int>().ToArray();
            MasterMemory.Tests.Legacy.LegacyBinarySearch.FindFirst(emptyArray, 0, x => x, Comparer<int>.Default).ShouldBe(-1);
            BinarySearch.Find(emptyArray, new IntKey(0)).ShouldBe(-1);
            BinarySearch.LowerBound(emptyArray, new IntKey(0)).ShouldBe(-1);
            BinarySearch.UpperBound(emptyArray, new IntKey(0)).ShouldBe(-1);
        }

        [Fact]
        public void Closest()
        {
            // empty
            var array = Enumerable.Empty<int>().ToArray();

            var near = BinarySearch.FindClosest(array, new IntKey(0), false);
            near.ShouldBe(-1);

            // mid
            var source = new[]{
                new Bound { id = 0, bound = 0 },
                new Bound { id = 1, bound = 100 },
                new Bound { id = 2, bound = 200 },
                new Bound { id = 3, bound = 300 },
                new Bound { id = 4, bound = 500 },
                new Bound { id = 5, bound = 1000 },
            };

            BinarySearch.FindClosest(source, new BoundKey(-100), true).ShouldBe(-1);
            BinarySearch.FindClosest(source, new BoundKey(0), true).ShouldBe(0);
            BinarySearch.FindClosest(source, new BoundKey(10), true).ShouldBe(0);
            BinarySearch.FindClosest(source, new BoundKey(50), true).ShouldBe(0);

            source[BinarySearch.FindClosest(source, new BoundKey(100), true)].id.ShouldBe(1);
            source[BinarySearch.FindClosest(source, new BoundKey(100), false)].id.ShouldBe(1);

            source[BinarySearch.FindClosest(source, new BoundKey(150), true)].id.ShouldBe(1);
            source[BinarySearch.FindClosest(source, new BoundKey(300), true)].id.ShouldBe(3);
            source[BinarySearch.FindClosest(source, new BoundKey(999), true)].id.ShouldBe(4);
            source[BinarySearch.FindClosest(source, new BoundKey(1000), true)].id.ShouldBe(5);
            source[BinarySearch.FindClosest(source, new BoundKey(1001), true)].id.ShouldBe(5);
            source[BinarySearch.FindClosest(source, new BoundKey(10000), true)].id.ShouldBe(5);

            // past the last element, asking for the upper neighbour: one past the end, which a caller reads as "no element"
            BinarySearch.FindClosest(source, new BoundKey(10000), false).ShouldBe(6);
        }
    }
}
