using MasterMemory.Internal;
using MasterMemory.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MasterMemory
{
    public abstract class TableBase<TElement>
    {
        protected readonly TElement[] data;

        // Common Properties
        public int Count => data.Length;
        public RangeView<TElement> All => new RangeView<TElement>(data, 0, data.Length - 1, true);
        public RangeView<TElement> AllReverse => new RangeView<TElement>(data, 0, data.Length - 1, false);
        public TElement[] GetRawDataUnsafe() => data;

        public TableBase(TElement[] sortedData)
        {
            this.data = sortedData;
        }

        // Validate

        static protected void ValidateUniqueCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, string message, ValidateResult resultSet)
        {
            var set = new HashSet<TKey>();
            foreach (var item in indexArray)
            {
                var v = keySelector(item);
                if (!set.Add(v))
                {
                    resultSet.AddFail(typeof(TElement), "Unique failed: " + message + ", value = " + v, item);
                }
            }
        }

        // Util

        protected TElement[] CloneAndSortBy<TKey>(Func<TElement, TKey> indexSelector, IComparer<TKey> comparer)
        {
            var array = new TElement[data.Length];
            var sortSource = new TKey[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                array[i] = data[i];
                sortSource[i] = indexSelector(data[i]);
            }

            Array.Sort(sortSource, array, 0, array.Length, comparer);
            return array;
        }

        static protected TElement ThrowKeyNotFound<TKey>(TKey key)
        {
            throw new KeyNotFoundException("DataType: " + typeof(TElement).FullName + ", Key: " + key.ToString());
        }

        // Unique

        static protected TElement FindUniqueCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey key, bool throwIfNotFound = true)
        {
            var index = BinarySearch.FindFirst(indexArray, key, keySelector, comparer);
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
                return default;
            }
        }

        // Optimize for IntKey
        static protected TElement FindUniqueCoreInt(TElement[] indexArray, Func<TElement, int> keySelector, IComparer<int> _, int key, bool throwIfNotFound = true)
        {
            var index = BinarySearch.FindFirstIntKey(indexArray, key, keySelector);
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
                return default;
            }
        }

        static protected bool TryFindUniqueCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey key, out TElement result)
        {
            var index = BinarySearch.FindFirst(indexArray, key, keySelector, comparer);
            if (index != -1)
            {
                result = indexArray[index];
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        static protected bool TryFindUniqueCoreInt(TElement[] indexArray, Func<TElement, int> keySelector, IComparer<int> _, int key, out TElement result)
        {
            var index = BinarySearch.FindFirstIntKey(indexArray, key, keySelector);
            if (index != -1)
            {
                result = indexArray[index];
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        static protected TElement FindUniqueClosestCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey key, bool selectLower)
        {
            var index = BinarySearch.FindClosest(indexArray, 0, indexArray.Length, key, keySelector, comparer, selectLower);
            return (index != -1) ? indexArray[index] : default(TElement);
        }

        static protected RangeView<TElement> FindUniqueRangeCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey min, TKey max, bool ascendant)
        {
            var lo = BinarySearch.FindClosest(indexArray, 0, indexArray.Length, min, keySelector, comparer, false);
            var hi = BinarySearch.FindClosest(indexArray, 0, indexArray.Length, max, keySelector, comparer, true);

            if ( lo == -1 ) lo = 0;
            if ( hi == indexArray.Length ) hi -= 1;

            return new RangeView<TElement>(indexArray, lo, hi, ascendant);
        }

        // Many

        static protected RangeView<TElement> FindManyCore<TKey>(TElement[] indexKeys, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey key)
        {
            var lo = BinarySearch.LowerBound(indexKeys, 0, indexKeys.Length, key, keySelector, comparer);
            if (lo == -1) return RangeView<TElement>.Empty;

            var hi = BinarySearch.UpperBound(indexKeys, 0, indexKeys.Length, key, keySelector, comparer);
            if (hi == -1) return RangeView<TElement>.Empty;

            return new RangeView<TElement>(indexKeys, lo, hi, true);
        }

        static protected RangeView<TElement> FindManyClosestCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey key, bool selectLower)
        {
            var closest = BinarySearch.FindClosest(indexArray, 0, indexArray.Length, key, keySelector, comparer, selectLower);

            if ((closest == -1) || ( closest >= indexArray.Length ))
                return RangeView<TElement>.Empty;

            return FindManyCore(indexArray, keySelector, comparer, keySelector(indexArray[closest]));
        }

        static protected RangeView<TElement> FindManyRangeCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, TKey min, TKey max, bool ascendant)
        {
            //... Empty set when min > max
            //... Alternatively, could treat this as between and swap min and max.

            if ( Comparer<TKey>.Default.Compare( min, max ) > 0 )
               return RangeView<TElement>.Empty;

            //... want lo to be the lowest  index of the values >= than min.
            //... lo should be in the range [0,arraylength]

            var lo = BinarySearch.LowerBoundClosest(indexArray, 0, indexArray.Length, min, keySelector, comparer );

            //... want hi to be the highest index of the values <= than max
            //... hi should be in the range [-1,arraylength-1]

            var hi = BinarySearch.UpperBoundClosest(indexArray, 0, indexArray.Length, max, keySelector, comparer );

            Debug.Assert( lo >= 0 );
            Debug.Assert( hi < indexArray.Length );

            if ( hi < lo )
               return RangeView<TElement>.Empty;

            return new RangeView<TElement>(indexArray, lo, hi, ascendant);
        }
    }
}
