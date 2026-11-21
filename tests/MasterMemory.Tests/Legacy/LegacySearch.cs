// The searches as they were before they took a struct comparable: a key selector delegate and an IComparer per probe.
// Copied from src/MasterMemory/Internal/BinarySearch.cs and src/MasterMemory/TableBase.cs at 5b42a5a, unchanged apart
// from names and accessibility. The current searches are held to these on every input, exceptions included.
#pragma warning disable
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MasterMemory.Tests.Legacy
{
    internal static class LegacyBinarySearch
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

    internal static class LegacyTable<TElement>
    {
        public static TElement ThrowKeyNotFound<TKey>(TKey key)
        {
            throw new KeyNotFoundException("DataType: " + typeof(TElement).FullName + ", Key: " + key?.ToString());
        }

        public static TElement FindUniqueCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey key, bool throwIfNotFound = true)
        {
            var index = LegacyBinarySearch.FindFirst(indexArray, key, keySelector, comparer);
            if (index != -1)
            {
                return indexArray[index];
            }
            else
            {
                if (throwIfNotFound)
                {
                    ThrowKeyNotFound(key);
                }
                return default!;
            }
        }

        // Optimize for IntKey
        public static TElement FindUniqueCoreInt(TElement[] indexArray, Func<TElement, int> keySelector, IComparer<int> _, int key, bool throwIfNotFound = true)
        {
            var index = LegacyBinarySearch.FindFirstIntKey(indexArray, key, keySelector);
            if (index != -1)
            {
                return indexArray[index];
            }
            else
            {
                if (throwIfNotFound)
                {
                    ThrowKeyNotFound(key);
                }
                return default!;
            }
        }

        public static bool TryFindUniqueCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey key, out TElement result)
        {
            var index = LegacyBinarySearch.FindFirst(indexArray, key, keySelector, comparer);
            if (index != -1)
            {
                result = indexArray[index];
                return true;
            }
            else
            {
                result = default!;
                return false;
            }
        }

        public static bool TryFindUniqueCoreInt(TElement[] indexArray, Func<TElement, int> keySelector, IComparer<int> _, int key, out TElement result)
        {
            var index = LegacyBinarySearch.FindFirstIntKey(indexArray, key, keySelector);
            if (index != -1)
            {
                result = indexArray[index];
                return true;
            }
            else
            {
                result = default!;
                return false;
            }
        }

        public static TElement? FindUniqueClosestCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey key, bool selectLower)
        {
            var index = LegacyBinarySearch.FindClosest(indexArray, 0, indexArray.Length, key, keySelector, comparer, selectLower);
            return (index != -1) ? indexArray[index] : default(TElement);
        }

        public static RangeView<TElement> FindUniqueRangeCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey min, TKey max, bool ascendant)
        {
            var lo = LegacyBinarySearch.FindClosest(indexArray, 0, indexArray.Length, min, keySelector, comparer, false);
            var hi = LegacyBinarySearch.FindClosest(indexArray, 0, indexArray.Length, max, keySelector, comparer, true);

            if ( lo == -1 ) lo = 0;
            if ( hi == indexArray.Length ) hi -= 1;

            return new RangeView<TElement>(indexArray, lo, hi, ascendant);
        }

        // Many

        public static RangeView<TElement> FindManyCore<TKey>(TElement[] indexKeys, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey key)
        {
            var lo = LegacyBinarySearch.LowerBound(indexKeys, 0, indexKeys.Length, key, keySelector, comparer);
            if (lo == -1) return RangeView<TElement>.Empty;

            var hi = LegacyBinarySearch.UpperBound(indexKeys, 0, indexKeys.Length, key, keySelector, comparer);
            if (hi == -1) return RangeView<TElement>.Empty;

            return new RangeView<TElement>(indexKeys, lo, hi, true);
        }

        public static RangeView<TElement> FindManyClosestCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey key, bool selectLower)
        {
            var closest = LegacyBinarySearch.FindClosest(indexArray, 0, indexArray.Length, key, keySelector, comparer, selectLower);

            if ((closest == -1) || ( closest >= indexArray.Length ))
                return RangeView<TElement>.Empty;

            return FindManyCore(indexArray, keySelector, comparer, keySelector(indexArray[closest]));
        }

        public static RangeView<TElement> FindManyRangeCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey min, TKey max, bool ascendant)
        {
            //... Empty set when min > max
            //... Alternatively, could treat this as between and swap min and max.

            if ( Comparer<TKey>.Default.Compare( min, max ) > 0 )
               return RangeView<TElement>.Empty;

            //... want lo to be the lowest  index of the values >= than min.
            //... lo should be in the range [0,arraylength]

            var lo = LegacyBinarySearch.LowerBoundClosest(indexArray, 0, indexArray.Length, min, keySelector, comparer );

            //... want hi to be the highest index of the values <= than max
            //... hi should be in the range [-1,arraylength-1]

            var hi = LegacyBinarySearch.UpperBoundClosest(indexArray, 0, indexArray.Length, max, keySelector, comparer );

            Debug.Assert( lo >= 0 );
            Debug.Assert( hi < indexArray.Length );

            if ( hi < lo )
               return RangeView<TElement>.Empty;

            return new RangeView<TElement>(indexArray, lo, hi, ascendant);
        }
    }
}
