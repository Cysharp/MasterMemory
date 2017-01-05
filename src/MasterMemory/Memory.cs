using MasterMemory.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using ZeroFormatter;
using ZeroFormatter.Formatters;

namespace MasterMemory
{
    public interface IMemoryFinder<TKey, TElement>
    {
        bool TryFind(TKey key, out TElement value);
        TElement Find(TKey key);
        TElement FindOrDefault(TKey key, TElement defaultValue = default(TElement));
        TElement FindClosest(TKey key, bool selectLower = true);
        RangeView<TElement> FindMany(TKey key, bool ascendant = true);

        // drop of initial release.
        // RangeView<TElement> FindRange(TKey left, TKey right, bool ascendant = true);

        RangeView<TElement> FindAll(bool ascendant = true);

        LookupView<TKey, TElement> ToLookupView();
        DictionaryView<TKey, TElement> ToDictionaryView();
    }

    /// <summary>
    /// Represents key indexed memory like Dictionary/ILookup.
    /// </summary>
    public class Memory<TKey, TElement> : IMemoryFinder<TKey, TElement>
    {
        readonly object gate = new object();
        Dictionary<string, object> dynamicIndexMemoriesCache;
        readonly bool rootMemory;

        public int Count { get; set; }

        readonly IList<TElement> orderedData;
        readonly Func<TElement, TKey> indexSelector;
        readonly IComparer<TKey>[] comparers;

        public Memory(IEnumerable<TElement> datasource, Func<TElement, TKey> indexSelector)
            : this(datasource, indexSelector, true)
        {
        }

        public Memory(IEnumerable<TElement> datasource, Func<TElement, TKey> indexSelector, bool rootMemory)
        {
            IComparer<TKey> comparer;
            if (typeof(TKey).GetInterfaces().Contains(typeof(IKeyTuple)))
            {
                // Type is KeyTuple.
                var args = typeof(TKey).GetGenericArguments();
                var t = KeyTupleComparer.Types[args.Length].MakeGenericType(args);
                comparer = (IComparer<TKey>)Activator.CreateInstance(t, new object[] { -1 });

                this.comparers = new IComparer<TKey>[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    this.comparers[i] = (IComparer<TKey>)Activator.CreateInstance(t, new object[] { (i + 1) });
                }
            }
            else
            {
                comparer = Comparer<TKey>.Default;
                this.comparers = new[] { comparer };
            }

            this.orderedData = FastSort(datasource, indexSelector, comparer);
            this.indexSelector = indexSelector;
            this.rootMemory = rootMemory;
        }

        // TODO
        //internal Memory(string key, ArraySegment<byte> bytes, Func<TElement, TKey> indexSelector)
        //{
        //    // TODO:NullTracker
        //    var array = bytes.Array;
        //    int size;
        //    this.orderedData = ZeroFormatter.Formatters.Formatter<DefaultResolver, IList<TElement>>.Default.Deserialize(ref array, bytes.Offset, new DirtyTracker(), out size);
        //    this.indexSelector = indexSelector;
        //    this.comparers = new[] { Comparer<TKey>.Default };
        //}

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
                var hi = orderedData.Count;
                for (int i = 0; i < comparerCount; i++)
                {
                    var comparer = comparers[i];

                    var newlo = BinarySearch.LowerBound(orderedData, lo, hi, key, indexSelector, comparer);
                    if (newlo == -1)
                    {
                        value = default(TElement);
                        return false;
                    }
                    var newhi = BinarySearch.UpperBound(orderedData, lo, hi, key, indexSelector, comparer);
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
            TElement value;
            if (TryFind(key, out value))
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
            TElement value;
            if (TryFind(key, out value))
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
            if (orderedData.Count == 0) throw new ArgumentOutOfRangeException("Empty data is not supported.");

            if (comparers.Length == 1)
            {
                var index = BinarySearch.FindClosest(orderedData, 0, orderedData.Count, key, indexSelector, comparers[0], selectLower);
                return orderedData[index];
            }
            else
            {
                var lo = 0;
                var hi = orderedData.Count;

                for (int i = 0; i < comparers.Length; i++)
                {
                    var comparer = comparers[i];

                    if (i == comparers.Length - 1)
                    {
                        var index = BinarySearch.FindClosest(orderedData, lo, hi, key, indexSelector, comparer, selectLower);
                        return orderedData[index];
                    }
                    else
                    {
                        var newlo = BinarySearch.LowerBound(orderedData, lo, hi, key, indexSelector, comparer);
                        if (newlo == -1)
                        {
                            newlo = lo;
                        }
                        var newhi = BinarySearch.UpperBound(orderedData, lo, hi, key, indexSelector, comparer);
                        if (newhi == -1)
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
            if (comparers.Length == 1)
            {
                var lo = BinarySearch.LowerBound(orderedData, 0, orderedData.Count, key, indexSelector, comparers[0]);
                if (lo == -1) return RangeView<TElement>.Empty();
                var hi = BinarySearch.UpperBound(orderedData, 0, orderedData.Count, key, indexSelector, comparers[0]);

                return new RangeView<TElement>(orderedData, lo, hi, ascendant);
            }
            else
            {
                var lo = 0;
                var hi = orderedData.Count;
                foreach (var comparer in comparers)
                {
                    var newlo = BinarySearch.LowerBound(orderedData, lo, hi, key, indexSelector, comparer);
                    if (newlo == -1)
                    {
                        return RangeView<TElement>.Empty();
                    }
                    var newhi = BinarySearch.UpperBound(orderedData, lo, hi, key, indexSelector, comparer);
                    lo = newlo;
                    hi = newhi + 1;
                }

                return new RangeView<TElement>(orderedData, lo, hi - 1, ascendant);
            }
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return new MasterMemory.RangeView<TElement>(orderedData, 0, orderedData.Count - 1, ascendant);
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
                    memory = dynamicIndexMemoriesCache[indexName] = new Memory<TSecondaryIndex, TElement>(this.orderedData, secondaryIndexSelector, false);
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

        public LookupView<TKey, TElement> ToLookupView()
        {
            return new LookupView<TKey, TElement>(this);
        }

        public DictionaryView<TKey, TElement> ToDictionaryView()
        {
            return new DictionaryView<TKey, TElement>(this);
        }
    }
}