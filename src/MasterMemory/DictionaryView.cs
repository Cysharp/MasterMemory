using System;
using System.Collections;
using System.Collections.Generic;

namespace MasterMemory
{
    public class DictionaryView<TKey, TElement> : IDictionary<TKey, TElement>, IReadOnlyDictionary<TKey, TElement>
    {
        readonly Memory<TKey, TElement> innerMemory;

        public DictionaryView(Memory<TKey, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        TElement IDictionary<TKey, TElement>.this[TKey key]
        {
            get
            {
                return this[key];
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public TElement this[TKey key]
        {
            get
            {
                TElement v;
                if (TryGetValue(key, out v))
                {
                    return v;
                }
                else
                {
                    throw new KeyNotFoundException("NotFound Key:" + key);
                }
            }
        }

        int ICollection<KeyValuePair<TKey, TElement>>.Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        int IReadOnlyCollection<KeyValuePair<TKey, TElement>>.Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        bool ICollection<KeyValuePair<TKey, TElement>>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        ICollection<TKey> IDictionary<TKey, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        ICollection<TElement> IDictionary<TKey, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<TKey, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        void ICollection<KeyValuePair<TKey, TElement>>.Add(KeyValuePair<TKey, TElement> item)
        {
            throw new NotSupportedException();
        }

        void IDictionary<TKey, TElement>.Add(TKey key, TElement value)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TElement>>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TElement>>.Contains(KeyValuePair<TKey, TElement> item)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<TKey, TElement>.ContainsKey(TKey key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        bool IReadOnlyDictionary<TKey, TElement>.ContainsKey(TKey key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        void ICollection<KeyValuePair<TKey, TElement>>.CopyTo(KeyValuePair<TKey, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        IEnumerator<KeyValuePair<TKey, TElement>> IEnumerable<KeyValuePair<TKey, TElement>>.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TElement>>.Remove(KeyValuePair<TKey, TElement> item)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<TKey, TElement>.Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(TKey key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }
    }
}