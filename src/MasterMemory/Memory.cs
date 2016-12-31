using MasterMemory.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;
using ZeroFormatter.Formatters;

namespace MasterMemory
{
    public enum SelectSide
    {
        Lower, Upper
    }

    /// <summary>
    /// Represents primary indexed memory.
    /// </summary>
    public sealed class Memory<T, TKey>
    {
        readonly object gate = new object();
        Dictionary<string, object> dynamicIndexMemoriesCache;

        // don't serialize, only for debug view.
        public string Key { get; private set; }

        public int Count { get; set; }

        // ZeroFormatter delayed list.
        readonly IList<T> orderedData;
        readonly Func<T, TKey> indexSelector;
        readonly IComparer<TKey>[] comparers;

        // TODO:internal


        public Memory(string key, IEnumerable<T> datasource, Func<T, TKey> indexSelector)
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
        }

        public Memory(string key, ArraySegment<byte> bytes, Func<T, TKey> indexSelector)
        {
            // TODO:NullTracker
            var array = bytes.Array;
            int size;
            this.orderedData = ZeroFormatter.Formatters.Formatter<DefaultResolver, IList<T>>.Default.Deserialize(ref array, bytes.Offset, new DirtyTracker(), out size);
            this.indexSelector = indexSelector;
            this.comparers = new[] { Comparer<TKey>.Default };
        }

        // get the first found value.
        int FindIndex(int lower, int upper, TKey key, IComparer<TKey> comparer)
        {
            while (upper - lower > 1)
            {
                var index = lower + ((upper - lower) / 2);
                var compare = comparer.Compare(indexSelector(orderedData[index]), key);
                if (compare == 0)
                {
                    lower = upper = index;
                    return lower;
                }
                if (compare >= 1)
                {
                    upper = index;
                }
                else
                {
                    lower = index;
                }
            }

            return -1;
        }

        int FindLeftIndex(int lower, int upper, TKey key, IComparer<TKey> comparer)
        {
            while (true)
            {
                var index = FindIndex(lower, upper, key, comparer);
                if (index == -1) return -1;
                if (index == 0) return 0;

                var l = orderedData[index - 1];
                if (comparer.Compare(indexSelector(l), key) == 0)
                {
                    return index;
                }
                else
                {
                    upper = index; // search again.
                }
            }
        }

        int FindRightIndex(int lower, int upper, TKey key, IComparer<TKey> comparer)
        {
            while (true)
            {
                var index = FindIndex(lower, upper, key, comparer);
                if (index == -1) return -1;
                if (index == orderedData.Count - 1) return index;

                var r = orderedData[index + 1];
                if (comparer.Compare(indexSelector(r), key) == 0)
                {
                    return index;
                }
                else
                {
                    lower = index; // search again.
                }
            }
        }

        /// <summary>
        /// Get first(single) value.
        /// </summary>
        public T Find(TKey key)
        {
            if (comparers.Length == 1)
            {
                var index = FindIndex(0, orderedData.Count, key, comparers[0]);
                if (index == -1)
                {
                    throw new KeyNotFoundException("Key:" + key);
                }
                return orderedData[index];
            }
            else
            {
                // multi key

                foreach (var comparer in comparers)
                {

                }
                throw new NotImplementedException();
            }
        }

        // get the nearest value.
        public T FindNearest()
        {
            throw new NotImplementedException();
        }

        // TODO:Return IRangeView
        public RangeView<T> FindRange(TKey key, bool ascendant = true)
        {
            // TODO:Comparers

            var left = FindLeftIndex(0, orderedData.Count, key, comparers[0]);
            if (left == -1) new RangeView<T>(orderedData, 0, 0, ascendant); // empty

            var right = FindRightIndex(0, orderedData.Count, key, comparers[0]);

            return new MasterMemory.RangeView<T>(orderedData, left, right, ascendant);
        }

        public RangeView<T> FindRange(TKey left, TKey right)
        {
            throw new NotImplementedException();


        }

        internal void Serialize()
        {

        }

        // static? needs serializer?
        internal static Memory<T, TKey> Deserialize()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Memory:" + Key + ", Count:" + orderedData.Count;
        }





        public Memory<T, TSecondaryIndex> DynamicIndex<TSecondaryIndex>(string indexName, Func<T, TSecondaryIndex> secondaryIndexSelector)
        {
            object memory;
            lock (gate)
            {
                if (dynamicIndexMemoriesCache == null)
                {
                    dynamicIndexMemoriesCache = new Dictionary<string, object>();
                }

                if (!dynamicIndexMemoriesCache.TryGetValue(indexName, out memory))
                {
                    memory = dynamicIndexMemoriesCache[indexName] = new Memory<T, TSecondaryIndex>(indexName, this.orderedData, secondaryIndexSelector);
                }
            }

            return (Memory<T, TSecondaryIndex>)memory;
        }

        // don't use OrderBy
        T[] FastSort(IEnumerable<T> datasource, Func<T, TKey> indexSelector, IComparer<TKey> comparer)
        {
            var collection = datasource as ICollection<T>;
            if (collection != null)
            {
                var array = new T[collection.Count];
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
                var array = new ExpandableArray<T>();
                var sortSource = new ExpandableArray<TKey>();
                foreach (var item in collection)
                {
                    array.Add(item);
                    sortSource.Add(indexSelector(item));
                }

                Array.Sort(sortSource.items, array.items, 0, array.count, comparer);

                Array.Resize(ref array.items, array.count);
                return array.items;
            }
        }
    }
}