using FluentAssertions;
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

                range.Count.Should().Be(5);
                range[0].Should().Be(4);
                range[1].Should().Be(5);
                range[2].Should().Be(6);
                range[3].Should().Be(7);
                range[4].Should().Be(8);

                Assert.Throws<ArgumentOutOfRangeException>(() => range[-1]);
                Assert.Throws<ArgumentOutOfRangeException>(() => range[5]);

                var begin = 4;
                foreach (var item in range)
                {
                    item.Should().Be(begin++);
                }

                var xs = new int[10];
                range.CopyTo(xs, 3);
                xs[3].Should().Be(4);
                xs[4].Should().Be(5);
                xs[5].Should().Be(6);
                xs[6].Should().Be(7);
                xs[7].Should().Be(8);
                xs[8].Should().Be(0);

                range.IndexOf(5).Should().Be(1);
                range.IndexOf(9).Should().Be(-1);


                range.Contains(5).Should().BeTrue();
                range.Contains(9).Should().BeFalse();
            }
            {
                // 7 -> 2
                var range = new RangeView<int>(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 2, 7, false);

                range.Count.Should().Be(6);
                range[0].Should().Be(7);
                range[1].Should().Be(6);
                range[2].Should().Be(5);
                range[3].Should().Be(4);
                range[4].Should().Be(3);
                range[5].Should().Be(2);

                Assert.Throws<ArgumentOutOfRangeException>(() => range[-1]);
                Assert.Throws<ArgumentOutOfRangeException>(() => range[6]);

                var begin = 7;
                foreach (var item in range)
                {
                    item.Should().Be(begin--);
                }

                var xs = new int[10];
                range.CopyTo(xs, 3);
                xs[3].Should().Be(7);
                xs[4].Should().Be(6);
                xs[5].Should().Be(5);
                xs[6].Should().Be(4);
                xs[7].Should().Be(3);
                xs[8].Should().Be(2);

                range.IndexOf(5).Should().Be(2);
                range.IndexOf(9).Should().Be(-1);

                range.Contains(5).Should().BeTrue();
                range.Contains(9).Should().BeFalse();
            }

            var empty = new RangeView<int>(Enumerable.Empty<int>().ToArray(), 0, 0, true);
            empty.Count.Should().Be(0);

            var same = new RangeView<int>(Enumerable.Range(1, 1000).ToArray(), 100, 100, true);
            same.Count.Should().Be(1);
            same[0].Should().Be(101);
        }
    }
}
