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

        static List<int> Enumerate(ReverseRangeView<int> view)
        {
            var list = new List<int>();
            foreach (var item in view)
            {
                list.Add(item);
            }
            return list;
        }

        [Theory]
        [InlineData(4, 5)]
        [InlineData(0, 10)]
        [InlineData(3, 1)]
        [InlineData(9, 1)]
        [InlineData(5, 0)]
        [InlineData(10, 0)]
        public void SameAsIndexer(int offset, int count)
        {
            var view = new RangeView<int>(Data, offset, count);
            var expected = Enumerable.Range(0, view.Count).Select(i => view[i]).ToArray();
            expected.ShouldBe(Data.Skip(offset).Take(count).ToArray());

            Enumerate(view).ToArray().ShouldBe(expected);

            // the interface paths box the same enumerator
            ((IEnumerable<int>)view).ToArray().ShouldBe(expected);
            ((IEnumerable)view).Cast<int>().ToArray().ShouldBe(expected);
            Enumerate(view.Reverse.Reverse).ToArray().ShouldBe(expected);

            var reverse = view.Reverse;
            var expectedReverse = Enumerable.Range(0, reverse.Count).Select(i => reverse[i]).ToArray();
            expectedReverse.ShouldBe(expected.Reverse().ToArray());

            Enumerate(reverse).ToArray().ShouldBe(expectedReverse);
            ((IEnumerable<int>)reverse).ToArray().ShouldBe(expectedReverse);
            ((IEnumerable)reverse).Cast<int>().ToArray().ShouldBe(expectedReverse);
        }

        [Fact]
        public void Empty()
        {
            Enumerate(RangeView<int>.Empty).Count.ShouldBe(0);
            Enumerate(new RangeView<int>(Array.Empty<int>())).Count.ShouldBe(0);
            Enumerate(default(RangeView<int>)).Count.ShouldBe(0);

            Enumerate(ReverseRangeView<int>.Empty).Count.ShouldBe(0);
            Enumerate(new RangeView<int>(Array.Empty<int>()).Reverse).Count.ShouldBe(0);
            Enumerate(default(ReverseRangeView<int>)).Count.ShouldBe(0);
        }

        [Fact]
        public void EnumeratorIsReusableAfterReset()
        {
            var view = new RangeView<int>(Data, 2, 3);
            var enumerator = view.GetEnumerator();

            enumerator.MoveNext().ShouldBeTrue();
            enumerator.Current.ShouldBe(2);
            enumerator.MoveNext().ShouldBeTrue();
            enumerator.MoveNext().ShouldBeTrue();
            enumerator.Current.ShouldBe(4);
            enumerator.MoveNext().ShouldBeFalse();
            enumerator.MoveNext().ShouldBeFalse();

            enumerator.Reset();
            enumerator.MoveNext().ShouldBeTrue();
            enumerator.Current.ShouldBe(2);
        }

        [Fact]
        public void ReverseEnumeratorIsReusableAfterReset()
        {
            var view = new RangeView<int>(Data, 2, 3).Reverse;
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
        public void ReadingDoesNotAllocate()
        {
            var view = new RangeView<int>(Data);
            var reverse = view.Reverse;
            var sum = 0;
            foreach (var item in view) { sum += item; } // warm up
            foreach (var item in reverse) { sum += item; }
            foreach (var item in view.AsSpan()) { sum += item; }
            sum += view.FirstOrDefault() + view.LastOrDefault() + reverse.FirstOrDefault() + reverse.LastOrDefault();

            var before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 100; i++)
            {
                foreach (var item in view)
                {
                    sum += item;
                }
                foreach (var item in reverse)
                {
                    sum += item;
                }
                foreach (var item in view.AsSpan())
                {
                    sum += item;
                }

                // the methods of the views themselves, not those of LINQ, which would box the view
                sum += view.FirstOrDefault() + view.LastOrDefault() + reverse.FirstOrDefault() + reverse.LastOrDefault();
                sum += view.Reverse.Reverse.Count;
            }
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            allocated.ShouldBe(0);
            sum.ShouldBe((45 * 3 + 18) * 101 + 10 * 100);
        }
    }
}
