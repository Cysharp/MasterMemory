using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMemory
{
    public class LookupView<TKey, TElement> : ILookup<TKey, TElement>
    {
        Memory<TElement, TKey> innerMemory;

        

        public LookupView(Memory<TElement, TKey> innerMemory)
        {
            this.innerMemory = innerMemory;



        }

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                return this.innerMemory.FindRange(key);
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
            throw new NotImplementedException();
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}