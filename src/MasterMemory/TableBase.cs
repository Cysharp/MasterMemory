using MasterMemory.Internal;
using MasterMemory.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MasterMemory
{
    public abstract class TableBase<TElement>
    {
        protected readonly TElement[] data;

        /// <summary>
        /// The row count up to which the generated primary key lookup calls <see cref="FindIndexByBranchlessSearch"/>,
        /// and above which it calls <see cref="FindIndexByBinarySearch"/>.
        /// Measured on x64 (.NET 10): 2.8 times faster than the jumping search at 100 rows, twice at 10,000 and at
        /// 30,000, 1.3 times at 100,000, and two to three times slower from 300,000 rows on, where the jumps let the
        /// processor load the next row ahead of time. The switch is set well below that crossing, since a device with
        /// smaller caches reaches it earlier.
        /// </summary>
        protected const int BranchlessSearchMaxLength = 32768;

        // Common Properties
        public int Count => data.Length;
        public RangeView<TElement> All => new RangeView<TElement>(data);
        public ReverseRangeView<TElement> AllReverse => new RangeView<TElement>(data).Reverse;
        public TElement[] GetRawDataUnsafe() => data;

        public TableBase(TElement[] sortedData)
        {
            this.data = sortedData;
        }

        // Validate

        /// <summary>
        /// Reports every row whose key another row has already. Rows of one key lie next to each other in the index of
        /// that key, and what one key is, is decided by the comparison the index is sorted and searched by: under
        /// OrdinalIgnoreCase "abc" and "ABC" are one key, and a lookup finds only one of the two rows.
        /// </summary>
        static protected void ValidateUniqueCore<TKey>(TElement[] indexArray, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, string message, ValidateResult resultSet)
        {
            if (indexArray.Length == 0) return;

            var previous = keySelector(indexArray[0]);
            for (int i = 1; i < indexArray.Length; i++)
            {
                var item = indexArray[i];
                var v = keySelector(item);
                if (comparer.Compare(previous, v) == 0)
                {
                    resultSet.AddFail(typeof(TElement), "Unique failed: " + message + ", value = " + v, item!);
                }
                previous = v;
            }
        }

        // Util

        /// <summary>
        /// A secondary index: the rows of the table in a new array, in the order of another key. Rows that share that
        /// key are in the order they have in the table, the order of the primary key.
        /// </summary>
        /// <param name="sortBuffer">
        /// What the sort works in. The constructor of a table declares one, null, and hands it to the sort of each of
        /// its indexes: the first makes the buffer and the others use it again.
        /// </param>
        protected TElement[] CloneAndSortBy<TKey, TComparer>(Func<TElement, TKey> indexSelector, TComparer comparer, ref int[]? sortBuffer)
            where TComparer : IComparer<TKey>
        {
            return CloneAndSortBy(data, indexSelector, comparer, ref sortBuffer);
        }

        /// <summary>
        /// The rows in a new array, in the order of a key; rows that share the key in the order they are in
        /// <paramref name="rows"/>.
        /// </summary>
        static protected TElement[] CloneAndSortBy<TKey, TComparer>(TElement[] rows, Func<TElement, TKey> keySelector, TComparer comparer)
            where TComparer : IComparer<TKey>
        {
            int[]? sortBuffer = null;
            return CloneAndSortBy(rows, keySelector, comparer, ref sortBuffer);
        }

        static TElement[] CloneAndSortBy<TKey, TComparer>(TElement[] rows, Func<TElement, TKey> keySelector, TComparer comparer, ref int[]? sortBuffer)
            where TComparer : IComparer<TKey>
        {
            var keys = new TKey[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                keys[i] = keySelector(rows[i]);
            }

            return StableSort.Sort(rows, keys, comparer, ref sortBuffer);
        }

        static protected TElement ThrowKeyNotFound<TKey>(TKey key)
        {
            throw new KeyNotFoundException("DataType: " + typeof(TElement).FullName + ", Key: " + key?.ToString());
        }

        // Unique, by a key that compares itself to a row (the generated code declares one struct per such key)

        /// <summary>
        /// The index of the row <paramref name="comparable"/> compares equal to, negative when there is none.
        /// A binary search with a conditional jump per step: the better choice for a large table.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // a forwarding call: the search itself is not inlined
        static protected int FindIndexByBinarySearch<TComparable>(TElement[] indexArray, TComparable comparable)
            where TComparable : struct, IComparable<TElement>
        {
            return BinarySearch.Find(indexArray, comparable);
        }

        /// <summary>
        /// The index of the row <paramref name="comparable"/> compares equal to, negative when there is none.
        /// A search that halves its range without a conditional jump, at every size: the better choice up to
        /// <see cref="BranchlessSearchMaxLength"/> rows, and the caller's to make.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // a forwarding call: the search itself is not inlined
        static protected int FindIndexByBranchlessSearch<TComparable>(TElement[] indexArray, TComparable comparable)
            where TComparable : struct, IBranchlessComparable<TElement>
        {
            return BinarySearch.FindBranchless(indexArray, comparable);
        }

        /// <summary>
        /// The row equal to the key, or the nearest one below (<paramref name="selectLower"/>) or above it; default
        /// when there is no row on that side: the table is empty, the key is before every row and the lower one was
        /// asked for, or it is after every row and the upper one was.
        /// </summary>
        static protected TElement? FindUniqueClosestCore<TComparable>(TElement[] indexArray, TComparable comparable, bool selectLower)
            where TComparable : struct, IComparable<TElement>
        {
            // FindClosest answers -1 below the first row and indexArray.Length above the last
            var index = BinarySearch.FindClosest(indexArray, comparable, selectLower);
            return ((uint)index < (uint)indexArray.Length) ? indexArray[index] : default(TElement);
        }

        /// <summary>
        /// The rows from the first at or after <paramref name="min"/> to the last at or before <paramref name="max"/>,
        /// in the order of the index; its Reverse is the same rows the other way.
        /// </summary>
        static protected RangeView<TElement> FindUniqueRangeCore<TComparable>(TElement[] indexArray, TComparable min, TComparable max)
            where TComparable : struct, IComparable<TElement>
        {
            var lo = BinarySearch.FindClosest(indexArray, min, false);
            var hi = BinarySearch.FindClosest(indexArray, max, true);

            if ( lo == -1 ) lo = 0;
            if ( hi == indexArray.Length ) hi -= 1;

            return RangeView<TElement>.FromBounds(indexArray, lo, hi);
        }

        // Many

        static protected RangeView<TElement> FindManyCore<TComparable>(TElement[] indexKeys, TComparable comparable)
            where TComparable : struct, IComparable<TElement>
        {
            var lo = BinarySearch.LowerBound(indexKeys, comparable);
            if (lo == -1) return RangeView<TElement>.Empty;

            var hi = BinarySearch.UpperBound(indexKeys, comparable);
            if (hi == -1) return RangeView<TElement>.Empty;

            return RangeView<TElement>.FromBounds(indexKeys, lo, hi);
        }

        /// <summary>
        /// The index <see cref="FindUniqueClosestCore"/> reads its row at. For a key several rows share, the generated
        /// code takes the key of the row found here and asks <see cref="FindManyCore"/> for all of its rows.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // a forwarding call: the search itself is not inlined
        static protected int FindClosestIndex<TComparable>(TElement[] indexArray, TComparable comparable, bool selectLower)
            where TComparable : struct, IComparable<TElement>
        {
            return BinarySearch.FindClosest(indexArray, comparable, selectLower);
        }

        /// <summary>
        /// The rows from the first at or after <paramref name="min"/> to the last at or before <paramref name="max"/>;
        /// empty when min is after max, since the last row at or before max then lies before the first at or after min.
        /// </summary>
        static protected RangeView<TElement> FindManyRangeCore<TComparable>(TElement[] indexArray, TComparable min, TComparable max)
            where TComparable : struct, IComparable<TElement>
        {
            //... want lo to be the lowest  index of the values >= than min.
            //... lo should be in the range [0,arraylength]

            var lo = BinarySearch.LowerBoundClosest(indexArray, min);

            //... want hi to be the highest index of the values <= than max
            //... hi should be in the range [-1,arraylength-1]

            var hi = BinarySearch.UpperBoundClosest(indexArray, max);

            Debug.Assert( lo >= 0 );
            Debug.Assert( hi < indexArray.Length );

            if ( hi < lo )
               return RangeView<TElement>.Empty;

            return RangeView<TElement>.FromBounds(indexArray, lo, hi);
        }
    }
}
