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

        public static int FindFirstIntKey<T>(T[] array, int key, Func<T, int> selector)
        {
            var lo = 0;
            var hi = array.Length - 1;

            while (lo <= hi)
            {
                var mid = (int)(((uint)hi + (uint)lo) >> 1);
                // compare inlining
                var selectedValue = selector(array[mid]);
                var found = (selectedValue < key) ? -1 : (selectedValue > key) ? 1 : 0;

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

        // lo = 0, hi = Count.
        public static int FindClosest<T, TKey>(T[] array, int lo, int hi, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer, bool selectLower)
        {
            if (array.Length == 0) return -1;

            lo = lo - 1;

            while (hi - lo > 1)
            {
                var mid = lo + ((hi - lo) >> 1);
                var found = comparer.Compare(selector(array[mid]), key);

                if (found == 0)
                {
                    lo = hi = mid;
                    break;
                }
                if (found >= 1)
                {
                    hi = mid;
                }
                else
                {
                    lo = mid;
                }
            }

            return selectLower ? lo : hi;
        }

        // default lo = 0, hi = array.Count
        public static int LowerBound<T, TKey>(T[] array, int lo, int hi, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var found = comparer.Compare(key, selector(array[mid]));

                if (found <= 0)
                {
                    hi = mid;
                }
                else
                {
                    lo = mid + 1;
                }
            }

            var index = lo;
            if (index == -1 || array.Length <= index)
            {
                return -1;
            }

            // check final
            return (comparer.Compare(key, selector(array[index])) == 0)
                ? index
                : -1;
        }

        public static int UpperBound<T, TKey>(T[] array, int lo, int hi, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var found = comparer.Compare(key, selector(array[mid]));

                if (found >= 0)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }

            var index = (lo == 0) ? 0 : lo - 1;
            if (index == -1 || array.Length <= index)
            {
                return -1;
            }

            // check final
            return (comparer.Compare(key, selector(array[index])) == 0)
                ? index
                : -1;
        }


        //... want the lowest index of  Key <= Value
        //... returns 0 if key is <= all values in array
        //... returns array.Length if key is > all values in array

        public static int LowerBoundClosest<T, TKey>(T[] array, int lo, int hi, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var found = comparer.Compare(key, selector(array[mid]));

                if (found <= 0)     //... Key is <= value at mid
                {
                    hi = mid;
                }
                else
                {
                    lo = mid + 1;   //... Notice that lo starts at zero and can only increase
                }
            }

            var index = lo;         //... index will always be zero or greater

            if ( array.Length <= index)
            {
               return array.Length;
            }

            // check final
            return (comparer.Compare(key, selector(array[index])) <= 0)
                ? index
                : -1;
        }

 
        //... want the highest index of  Key >= Value
        //... returns -1 if key is < than all values in array
        //... returns array.Length - 1 if key is >= than all values in array

        public static int UpperBoundClosest<T, TKey>(T[] array, int lo, int hi, TKey key, Func<T, TKey> selector, IComparer<TKey> comparer)
        {
            while (lo < hi)
            {
                var mid = lo + ((hi - lo) >> 1);
                var found = comparer.Compare(key, selector(array[mid]));

                if (found >= 0)     //... Key >= value at mid
                {
                    lo = mid + 1;   //... Note lo starts at zero and can only increase
                }
                else
                {
                    hi = mid;
                }
            }

            var index = (lo == 0) ? 0 : lo - 1;   //... index will always be zero or greater

            if ( index >= array.Length )
            {
               return array.Length;
            }

            // check final
            return (comparer.Compare(key, selector(array[index])) >= 0)
                ? index
                : -1;
        }



    }
}