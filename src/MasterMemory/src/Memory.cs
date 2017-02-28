using MasterMemory.Internal;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterMemory
{
    public interface IInternalMemory
    {
        int Serialize(ref byte[] bytes, int offset, IFormatterResolver resolver);
    }

    internal class InternalRawMemory : IInternalMemory
    {
        public readonly byte[] RawMemory;

        public InternalRawMemory(byte[] bytes)
        {
            this.RawMemory = bytes;
        }

        int IInternalMemory.Serialize(ref byte[] bytes, int offset, IFormatterResolver resolver)
        {
            MessagePackBinary.EnsureCapacity(ref bytes, offset, RawMemory.Length);
            Buffer.BlockCopy(RawMemory, 0, bytes, offset, RawMemory.Length);
            return RawMemory.Length;
        }
    }

    public interface IMemoryFinder<TKey, TElement>
    {
        int Count { get; }
        bool TryFind(TKey key, out TElement value);
        TElement Find(TKey key);
        TElement FindOrDefault(TKey key, TElement defaultValue = default(TElement));
        TElement FindClosest(TKey key, bool selectLower = true);
        RangeView<TElement> FindMany(TKey key, bool ascendant = true);

        // drop of initial release.
        // RangeView<TElement> FindRange(TKey left, TKey right, bool ascendant = true);

        RangeView<TElement> FindAll(bool ascendant = true);

        ILookup<TKey, TElement> ToLookupView();
#if UNITY_5
        IDictionary<TKey, TElement> ToDictionaryView();
#else
        IReadOnlyDictionary<TKey, TElement> ToDictionaryView();
#endif
    }

    /// <summary>
    /// Represents key indexed memory like Dictionary/ILookup.
    /// </summary>
    public class Memory<TKey, TElement> : IMemoryFinder<TKey, TElement>, IInternalMemory
    {
        readonly object gate = new object();
        Dictionary<string, object> dynamicIndexMemoriesCache;
        readonly bool rootMemory;

        public int Count
        {
            get
            {
                return orderedData.Length;
            }
        }

        readonly TElement[] orderedData;
        readonly Func<TElement, TKey> indexSelector;
        readonly IComparer<TKey>[] comparers;

        public Memory(IEnumerable<TElement> datasource, Func<TElement, TKey> indexSelector)
            : this(datasource, indexSelector, true, false)
        {
        }

        /// <summary>
        /// unsafe internal API.
        /// </summary>
        internal Memory(IEnumerable<TElement> datasource, Func<TElement, TKey> indexSelector, bool rootMemory, bool ordered)
        {
            MemoryKeyComparerRegister.RegisterDynamic<TKey>();

            var comparer = MasterMemoryComparer<TKey>.Default;
            this.comparers = MasterMemoryComparer<TKey>.DefaultArray;
            if (ordered)
            {
                this.orderedData = (TElement[])datasource;
            }
            else
            {
                this.orderedData = FastSort(datasource, indexSelector, comparer);
            }
            this.indexSelector = indexSelector;
            this.rootMemory = rootMemory;
        }

        /// <summary>Get the first(single) value.</summary>
        public bool TryFind(TKey key, out TElement value)
        {
            return InternalTryFind(key, comparers.Length, out value);
        }

        internal bool InternalTryFind(TKey key, int comparerCount, out TElement value)
        {
            if (comparerCount == 1)
            {
                var index = BinarySearch.FindFirst(orderedData, key, indexSelector, comparers[0]);
                if (index == -1)
                {
                    value = default(TElement);
                    return false;
                }
                value = orderedData[index];
                return true;
            }
            else
            {
                var lo = 0;
                var hi = orderedData.Length;
                for (int i = 0; i < comparerCount; i++)
                {
                    var comparer = comparers[i];

                    var newlo = BinarySearch.LowerBound(orderedData, lo, hi, key, indexSelector, comparer);
                    if (newlo == -1 || !(lo <= newlo) && (newlo < hi))
                    {
                        value = default(TElement);
                        return false;
                    }
                    var newhi = BinarySearch.UpperBound(orderedData, lo, hi, key, indexSelector, comparer);
                    if (newhi == -1 || !(lo <= newhi) && (newhi < hi))
                    {
                        value = default(TElement);
                        return false;
                    }
                    lo = newlo;
                    hi = newhi + 1;
                }

                if (lo == -1)
                {
                    value = default(TElement);
                    return false;
                }
                value = orderedData[lo];
                return true;
            }
        }

        /// <summary>Get the first(single) value.</summary>
        public TElement Find(TKey key)
        {
            return InternalFind(key, comparers.Length);
        }

        internal TElement InternalFind(TKey key, int comparerCount)
        {
            TElement value;
            if (InternalTryFind(key, comparerCount, out value))
            {
                return value;
            }
            else
            {
                throw new KeyNotFoundException("Key:" + key);
            }
        }

        /// <summary>Get the first(single) value.</summary>
        public TElement FindOrDefault(TKey key, TElement defaultValue = default(TElement))
        {
            return InternalFindOrDefault(key, defaultValue, comparers.Length);
        }

        internal TElement InternalFindOrDefault(TKey key, TElement defaultValue, int comparerCount)
        {
            TElement value;
            if (InternalTryFind(key, comparerCount, out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get the closest value. e.g. [1,2,6,7,8]: FindNearest(3) -> 2, FindNearest(5) -> 6.
        /// If same distance on both side, follow selectLower argument.
        /// </summary>
        /// <returns></returns>
        public TElement FindClosest(TKey key, bool selectLower = true)
        {
            return InternalFindClosest(key, selectLower, comparers.Length);
        }

        internal TElement InternalFindClosest(TKey key, bool selectLower, int comparerCount)
        {
            var index = FindClosestIndex(key, selectLower, comparerCount);
            return orderedData[index];
        }

        int FindClosestIndex(TKey key, bool selectLower, int comparerCount)
        {
            if (orderedData.Length == 0) throw new ArgumentOutOfRangeException("Empty data is not supported.");

            if (comparerCount == 1)
            {
                var index = BinarySearch.FindClosest(orderedData, 0, orderedData.Length, key, indexSelector, comparers[0], selectLower);
                return index;
            }
            else
            {
                var lo = 0;
                var hi = orderedData.Length;

                for (int i = 0; i < comparerCount; i++)
                {
                    var comparer = comparers[i];

                    if (i == comparerCount - 1)
                    {
                        var index = BinarySearch.FindClosest(orderedData, lo, hi, key, indexSelector, comparer, selectLower);
                        return index;
                    }
                    else
                    {
                        var newlo = BinarySearch.LowerBound(orderedData, lo, hi, key, indexSelector, comparer);
                        if (newlo == -1 || !(lo <= newlo) && (newlo < hi))
                        {
                            newlo = lo;
                        }
                        var newhi = BinarySearch.UpperBound(orderedData, lo, hi, key, indexSelector, comparer);
                        if (newhi == -1 || !(lo <= newhi) && (newhi < hi))
                        {
                            newhi = hi - 1;
                        }

                        lo = newlo;
                        hi = newhi + 1;
                    }
                }

                throw new InvalidProgramException();
            }
        }

        /// <summary>
        /// Get the range value, useful when key is not unique.
        /// </summary>
        public RangeView<TElement> FindMany(TKey key, bool ascendant = true)
        {
            return InternalFindMany(key, ascendant, comparers.Length);
        }

        internal RangeView<TElement> InternalFindMany(TKey key, bool ascendant, int comparerCount)
        {
            if (comparerCount == 1)
            {
                var lo = BinarySearch.LowerBound(orderedData, 0, orderedData.Length, key, indexSelector, comparers[0]);
                if (lo == -1) return RangeView<TElement>.Empty();
                var hi = BinarySearch.UpperBound(orderedData, 0, orderedData.Length, key, indexSelector, comparers[0]);

                return new RangeView<TElement>(orderedData, lo, hi, ascendant);
            }
            else
            {
                var lo = 0;
                var hi = orderedData.Length;
                for (int i = 0; i < comparerCount; i++)
                {
                    var comparer = comparers[i];

                    var newlo = BinarySearch.LowerBound(orderedData, lo, hi, key, indexSelector, comparer);
                    if (newlo == -1 || !(lo <= newlo) && (newlo < hi))
                    {
                        return RangeView<TElement>.Empty();
                    }
                    var newhi = BinarySearch.UpperBound(orderedData, lo, hi, key, indexSelector, comparer);
                    if (newhi == -1 || !(lo <= newhi) && (newhi < hi))
                    {
                        return RangeView<TElement>.Empty();
                    }
                    lo = newlo;
                    hi = newhi + 1;
                }

                return new RangeView<TElement>(orderedData, lo, hi - 1, ascendant);
            }
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return new MasterMemory.RangeView<TElement>(orderedData, 0, orderedData.Length - 1, ascendant);
        }

        public ILookup<TKey, TElement> ToLookupView()
        {
            return new LookupView<TKey, TElement>(this);
        }

#if UNITY_5
        public IDictionary<TKey, TElement> ToDictionaryView()
#else
        public IReadOnlyDictionary<TKey, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView<TKey, TElement>(this);
        }

        public IMemoryFinder<TSecondaryIndex, TElement> SecondaryIndex<TSecondaryIndex>(string indexName, Func<TElement, TSecondaryIndex> secondaryIndexSelector)
        {
            if (!rootMemory) throw new Exception("Can't create dynamic index on secondary indexed memory.");

            object memory;
            lock (gate)
            {
                if (dynamicIndexMemoriesCache == null)
                {
                    dynamicIndexMemoriesCache = new Dictionary<string, object>();
                }

                if (!dynamicIndexMemoriesCache.TryGetValue(indexName, out memory))
                {
                    memory = dynamicIndexMemoriesCache[indexName] = new Memory<TSecondaryIndex, TElement>(this.orderedData, secondaryIndexSelector, false, false);
                }
            }

            return (Memory<TSecondaryIndex, TElement>)memory;
        }

        public override string ToString()
        {
            return "IndexType:" + typeof(TKey).Name + " Count:" + Count;
        }

        // don't use OrderBy for performance reason.
        TElement[] FastSort(IEnumerable<TElement> datasource, Func<TElement, TKey> indexSelector, IComparer<TKey> comparer)
        {
            var collection = datasource as ICollection<TElement>;
            if (collection != null)
            {
                var array = new TElement[collection.Count];
                var sortSource = new TKey[collection.Count];
                var i = 0;
                foreach (var item in collection)
                {
                    array[i] = item;
                    sortSource[i] = indexSelector(item);
                    i++;
                }
                Array.Sort(sortSource, array, 0, collection.Count, comparer);
                return array;
            }
            else
            {
                var array = new ExpandableArray<TElement>();
                var sortSource = new ExpandableArray<TKey>();
                foreach (var item in datasource)
                {
                    array.Add(item);
                    sortSource.Add(indexSelector(item));
                }

                Array.Sort(sortSource.items, array.items, 0, array.count, comparer);

                Array.Resize(ref array.items, array.count);
                return array.items;
            }
        }

        public int Serialize(ref byte[] bytes, int offset, IFormatterResolver resolver)
        {
            return resolver.GetFormatterWithVerify<TElement[]>().Serialize(ref bytes, offset, orderedData, resolver);
        }
    }
}