using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    public class RangeViewTest
    {
        [Fact]
        public void Range()
        {
            // 4 -> 8
            {
                var range = new RangeView<int>(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 4, 8, true);

                range.Count.ShouldBe(5);
                range[0].ShouldBe(4);
                range[1].ShouldBe(5);
                range[2].ShouldBe(6);
                range[3].ShouldBe(7);
                range[4].ShouldBe(8);

                Assert.Throws<ArgumentOutOfRangeException>(() => range[-1]);
                Assert.Throws<ArgumentOutOfRangeException>(() => range[5]);

                var begin = 4;
                foreach (var item in range)
                {
                    item.ShouldBe(begin++);
                }

                var xs = new int[10];
                range.CopyTo(xs, 3);
                xs[3].ShouldBe(4);
                xs[4].ShouldBe(5);
                xs[5].ShouldBe(6);
                xs[6].ShouldBe(7);
                xs[7].ShouldBe(8);
                xs[8].ShouldBe(0);

                range.IndexOf(5).ShouldBe(1);
                range.IndexOf(9).ShouldBe(-1);


                range.Contains(5).ShouldBeTrue();
                range.Contains(9).ShouldBeFalse();
            }
            {
                // 7 -> 2
                var range = new RangeView<int>(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 2, 7, false);

                range.Count.ShouldBe(6);
                range[0].ShouldBe(7);
                range[1].ShouldBe(6);
                range[2].ShouldBe(5);
                range[3].ShouldBe(4);
                range[4].ShouldBe(3);
                range[5].ShouldBe(2);

                Assert.Throws<ArgumentOutOfRangeException>(() => range[-1]);
                Assert.Throws<ArgumentOutOfRangeException>(() => range[6]);

                var begin = 7;
                foreach (var item in range)
                {
                    item.ShouldBe(begin--);
                }

                var xs = new int[10];
                range.CopyTo(xs, 3);
                xs[3].ShouldBe(7);
                xs[4].ShouldBe(6);
                xs[5].ShouldBe(5);
                xs[6].ShouldBe(4);
                xs[7].ShouldBe(3);
                xs[8].ShouldBe(2);

                range.IndexOf(5).ShouldBe(2);
                range.IndexOf(9).ShouldBe(-1);

                range.Contains(5).ShouldBeTrue();
                range.Contains(9).ShouldBeFalse();
            }

            var empty = new RangeView<int>(Enumerable.Empty<int>().ToArray(), 0, 0, true);
            empty.Count.ShouldBe(0);

            var same = new RangeView<int>(Enumerable.Range(1, 1000).ToArray(), 100, 100, true);
            same.Count.ShouldBe(1);
            same[0].ShouldBe(101);
        }
    }
}
