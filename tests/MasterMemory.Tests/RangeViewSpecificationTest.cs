using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MasterMemory.Tests
{
    // RangeView is what every range and shared-key lookup hands back. Each member is checked against the plain array
    // slice it stands for, for every left / right of arrays up to six elements, both directions, and the forms an
    // empty view is built in (no array, an empty array, left past right).
    public class RangeViewSpecificationTest
    {
        static IEnumerable<(int[] data, int left, int right, bool ascendant, int[] expected)> Cases()
        {
            foreach (var ascendant in new[] { true, false })
            {
                yield return (null, -1, -1, ascendant, Array.Empty<int>());                 // RangeView<T>.Empty
                yield return (Array.Empty<int>(), 0, 0, ascendant, Array.Empty<int>());     // a range over an empty table
                yield return (Array.Empty<int>(), 0, -1, ascendant, Array.Empty<int>());

                for (var length = 1; length <= 6; length++)
                {
                    var data = Enumerable.Range(0, length).Select(x => x * 10 + 5).ToArray();
                    for (var left = 0; left < length; left++)
                    {
                        for (var right = left; right < length; right++)
                        {
                            var slice = data.Skip(left).Take(right - left + 1);
                            yield return (data, left, right, ascendant, (ascendant ? slice : slice.Reverse()).ToArray());
                        }
                    }

                    // what a lookup that found nothing builds: left past right, either of them one outside the array
                    yield return (data, 0, -1, ascendant, Array.Empty<int>());
                    yield return (data, length, length - 1, ascendant, Array.Empty<int>());
                    yield return (data, 2, 1, ascendant, Array.Empty<int>());
                }
            }
        }

        [Fact]
        public void EveryMember()
        {
            foreach (var (data, left, right, ascendant, expected) in Cases())
            {
                var view = new RangeView<int>(data, left, right, ascendant);
                var context = $"[{(data == null ? "null" : string.Join(",", data))}] {left}..{right} ascendant:{ascendant}";

                view.Count.ShouldBe(expected.Length, context);
                view.Any().ShouldBe(expected.Length != 0, context);

                // the indexer, inside and just outside
                for (var i = 0; i < expected.Length; i++)
                {
                    view[i].ShouldBe(expected[i], context);
                    ((IReadOnlyList<int>)view)[i].ShouldBe(expected[i], context);
                    ((IList<int>)view)[i].ShouldBe(expected[i], context);
                }
                Should.Throw<ArgumentOutOfRangeException>(() => view[-1], context);
                Should.Throw<ArgumentOutOfRangeException>(() => view[expected.Length], context);

                if (expected.Length != 0)
                {
                    view.First.ShouldBe(expected[0], context);
                    view.Last.ShouldBe(expected[expected.Length - 1], context);
                }
                else
                {
                    Should.Throw<ArgumentOutOfRangeException>(() => view.First, context);
                    Should.Throw<ArgumentOutOfRangeException>(() => view.Last, context);
                }

                // enumeration: the struct enumerator, both interfaces, LINQ's collection fast paths
                var enumerated = new List<int>();
                foreach (var item in view) enumerated.Add(item);
                enumerated.ToArray().ShouldBe(expected, context);
                ((IEnumerable<int>)view).Select(x => x).ToArray().ShouldBe(expected, context);
                ((IEnumerable)view).Cast<int>().ToArray().ShouldBe(expected, context);
                view.ToArray().ShouldBe(expected, context);
                view.ToList().ToArray().ShouldBe(expected, context);
                new List<int>(view).ToArray().ShouldBe(expected, context);

                // Reverse, and back
                view.Reverse.ToArray().ShouldBe(expected.Reverse().ToArray(), context);
                view.Reverse.Count.ShouldBe(expected.Length, context);
                view.Reverse.Reverse.ToArray().ShouldBe(expected, context);

                // CopyTo into the middle of a larger array, which must leave the rest alone
                var destination = Enumerable.Repeat(-1, expected.Length + 4).ToArray();
                view.CopyTo(destination, 2);
                destination.ShouldBe(new[] { -1, -1 }.Concat(expected).Concat(new[] { -1, -1 }).ToArray(), context);

                // Contains / IndexOf, for every element and for values that are not there
                for (var i = 0; i < expected.Length; i++)
                {
                    view.Contains(expected[i]).ShouldBeTrue(context);
                    view.IndexOf(expected[i]).ShouldBe(i, context);
                }
                foreach (var missing in new[] { -1, 0, 4, 6, 1000 })
                {
                    view.Contains(missing).ShouldBe(expected.Contains(missing), context);
                    view.IndexOf(missing).ShouldBe(Array.IndexOf(expected, missing), context);
                }

                // a value of the array that lies outside the view is not in the view
                if (data != null)
                {
                    foreach (var outside in data.Except(expected))
                    {
                        view.Contains(outside).ShouldBeFalse(context);
                        view.IndexOf(outside).ShouldBe(-1, context);
                    }
                }

                // read-only
                var list = (IList<int>)view;
                list.IsReadOnly.ShouldBeTrue(context);
                Should.Throw<NotSupportedException>(() => list.Add(1), context);
                Should.Throw<NotSupportedException>(() => list.Clear(), context);
                Should.Throw<NotSupportedException>(() => list.Insert(0, 1), context);
                Should.Throw<NotSupportedException>(() => list.Remove(1), context);
                Should.Throw<NotSupportedException>(() => list.RemoveAt(0), context);
                Should.Throw<NotSupportedException>(() => list[0] = 1, context);
            }
        }

        [Fact]
        public void DefaultIsEmpty()
        {
            var view = default(RangeView<int>);

            view.Count.ShouldBe(0);
            view.Any().ShouldBeFalse();
            view.ToArray().ShouldBeEmpty();
            view.Reverse.Count.ShouldBe(0);
            view.Contains(0).ShouldBeFalse();
            view.IndexOf(0).ShouldBe(-1);
            view.CopyTo(new int[1], 0);
        }
    }
}
