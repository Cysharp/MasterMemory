using System;
using System.Collections.Generic;
using System.Linq;
using RuntimeUnitTestToolkit;

namespace MasterMemory.Tests
{
    public class RangeViewTest
    {
        
        public void Range()
        {
            // 4 -> 8
            {
                var range = new RangeView<int>(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 4, 8, true);

                range.Count.Is(5);
                range[0].Is(4);
                range[1].Is(5);
                range[2].Is(6);
                range[3].Is(7);
                range[4].Is(8);

                Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = range[-1]; UnityEngine.Debug.Log(_); });
                Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = range[5]; UnityEngine.Debug.Log(_); });

                var begin = 4;
                foreach (var item in range)
                {
                    item.Is(begin++);
                }

                var c = (ICollection<int>)range;
                c.Contains(6).IsTrue();
                c.Contains(-1).IsFalse();

                var l = (IList<int>)range;
                l.IndexOf(6).Is(2);
                var newXs = new int[10];
                l.CopyTo(newXs, 3);
                newXs[3].Is(4);
                newXs[4].Is(5);
                newXs[5].Is(6);
                newXs[6].Is(7);
                newXs[7].Is(8);
            }
            {
                // 7 -> 2
                var range = new RangeView<int>(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 2, 7, false);

                range.Count.Is(6);
                range[0].Is(7);
                range[1].Is(6);
                range[2].Is(5);
                range[3].Is(4);
                range[4].Is(3);
                range[5].Is(2);

                Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = range[-1]; UnityEngine.Debug.Log(_); });
                Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = range[6]; UnityEngine.Debug.Log(_); });

                var begin = 7;
                foreach (var item in range)
                {
                    item.Is(begin--);
                }

                var c = (ICollection<int>)range;
                c.Contains(6).IsTrue();
                c.Contains(-1).IsFalse();

                var l = (IList<int>)range;
                l.IndexOf(6).Is(1);
                var newXs = new int[10];
                l.CopyTo(newXs, 3);
                newXs[3].Is(7);
                newXs[4].Is(6);
                newXs[5].Is(5);
                newXs[6].Is(4);
                newXs[7].Is(3);
                newXs[8].Is(2);
            }

            var empty = new RangeView<int>(Enumerable.Empty<int>().ToArray(), 0, 0, true);
            empty.Count.Is(0);

            var same = new RangeView<int>(Enumerable.Range(1, 1000).ToArray(), 100, 100, true);
            same.Count.Is(1);
            same[0].Is(101);


            Assert.Throws<ArgumentException>(() => new RangeView<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 5, 4, true));
        }
    }
}
