using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace MasterMemory.Tests
{
    // RangeView is what every range and shared-key lookup hands back, and ReverseRangeView is its Reverse. Each member
    // of both is checked against the plain array slice it stands for, for every offset / count of arrays up to six
    // elements, and for the forms an empty view comes in (default, Empty, no elements of an array).
    public class RangeViewSpecificationTest
    {
        static IEnumerable<(int[] data, int offset, int count)> Cases()
        {
            var arrays = new List<int[]> { Array.Empty<int>() };
            for (var length = 1; length <= 6; length++)
            {
                arrays.Add(Enumerable.Range(0, length).Select(x => x * 10 + 5).ToArray());
            }
            arrays.Add(new[] { 5, 7, 5, 7, 7, 5 }); // values that repeat: IndexOf is the first one in the order of the view

            foreach (var data in arrays)
            {
                for (var offset = 0; offset <= data.Length; offset++)
                {
                    for (var count = 0; count <= data.Length - offset; count++)
                    {
                        yield return (data, offset, count);
                    }
                }
            }
        }

        [Fact]
        public void EveryMember()
        {
            foreach (var (data, offset, count) in Cases())
            {
                var view = new RangeView<int>(data, offset, count);
                var expected = data.Skip(offset).Take(count).ToArray();
                var reversed = expected.Reverse().ToArray();
                var context = $"[{string.Join(",", data)}] offset:{offset} count:{count}";

                CheckView(view, expected, data, context);
                CheckReverse(view.Reverse, reversed, data, context + " Reverse");

                // and back: the same window on the same array
                CheckView(view.Reverse.Reverse, expected, data, context + " Reverse.Reverse");

                // the span is that part of the array itself, not a copy of it
                if (count != 0)
                {
                    Unsafe.AreSame(ref MemoryMarshal.GetReference(view.AsSpan()), ref data[offset]).ShouldBeTrue(context);
                    Unsafe.AreSame(ref MemoryMarshal.GetReference(view.Reverse.Reverse.AsSpan()), ref data[offset]).ShouldBeTrue(context);
                }
            }
        }

        static void CheckView(RangeView<int> view, int[] expected, int[] data, string context)
        {
            view.Count.ShouldBe(expected.Length, context);
            view.Any().ShouldBe(expected.Length != 0, context);

            for (var i = 0; i < expected.Length; i++)
            {
                view[i].ShouldBe(expected[i], context);
            }
            Should.Throw<ArgumentOutOfRangeException>(() => view[-1], context);
            Should.Throw<ArgumentOutOfRangeException>(() => view[expected.Length], context);
            Should.Throw<ArgumentOutOfRangeException>(() => view[int.MinValue], context);
            Should.Throw<ArgumentOutOfRangeException>(() => view[int.MaxValue], context);

            if (expected.Length != 0)
            {
                view.First.ShouldBe(expected[0], context);
                view.Last.ShouldBe(expected[expected.Length - 1], context);
                view.FirstOrDefault().ShouldBe(expected[0], context);
                view.LastOrDefault().ShouldBe(expected[expected.Length - 1], context);
            }
            else
            {
                Should.Throw<ArgumentOutOfRangeException>(() => view.First, context);
                Should.Throw<ArgumentOutOfRangeException>(() => view.Last, context);
                view.FirstOrDefault().ShouldBe(0, context);
                view.LastOrDefault().ShouldBe(0, context);
            }

            var enumerated = new List<int>();
            foreach (var item in view) enumerated.Add(item);
            enumerated.ToArray().ShouldBe(expected, context);

            view.ToArray().ShouldBe(expected, context);
            if (expected.Length != 0) view.ToArray().ShouldNotBeSameAs(data, context);

            view.AsSpan().ToArray().ShouldBe(expected, context);

            var destination = Enumerable.Repeat(-1, expected.Length + 4).ToArray();
            view.CopyTo(destination, 2);
            destination.ShouldBe(new[] { -1, -1 }.Concat(expected).Concat(new[] { -1, -1 }).ToArray(), context);

            foreach (var value in expected.Concat(data).Concat(new[] { -1, 0, 4, 6, 1000 }).Distinct())
            {
                view.IndexOf(value).ShouldBe(Array.IndexOf(expected, value), context + " IndexOf " + value);
                view.Contains(value).ShouldBe(expected.Contains(value), context + " Contains " + value);
            }

            CheckInterfaces(view, expected, context);
        }

        static void CheckReverse(ReverseRangeView<int> view, int[] expected, int[] data, string context)
        {
            view.Count.ShouldBe(expected.Length, context);
            view.Any().ShouldBe(expected.Length != 0, context);

            for (var i = 0; i < expected.Length; i++)
            {
                view[i].ShouldBe(expected[i], context);
            }
            Should.Throw<ArgumentOutOfRangeException>(() => view[-1], context);
            Should.Throw<ArgumentOutOfRangeException>(() => view[expected.Length], context);
            Should.Throw<ArgumentOutOfRangeException>(() => view[int.MinValue], context);
            Should.Throw<ArgumentOutOfRangeException>(() => view[int.MaxValue], context);

            if (expected.Length != 0)
            {
                view.First.ShouldBe(expected[0], context);
                view.Last.ShouldBe(expected[expected.Length - 1], context);
                view.FirstOrDefault().ShouldBe(expected[0], context);
                view.LastOrDefault().ShouldBe(expected[expected.Length - 1], context);
            }
            else
            {
                Should.Throw<ArgumentOutOfRangeException>(() => view.First, context);
                Should.Throw<ArgumentOutOfRangeException>(() => view.Last, context);
                view.FirstOrDefault().ShouldBe(0, context);
                view.LastOrDefault().ShouldBe(0, context);
            }

            var enumerated = new List<int>();
            foreach (var item in view) enumerated.Add(item);
            enumerated.ToArray().ShouldBe(expected, context);

            view.ToArray().ShouldBe(expected, context);
            if (expected.Length != 0) view.ToArray().ShouldNotBeSameAs(data, context);

            var destination = Enumerable.Repeat(-1, expected.Length + 4).ToArray();
            view.CopyTo(destination, 2);
            destination.ShouldBe(new[] { -1, -1 }.Concat(expected).Concat(new[] { -1, -1 }).ToArray(), context);

            foreach (var value in expected.Concat(data).Concat(new[] { -1, 0, 4, 6, 1000 }).Distinct())
            {
                view.IndexOf(value).ShouldBe(Array.IndexOf(expected, value), context + " IndexOf " + value);
                view.Contains(value).ShouldBe(expected.Contains(value), context + " Contains " + value);
            }

            CheckInterfaces(view, expected, context);
        }

        // what the two views have in common: the interfaces, and what LINQ makes of them
        static void CheckInterfaces<TView>(TView view, int[] expected, string context)
            where TView : IList<int>, IReadOnlyList<int>
        {
            var list = (IList<int>)view;
            var readOnlyList = (IReadOnlyList<int>)view;

            list.Count.ShouldBe(expected.Length, context);
            readOnlyList.Count.ShouldBe(expected.Length, context);
            for (var i = 0; i < expected.Length; i++)
            {
                list[i].ShouldBe(expected[i], context);
                readOnlyList[i].ShouldBe(expected[i], context);
            }
            Should.Throw<ArgumentOutOfRangeException>(() => list[expected.Length], context);

            ((IEnumerable<int>)view).Select(x => x).ToArray().ShouldBe(expected, context);
            ((IEnumerable)view).Cast<int>().ToArray().ShouldBe(expected, context);
            Enumerable.ToArray(list).ShouldBe(expected, context);
            Enumerable.ToList(list).ToArray().ShouldBe(expected, context);
            new List<int>(list).ToArray().ShouldBe(expected, context);
            Enumerable.Count(list).ShouldBe(expected.Length, context);
            if (expected.Length != 0)
            {
                Enumerable.First(list).ShouldBe(expected[0], context);
                Enumerable.Last(list).ShouldBe(expected[expected.Length - 1], context);
                Enumerable.ElementAt(list, expected.Length - 1).ShouldBe(expected[expected.Length - 1], context);
            }

            // an enumerator behind the interface: to the end, past it, and again after Reset
            using (var enumerator = ((IEnumerable<int>)view).GetEnumerator())
            {
                for (var round = 0; round < 2; round++)
                {
                    foreach (var item in expected)
                    {
                        enumerator.MoveNext().ShouldBeTrue(context);
                        enumerator.Current.ShouldBe(item, context);
                    }
                    enumerator.MoveNext().ShouldBeFalse(context);
                    enumerator.MoveNext().ShouldBeFalse(context);
                    enumerator.Reset();
                }
            }

            // read-only
            list.IsReadOnly.ShouldBeTrue(context);
            Should.Throw<NotSupportedException>(() => list.Add(1), context);
            Should.Throw<NotSupportedException>(() => list.Clear(), context);
            Should.Throw<NotSupportedException>(() => list.Insert(0, 1), context);
            Should.Throw<NotSupportedException>(() => list.Remove(1), context);
            Should.Throw<NotSupportedException>(() => list.RemoveAt(0), context);
            Should.Throw<NotSupportedException>(() => list[0] = 1, context);
        }

        [Fact]
        public void EmptyInEveryForm()
        {
            var none = Array.Empty<int>();

            CheckView(default, none, none, "default");
            CheckView(RangeView<int>.Empty, none, none, "Empty");
            CheckView(default(ReverseRangeView<int>).Reverse, none, none, "default reverse, Reverse");
            CheckView(new RangeView<int>(none), none, none, "an empty array");

            CheckReverse(default, none, none, "default");
            CheckReverse(ReverseRangeView<int>.Empty, none, none, "Empty");
            CheckReverse(default(RangeView<int>).Reverse, none, none, "default, Reverse");
            CheckReverse(new RangeView<int>(none).Reverse, none, none, "an empty array, Reverse");

            default(RangeView<int>).AsSpan().IsEmpty.ShouldBeTrue();
            default(ReverseRangeView<int>).Reverse.AsSpan().IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void WholeArray()
        {
            var data = new[] { 3, 1, 2 };

            CheckView(new RangeView<int>(data), data, data, "whole");
            Unsafe.AreSame(ref MemoryMarshal.GetReference(new RangeView<int>(data).AsSpan()), ref data[0]).ShouldBeTrue();
        }

        [Fact]
        public void WhatIsNotARangeOfTheArray()
        {
            var data = new[] { 1, 2, 3 };

            Should.Throw<ArgumentNullException>(() => new RangeView<int>(null));
            Should.Throw<ArgumentNullException>(() => new RangeView<int>(null, 0, 0));
            Should.Throw<ArgumentOutOfRangeException>(() => new RangeView<int>(data, -1, 1));
            Should.Throw<ArgumentOutOfRangeException>(() => new RangeView<int>(data, 4, 0));
            Should.Throw<ArgumentOutOfRangeException>(() => new RangeView<int>(data, 0, -1));
            Should.Throw<ArgumentOutOfRangeException>(() => new RangeView<int>(data, 0, 4));
            Should.Throw<ArgumentOutOfRangeException>(() => new RangeView<int>(data, 2, 2));
            Should.Throw<ArgumentOutOfRangeException>(() => new RangeView<int>(data, 3, 1));
            Should.Throw<ArgumentOutOfRangeException>(() => new RangeView<int>(data, int.MaxValue, int.MaxValue));

            // the end of the array is a place for an empty view
            new RangeView<int>(data, 3, 0).Count.ShouldBe(0);
        }

        [Fact]
        public void RowsThatAreObjects()
        {
            var a = "a";
            var b = "b";
            var data = new[] { a, null, b, a };
            var view = new RangeView<string>(data, 1, 3);

            view.ToArray().ShouldBe(new[] { null, b, a });
            view.IndexOf(null).ShouldBe(0);
            view.IndexOf(a).ShouldBe(2);
            view.Contains("c").ShouldBeFalse();
            view.FirstOrDefault().ShouldBeNull();
            view.LastOrDefault().ShouldBeSameAs(a);

            view.Reverse.ToArray().ShouldBe(new[] { a, b, null });
            view.Reverse.IndexOf(null).ShouldBe(2);
            view.Reverse.IndexOf(a).ShouldBe(0);
            view.Reverse.FirstOrDefault().ShouldBeSameAs(a);
            view.Reverse.LastOrDefault().ShouldBeNull();

            default(RangeView<string>).FirstOrDefault().ShouldBeNull();
            default(ReverseRangeView<string>).LastOrDefault().ShouldBeNull();
        }
    }
}
