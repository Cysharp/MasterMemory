using MasterMemory.Internal;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using MessagePack;

namespace MasterMemory.Tests
{
    public class BinarySearchTest
    {
        public BinarySearchTest()
        {
            MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions.WithResolver(MessagePackResolver.Instance);
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

                    var f = BinarySearch.FindFirst(array, i, x => x, Comparer<int>.Default);
                    var l = BinarySearch.LowerBound(array, 0, array.Length, i, x => x, Comparer<int>.Default);
                    var u = BinarySearch.UpperBound(array, 0, array.Length, i, x => x, Comparer<int>.Default);

                    // not found
                    if (firstIndex == -1)
                    {
                        f.ShouldBe(-1);
                        l.ShouldBe(-1);
                        u.ShouldBe(-1);
                        continue;
                    }

                    array[f].ShouldBe(i);
                    array[l].ShouldBe(i);
                    array[u].ShouldBe(i);

                    l.ShouldBe(firstIndex);
                    u.ShouldBe(lastIndex);
                }
            }

            // and empty
            var emptyArray = Enumerable.Empty<int>().ToArray();
            BinarySearch.FindFirst(emptyArray, 0, x => x, Comparer<int>.Default).ShouldBe(-1);
            BinarySearch.LowerBound(emptyArray, 0, emptyArray.Length, 0, x => x, Comparer<int>.Default).ShouldBe(-1);
            BinarySearch.UpperBound(emptyArray, 0, emptyArray.Length, 0, x => x, Comparer<int>.Default).ShouldBe(-1);
        }

        [Fact]
        public void Closest()
        {
            // empty
            var array = Enumerable.Empty<int>().ToArray();

            var near = BinarySearch.FindClosest(array, 0, 0, array.Length, x => x, Comparer<int>.Default, false);
            near.ShouldBe(-1);

            // mid
            var source = new[]{
                new { id = 0, bound = 0 },
                new { id = 1, bound = 100 },
                new { id = 2, bound = 200 },
                new { id = 3, bound = 300 },
                new { id = 4, bound = 500 },
                new { id = 5, bound = 1000 },
            };

            BinarySearch.FindClosest(source, 0, source.Length, -100, x => x.bound, Comparer<int>.Default, true).ShouldBe(-1);
//          BinarySearch.FindClosest(source, 0, source.Length, -100, x => x.bound, Comparer<int>.Default, true).ShouldBe(0);
            BinarySearch.FindClosest(source, 0, source.Length, 0, x => x.bound, Comparer<int>.Default, true).ShouldBe(0);
            BinarySearch.FindClosest(source, 0, source.Length, 10, x => x.bound, Comparer<int>.Default, true).ShouldBe(0);
            BinarySearch.FindClosest(source, 0, source.Length, 50, x => x.bound, Comparer<int>.Default, true).ShouldBe(0);

            source[BinarySearch.FindClosest(source, 0, source.Length, 100, x => x.bound, Comparer<int>.Default, true)].id.ShouldBe(1);
            source[BinarySearch.FindClosest(source, 0, source.Length, 100, x => x.bound, Comparer<int>.Default, false)].id.ShouldBe(1);

            source[BinarySearch.FindClosest(source, 0, source.Length, 150, x => x.bound, Comparer<int>.Default, true)].id.ShouldBe(1);
            source[BinarySearch.FindClosest(source, 0, source.Length, 300, x => x.bound, Comparer<int>.Default, true)].id.ShouldBe(3);
            source[BinarySearch.FindClosest(source, 0, source.Length, 999, x => x.bound, Comparer<int>.Default, true)].id.ShouldBe(4);
            source[BinarySearch.FindClosest(source, 0, source.Length, 1000, x => x.bound, Comparer<int>.Default, true)].id.ShouldBe(5);
            source[BinarySearch.FindClosest(source, 0, source.Length, 1001, x => x.bound, Comparer<int>.Default, true)].id.ShouldBe(5);
            source[BinarySearch.FindClosest(source, 0, source.Length, 10000, x => x.bound, Comparer<int>.Default, true)].id.ShouldBe(5);
//          source[BinarySearch.FindClosest(source, 0, source.Length, 10000, x => x.bound, Comparer<int>.Default, false)].id.ShouldBe(5);
 
            BinarySearch.FindClosest(source, 0, source.Length, 10000, x => x.bound, Comparer<int>.Default, false).ShouldBe(6);
        }
    }
}
