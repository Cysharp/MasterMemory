using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MasterMemory
{
    /// <summary>
    /// Rows that lie together in an index of a table, in the order of that index: what a lookup by a key that several
    /// rows share, a range lookup, All and SortBy*** return. It is a window on the array of the index, and making one
    /// copies nothing. The same rows from the last to the first are its <see cref="Reverse"/>, a type of its own, so
    /// that reading this one never has to ask which way it runs, and it can always be read as a span.
    /// </summary>
    /// <remarks>
    /// A view implements no interface. Read through one, a struct is boxed, and LINQ over it allocates: here that happens
    /// only where the code asks for it, with <see cref="AsReadOnlyList"/>. foreach, the indexer, Count, AsSpan and the
    /// members of the view allocate nothing.
    /// </remarks>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(RangeViewDebugView<>))]
    public readonly struct RangeView<T>
    {
        public static RangeView<T> Empty => default;

        readonly T[]? array;
        readonly int offset;
        readonly int count;

        /// <summary>Every element of the array.</summary>
        public RangeView(T[] orderedData)
        {
            if (orderedData == null) throw new ArgumentNullException(nameof(orderedData));

            this.array = orderedData;
            this.offset = 0;
            this.count = orderedData.Length;
        }

        /// <summary><paramref name="count"/> elements of the array, from the one at <paramref name="offset"/>.</summary>
        public RangeView(T[] orderedData, int offset, int count)
        {
            if (orderedData == null) throw new ArgumentNullException(nameof(orderedData));
            if ((uint)offset > (uint)orderedData.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if ((uint)count > (uint)(orderedData.Length - offset)) throw new ArgumentOutOfRangeException(nameof(count));

            this.array = orderedData;
            this.offset = offset;
            this.count = count;
        }

        // The elements from left to right, both included, which is how the searches answer; empty when right is before left.
        internal static RangeView<T> FromBounds(T[] orderedData, int left, int right)
        {
            return (right < left) ? default : new RangeView<T>(orderedData, left, right - left + 1);
        }

        public int Count => count;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)count) ThrowIndexOutOfRange();
                return array![offset + index];
            }
        }

        /// <summary>The first row; throws when the view is empty, see <see cref="FirstOrDefault"/>.</summary>
        public T First => this[0];

        /// <summary>The last row; throws when the view is empty, see <see cref="LastOrDefault"/>.</summary>
        public T Last => this[count - 1];

        /// <summary>The same rows, from the last to the first.</summary>
        public ReverseRangeView<T> Reverse => new ReverseRangeView<T>(array, offset, count);

        public bool Any()
        {
            return count != 0;
        }

        /// <summary>The first row, default when the view is empty. Unlike the method of LINQ, the view is not boxed for it.</summary>
        public T? FirstOrDefault()
        {
            return (count == 0) ? default : array![offset];
        }

        /// <summary>The last row, default when the view is empty. Unlike the method of LINQ, the view is not boxed for it.</summary>
        public T? LastOrDefault()
        {
            return (count == 0) ? default : array![offset + count - 1];
        }

        /// <summary>The rows as a span over the array of the index; nothing is copied.</summary>
        public ReadOnlySpan<T> AsSpan()
        {
            return new ReadOnlySpan<T>(array, offset, count);
        }

        public T[] ToArray()
        {
            if (count == 0) return Array.Empty<T>();

            var result = new T[count];
            Array.Copy(array!, offset, result, 0, count);
            return result;
        }

        /// <summary>
        /// The rows as a list, for what takes an interface: LINQ, a method with an IEnumerable&lt;T&gt; parameter. It
        /// allocates one object (none for an empty view) and copies no row. Nothing can be changed through the list.
        /// </summary>
        public IReadOnlyList<T> AsReadOnlyList()
        {
            return (count == 0) ? Internal.RangeViewList<T>.Empty : new Internal.RangeViewList<T>(this, reverse: false);
        }

        /// <summary>Enumerates without allocating: foreach binds to this struct.</summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(array, offset, count);
        }


        public struct Enumerator : IEnumerator<T>
        {
            readonly T[]? array;
            readonly int start; // one before the first element
            readonly int end;   // one after the last element
            int index;

            internal Enumerator(T[]? array, int offset, int count)
            {
                this.array = array;
                this.start = offset - 1;
                this.end = offset + count;
                this.index = start;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => array![index];
            }

            object? IEnumerator.Current => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var next = index + 1;
                if (next < end)
                {
                    index = next;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                index = start;
            }

            public void Dispose()
            {
            }
        }

        public int IndexOf(T item)
        {
            if (count == 0) return -1;

            var found = Array.IndexOf(array!, item, offset, count);
            return (found < 0) ? -1 : found - offset;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (count == 0)
            {
                return; // an empty view may have no array at all (RangeView<T>.Empty)
            }

            Array.Copy(this.array!, offset, array, arrayIndex, count);
        }

        static void ThrowIndexOutOfRange()
        {
            throw new ArgumentOutOfRangeException("index");
        }

    }

    // What the debugger shows for a view: its rows, not the whole array of the index and two numbers.
    internal sealed class RangeViewDebugView<T>
    {
        readonly T[] items;

        public RangeViewDebugView(RangeView<T> view)
        {
            this.items = view.ToArray();
        }

        public RangeViewDebugView(ReverseRangeView<T> view)
        {
            this.items = view.ToArray();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => items;
    }
}
