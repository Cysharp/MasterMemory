using System;
using System.Collections.Generic;

namespace MasterMemory.Internal
{
    internal static class BinarySearch
    {
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

            var originalHi = hi;
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

            if (selectLower)
            {
                return (lo < 0) ? 0 : lo;
            }
            else
            {
                return (originalHi <= hi) ? originalHi - 1 : hi;
            }
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
    }
}