using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MasterMemory
{
    /// <summary>
    /// The rows of a <see cref="RangeView{T}"/> from the last to the first: what <see cref="RangeView{T}.Reverse"/>
    /// and AllReverse return. It copies nothing either. There is no span of it, since a span runs one way only;
    /// <see cref="Reverse"/> gives the view in the order of the index back, which has one.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(RangeViewDebugView<>))]
    public readonly struct ReverseRangeView<T>
    {
        public static ReverseRangeView<T> Empty => default;

        // the same three as the view this is the reverse of
        readonly T[]? array;
        readonly int offset;
        readonly int count;

        internal ReverseRangeView(T[]? array, int offset, int count)
        {
            this.array = array;
            this.offset = offset;
            this.count = count;
        }

        public int Count => count;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)count) ThrowIndexOutOfRange();
                return array![offset + count - 1 - index];
            }
        }

        /// <summary>The first row, which is the last of the index; throws when the view is empty, see <see cref="FirstOrDefault"/>.</summary>
        public T First => this[0];

        /// <summary>The last row, which is the first of the index; throws when the view is empty, see <see cref="LastOrDefault"/>.</summary>
        public T Last => this[count - 1];

        /// <summary>The same rows in the order of the index.</summary>
        public RangeView<T> Reverse => (array == null) ? default : new RangeView<T>(array, offset, count);

        public bool Any()
        {
            return count != 0;
        }

        /// <summary>The first row, default when the view is empty. Unlike the method of LINQ, the view is not boxed for it.</summary>
        public T? FirstOrDefault()
        {
            return (count == 0) ? default : array![offset + count - 1];
        }

        /// <summary>The last row, default when the view is empty. Unlike the method of LINQ, the view is not boxed for it.</summary>
        public T? LastOrDefault()
        {
            return (count == 0) ? default : array![offset];
        }

        public T[] ToArray()
        {
            if (count == 0) return Array.Empty<T>();

            var result = new T[count];
            Array.Copy(array!, offset, result, 0, count);
            Array.Reverse(result);
            return result;
        }

        /// <inheritdoc cref="RangeView{T}.AsReadOnlyList"/>
        public IReadOnlyList<T> AsReadOnlyList()
        {
            return (count == 0) ? Internal.RangeViewList<T>.Empty : new Internal.RangeViewList<T>(Reverse, reverse: true);
        }

        /// <summary>Enumerates without allocating: foreach binds to this struct.</summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(array, offset, count);
        }


        public struct Enumerator : IEnumerator<T>
        {
            readonly T[]? array;
            readonly int start; // one after the element that comes first, the last of the index
            readonly int end;   // the element that comes last, the first of the index
            int index;

            internal Enumerator(T[]? array, int offset, int count)
            {
                this.array = array;
                this.start = offset + count;
                this.end = offset;
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
                var next = index - 1;
                if (next >= end)
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

            // the first in this order is the last in the array
            var last = offset + count - 1;
            var found = Array.LastIndexOf(array!, item, last, count);
            return (found < 0) ? -1 : last - found;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (count == 0)
            {
                return; // an empty view may have no array at all (ReverseRangeView<T>.Empty)
            }

            Array.Copy(this.array!, offset, array, arrayIndex, count);
            Array.Reverse(array, arrayIndex, count);
        }

        static void ThrowIndexOutOfRange()
        {
            throw new ArgumentOutOfRangeException("index");
        }

    }
}
