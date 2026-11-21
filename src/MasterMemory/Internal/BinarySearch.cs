using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MasterMemory.Internal
{
    internal static class BinarySearch
    {
        // The two searches below follow SpanHelpers.BinarySearch of the BCL: the key is a struct that compares itself
        // to an element, so the comparison is compiled into the loop for each key type (no delegate, no IComparer),
        // and elements are read through a reference to the first one, so no read is bounds-checked. Both loops keep
        // every index they read inside [0, array.Length).

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ref T GetReference<T>(T[] array)
        {
#if NET5_0_OR_GREATER
            return ref MemoryMarshal.GetArrayDataReference(array);
#else
            return ref MemoryMarshal.GetReference(new Span<T>(array));
#endif
        }

        /// <summary>
        /// SpanHelpers.BinarySearch. Returns the index of an element <paramref name="comparable"/> compares equal to,
        /// or the complement of the index it would be inserted at, a negative number.
        /// </summary>
        public static int Find<T, TComparable>(T[] array, TComparable comparable)
            where TComparable : struct, IComparable<T>
        {
            ref var start = ref GetReference(array);

            var lo = 0;
            var hi = array.Length - 1;
            // If length == 0, hi == -1, and loop will not be entered
            while (lo <= hi)
            {
                // lo and hi are never negative inside the loop, so the median computed in uint cannot overflow
                var i = (int)(((uint)hi + (uint)lo) >> 1);

                var c = comparable.CompareTo(Unsafe.Add(ref start, i));
                if (c == 0)
                {
                    return i;
                }
                else if (c > 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }
            // If none found, then a negative number that is the bitwise complement
            // of the index of the next element that is larger than or, if there is
            // no larger element, the bitwise complement of `length`, which
            // is `lo` at this point.
            return ~lo;
        }

        /// <summary>
        /// The same search without a conditional jump in the loop: the range halves every step, and whether the upper
        /// half is taken is a mask the key computes, not a branch. Returns the index of the equal element, or -1.
        /// A table small enough to stay in the processor's caches spends its lookup on the mispredicted jump of
        /// each step, which this avoids; on a large table the jumps are the better choice, see
        /// <see cref="TableBase{TElement}.BranchlessSearchMaxLength"/>.
        /// </summary>
        public static int FindBranchless<T, TComparable>(T[] array, TComparable comparable)
            where TComparable : struct, IBranchlessComparable<T>
        {
            var length = array.Length;
            if (length == 0)
            {
                return -1;
            }

            ref var start = ref GetReference(array);

            // array[index] is the last element known to be <= the key, once any element is; index + half < array.Length
            var index = 0;
            while (length > 1)
            {
                var half = length >> 1;
                index += comparable.AtOrAfterMask(Unsafe.Add(ref start, index + half)) & half; // move up to the probed element when it is <= the key
                length -= half;
            }

            return (comparable.CompareTo(Unsafe.Add(ref start, index)) == 0) ? index : -1;
        }

        public static int FindFirst<T, TKey>(T[] array, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            var lo = 0;
            var hi = array.Length - 1;

            while (lo <= hi)
            {
                var mid = (int)(((uint)hi + (uint)lo) >> 1);
                var found = comparer.Compare(selector(array[mid]), key);

                if (found == 0) return mid;
                if (found < 0)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return -1;
        }

        // The searches below are the ones a table answers its non-unique, closest and range lookups with. Each reads
        // only indexes inside [0, array.Length): mid lies strictly between its bounds, and the index looked at after a
        // loop is checked against the length first.

        /// <summary>
        /// The index of an element equal to the key if there is one; otherwise the nearest index below
        /// (<paramref name="selectLower"/>, -1 when the key is before every element) or above (array.Length when it is
        /// after every element). -1 for an empty array.
        /// </summary>
        public static int FindClosest<T, TComparable>(T[] array, TComparable comparable, bool selectLower)
            where TComparable : struct, IComparable<T>
        {
            if (array.Length == 0) return -1;

            ref var start = ref GetReference(array);

            var lo = -1;
            var hi = array.Length;

            while (hi - lo > 1)
            {
                var mid = lo + ((hi - lo) >> 1);
                var c = comparable.CompareTo(Unsafe.Add(ref start, mid));

                if (c == 0)
                {
                    lo = hi = mid;
                    break;
                }
                if (c < 0)
                {
                    hi = mid; // the key is before array[mid]
                }
                else
                {
                    lo = mid;
                }
            }

            return selectLower ? lo : hi;
        }

        /// <summary>The first index of the elements equal to the key, -1 when there is none.</summary>
        public static int LowerBound<T, TComparable>(T[] array, TComparable comparable)
            where TComparable : struct, IComparable<T>
        {
            ref var start = ref GetReference(array);

            var lo = 0;
            var hi = array.Length;

            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var c = comparable.CompareTo(Unsafe.Add(ref start, mid));

                if (c <= 0)
                {
                    hi = mid;
                }
                else
                {
                    lo = mid + 1;
                }
            }

            var index = lo;
            if (array.Length <= index)
            {
                return -1;
            }

            // check final
            return (comparable.CompareTo(Unsafe.Add(ref start, index)) == 0)
                ? index
                : -1;
        }

        /// <summary>The last index of the elements equal to the key, -1 when there is none.</summary>
        public static int UpperBound<T, TComparable>(T[] array, TComparable comparable)
            where TComparable : struct, IComparable<T>
        {
            ref var start = ref GetReference(array);

            var lo = 0;
            var hi = array.Length;

            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var c = comparable.CompareTo(Unsafe.Add(ref start, mid));

                if (c >= 0)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }

            var index = (lo == 0) ? 0 : lo - 1;
            if (array.Length <= index)
            {
                return -1;
            }

            // check final
            return (comparable.CompareTo(Unsafe.Add(ref start, index)) == 0)
                ? index
                : -1;
        }

        /// <summary>
        /// The lowest index whose element is at or after the key: 0 when the key is at or before every element,
        /// array.Length when it is after every element.
        /// </summary>
        public static int LowerBoundClosest<T, TComparable>(T[] array, TComparable comparable)
            where TComparable : struct, IComparable<T>
        {
            ref var start = ref GetReference(array);

            var lo = 0;
            var hi = array.Length;

            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var c = comparable.CompareTo(Unsafe.Add(ref start, mid));

                if (c <= 0) // the key is at or before array[mid]
                {
                    hi = mid;
                }
                else
                {
                    lo = mid + 1; // lo starts at zero and can only increase
                }
            }

            var index = lo; // zero or greater

            if (array.Length <= index)
            {
                return array.Length;
            }

            // check final
            return (comparable.CompareTo(Unsafe.Add(ref start, index)) <= 0)
                ? index
                : -1;
        }

        /// <summary>
        /// The highest index whose element is at or before the key: -1 when the key is before every element (an empty
        /// array included), array.Length - 1 when it is at or after every element.
        /// </summary>
        public static int UpperBoundClosest<T, TComparable>(T[] array, TComparable comparable)
            where TComparable : struct, IComparable<T>
        {
            ref var start = ref GetReference(array);

            var lo = 0;
            var hi = array.Length;

            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var c = comparable.CompareTo(Unsafe.Add(ref start, mid));

                if (c >= 0) // the key is at or after array[mid]
                {
                    lo = mid + 1; // lo starts at zero and can only increase
                }
                else
                {
                    hi = mid;
                }
            }

            var index = (lo == 0) ? 0 : lo - 1; // zero or greater

            if (index >= array.Length)
            {
                return -1; // only an empty array gets here
            }

            // check final
            return (comparable.CompareTo(Unsafe.Add(ref start, index)) >= 0)
                ? index
                : -1;
        }
    }
}
