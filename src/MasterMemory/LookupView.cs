using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MasterMemory
{
    public class LookupView<TKey, TElement> : ILookup<TKey, TElement>
    {
        Memory<TKey, TElement> innerMemory;

        public LookupView(Memory<TKey, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                return this.innerMemory.FindMany(key);
            }
        }

        public int Count
        {
            get
            {
                return this.innerMemory.Count;
            }
        }

        public bool Contains(TKey key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}