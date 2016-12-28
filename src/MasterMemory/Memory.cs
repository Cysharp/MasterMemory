using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;
using ZeroFormatter.Formatters;

namespace MasterMemory
{
    /// <summary>
    /// Represents primary indexed memory.
    /// </summary>
    public class Memory<T, TPrimaryIndex>
    {
        // don't serialize, only for debug view.
        public string Key { get; private set; }

        // ZeroFormatter delayed list.
        protected readonly IList<T> orderedData;
        readonly Func<T, TPrimaryIndex> primaryIndexSelector;
        readonly IComparer<TPrimaryIndex>[] comparers;

        public Memory(string key, IEnumerable<T> datasource, Func<T, TPrimaryIndex> primaryIndexSelector)
        {
            // TODO:improve performance to use primitive array.
            this.orderedData = datasource.OrderBy(x => primaryIndexSelector(x)).ToArray();
            this.primaryIndexSelector = primaryIndexSelector;
            this.comparers = new[] { Comparer<TPrimaryIndex>.Default };

            // typeof(TPrimaryIndex).GetInterfaces().contain
        }

        public Memory(string key, ArraySegment<byte> bytes, Func<T, TPrimaryIndex> primaryIndexSelector)
        {
            // TODO:NullTracker
            var array = bytes.Array;
            int size;
            this.orderedData = ZeroFormatter.Formatters.Formatter<DefaultResolver, IList<T>>.Default.Deserialize(ref array, bytes.Offset, new DirtyTracker(), out size);
            this.primaryIndexSelector = primaryIndexSelector;
            this.comparers = new[] { Comparer<TPrimaryIndex>.Default };
        }

        // get the first found value.
        protected bool FindIndex(int lower, int upper, TPrimaryIndex key, IComparer<TPrimaryIndex> comparer, out int foundIndex)
        {
            while (upper - lower > 1)
            {
                var index = lower + ((upper - lower) / 2);
                var compare = comparer.Compare(primaryIndexSelector(orderedData[index]), key);
                if (compare == 0)
                {
                    lower = upper = index;
                    foundIndex = lower;
                    return true;
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

            foundIndex = -1;
            return false;
        }

        protected int GetLeftIndex(int index, IComparer<TPrimaryIndex> comparer)
        {
            var pk = primaryIndexSelector(orderedData[index]);
            var minusOne = primaryIndexSelector(orderedData[index - 1]);

            // TODO:....
            if (comparer.Compare(pk, minusOne) == 0)
            {
                return -1;
            }
            else
            {
                return -1;
            }
            //if (pk == minusOne)
            //{
            //}
        }

        //protected int GetRightIndex()
        //{
        //}

        public T Find(TPrimaryIndex key)
        {
            // TODO:Comparers...
            int index;
            if (!FindIndex(-1, orderedData.Count, key, comparers[0], out index))
            {
                throw new KeyNotFoundException("Key:" + key);
            }

            return orderedData[index];
        }

        // get the nearest value.
        public T FindNearest()
        {
            throw new NotImplementedException();
        }

        // TODO:Return IRangeView
        public T[] FindRange(TPrimaryIndex index)
        {
            throw new NotImplementedException();
        }

        public T[] FindRange(TPrimaryIndex index, TPrimaryIndex left, TPrimaryIndex right)
        {
            throw new NotImplementedException();


        }

        internal virtual void Serialize()
        {

        }

        // static? needs serializer?
        internal static Memory<T, TPrimaryIndex> Deserialize()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Memory:" + Key + ", Count:" + orderedData.Count;
        }
    }


    // TODO:...
    public class Memory<T, TPrimaryIndex, TSecondaryIndex1> : Memory<T, TPrimaryIndex>
    {
        public Memory(string key, ArraySegment<byte> bytes, Func<T, TPrimaryIndex> primaryIndexSelector) : base(key, bytes, primaryIndexSelector)
        {
        }

        public Memory(string key, IEnumerable<T> datasource, Func<T, TPrimaryIndex> primaryIndexSelector) : base(key, datasource, primaryIndexSelector)
        {
        }

        internal override void Serialize()
        {
            base.Serialize();
        }
    }
}