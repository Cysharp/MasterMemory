using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    public class RangeViewEnumeratorTest
    {
        static readonly int[] Data = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        static List<int> Enumerate(RangeView<int> view)
        {
            var list = new List<int>();
            foreach (var item in view)
            {
                list.Add(item);
            }
            return list;
        }

        [Theory]
        [InlineData(4, 8, true)]
        [InlineData(4, 8, false)]
        [InlineData(0, 9, true)]
        [InlineData(0, 9, false)]
        [InlineData(3, 3, true)]
        [InlineData(3, 3, false)]
        [InlineData(5, 4, true)] // empty: left is past right
        public void SameAsIndexer(int left, int right, bool ascendant)
        {
            var view = new RangeView<int>(Data, left, right, ascendant);
            var expected = Enumerable.Range(0, view.Count).Select(i => view[i]).ToArray();

            Enumerate(view).ToArray().ShouldBe(expected);

            // the interface paths box the same enumerator
            ((IEnumerable<int>)view).ToArray().ShouldBe(expected);
            ((IEnumerable)view).Cast<int>().ToArray().ShouldBe(expected);
            view.Reverse.Reverse.ToArray().ShouldBe(expected);
        }

        [Fact]
        public void Empty()
        {
            Enumerate(RangeView<int>.Empty).Count.ShouldBe(0);
            Enumerate(new RangeView<int>(Array.Empty<int>(), 0, 0, true)).Count.ShouldBe(0);
            Enumerate(default).Count.ShouldBe(0);
        }

        [Fact]
        public void EnumeratorIsReusableAfterReset()
        {
            var view = new RangeView<int>(Data, 2, 4, false);
            var enumerator = view.GetEnumerator();

            enumerator.MoveNext().ShouldBeTrue();
            enumerator.Current.ShouldBe(4);
            enumerator.MoveNext().ShouldBeTrue();
            enumerator.MoveNext().ShouldBeTrue();
            enumerator.Current.ShouldBe(2);
            enumerator.MoveNext().ShouldBeFalse();
            enumerator.MoveNext().ShouldBeFalse();

            enumerator.Reset();
            enumerator.MoveNext().ShouldBeTrue();
            enumerator.Current.ShouldBe(4);
        }

        [Fact]
        public void ForeachDoesNotAllocate()
        {
            var view = new RangeView<int>(Data, 0, 9, true);
            var sum = 0;
            foreach (var item in view) { sum += item; } // warm up

            var before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 100; i++)
            {
                foreach (var item in view)
                {
                    sum += item;
                }
            }
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            allocated.ShouldBe(0);
            sum.ShouldBe(45 * 101);
        }
    }
}
