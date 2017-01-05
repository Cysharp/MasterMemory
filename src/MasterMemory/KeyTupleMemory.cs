using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace MasterMemory
{
    // TODO:...

    internal class KeyTupleMemory<TKey1, TKey2, TElement> : IMemoryFinder<TKey1, TElement>
    {
        readonly Memory<KeyTuple<TKey1, TKey2>, TElement> memory;

        public KeyTupleMemory(Memory<KeyTuple<TKey1, TKey2>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(TKey1 key)
        {
            return memory.Find(KeyTuple.Create(key, default(TKey2)));
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(TKey1 key, bool ascendant = true)
        {
            throw new NotImplementedException();
        }

        public TElement FindClosest(TKey1 key, bool selectLower = true)
        {
            throw new NotImplementedException();
        }

        public TElement FindOrDefault(TKey1 key, TElement defaultValue = default(TElement))
        {
            throw new NotImplementedException();
        }

        public RangeView<TElement> FindRange(TKey1 left, TKey1 right, bool ascendant = true)
        {
            throw new NotImplementedException();
        }

        public bool TryFind(TKey1 key, out TElement value)
        {
            throw new NotImplementedException();
        }

        public LookupView<TKey1, TElement> ToLookupView()
        {
            throw new NotImplementedException();
        }

        public DictionaryView<TKey1, TElement> ToDictionaryView()
        {
            throw new NotImplementedException();
        }
    }

    public static class MemoryExtensions
    {
        public static IMemoryFinder<TKey1, TElement> UseIndex1<TKey1, TKey2, TElement>(this Memory<KeyTuple<TKey1, TKey2>, TElement> memory)
        {
            return new KeyTupleMemory<TKey1, TKey2, TElement>(memory);
        }
    }
}
