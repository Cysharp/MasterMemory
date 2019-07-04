using MasterMemory.Internal;
using System;
using System.Collections.Generic;

namespace MasterMemory
{
    public abstract class TableBase<TElement>
    {
        protected readonly TElement[] data;

        // Common Properties
        public int Count => data.Length;
        public RangeView<TElement> All => new RangeView<TElement>(data, 0, data.Length, true);
        public RangeView<TElement> AllReverse => new RangeView<TElement>(data, 0, data.Length, false);
        public TElement[] GetRawDataUnsafe() => data;

        public TableBase(TElement[] sortedData)
        {
            this.data = sortedData;
        }

        // Unique

        protected TElement FindUniqueCore<TIndex, TKey>(TIndex[] indexArray, Func<TIndex, TKey> keySelector, TKey key)
        {
            var index = BinarySearch.FindFirst(indexArray, key, keySelector, Comparer<TKey>.Default);
            return (index != -1) ? data[index] : default(TElement);
        }

        protected TElement FindUniqueClosestCore<TIndex, TKey>(TIndex[] indexArray, Func<TIndex, TKey> keySelector, TKey key, bool selectLower)
        {
            var index = BinarySearch.FindClosest(indexArray, 0, indexArray.Length, key, keySelector, Comparer<TKey>.Default, selectLower);
            return (index != -1) ? data[index] : default(TElement);
        }

        protected RangeView<TElement> FindUniqueRangeCore<TIndex, TKey>(TIndex[] indexArray, Func<TIndex, TKey> keySelector, TKey min, TKey max, bool ascendant)
        {
            var lo = BinarySearch.FindClosest(indexArray, 0, indexArray.Length, min, keySelector, Comparer<TKey>.Default, false);
            var hi = BinarySearch.FindClosest(indexArray, 0, indexArray.Length, max, keySelector, Comparer<TKey>.Default, true);
            return new RangeView<TElement>(data, lo, hi, ascendant);
        }

        // Many

        protected RangeView<TElement> FindManyCore<TIndex, TKey>(TIndex[] indexArray, Func<TIndex, TKey> keySelector, TKey key, bool ascendant)
        {
            var lo = BinarySearch.LowerBound(indexArray, 0, indexArray.Length, key, keySelector, Comparer<TKey>.Default);
            if (lo == -1) return RangeView<TElement>.Empty;

            var hi = BinarySearch.UpperBound(indexArray, 0, indexArray.Length, key, keySelector, Comparer<TKey>.Default);
            if (hi == -1) return RangeView<TElement>.Empty;

            return new RangeView<TElement>(data, lo, hi, ascendant);
        }

        protected RangeView<TElement> FindManyClosestCore<TIndex, TKey>(TIndex[] indexArray, Func<TIndex, TKey> keySelector, TKey key, bool selectLower, bool ascendant)
        {
            var closest = BinarySearch.FindClosest(indexArray, 0, indexArray.Length, key, keySelector, Comparer<TKey>.Default, selectLower);
            if (closest == -1) return RangeView<TElement>.Empty;

            return FindManyCore(indexArray, keySelector, keySelector(indexArray[closest]), ascendant);
        }

        protected RangeView<TElement> FindManyRangeCore<TIndex, TKey>(TIndex[] indexArray, Func<TIndex, TKey> keySelector, TKey min, TKey max, bool ascendant)
        {
            var lo = FindManyClosestCore(indexArray, keySelector, min, false, true).FirstIndex;
            var hi = FindManyClosestCore(indexArray, keySelector, min, true, true).LastIndex;
            return new RangeView<TElement>(data, lo, hi, ascendant);
        }
    }
}
