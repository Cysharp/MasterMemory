 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MasterMemory.Internal
{
    public class DictionaryView<TKey, TElement> : IDictionary<TKey, TElement>
#if !UNITY_5
    , IReadOnlyDictionary<TKey, TElement>
#endif
    {
        readonly Memory<TKey, TElement> innerMemory;

        public DictionaryView(Memory<TKey, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(TKey key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(TKey key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TElement>.Keys
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

#endif

        public void Add(KeyValuePair<TKey, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(TKey key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<TKey, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<TKey, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TKey, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TKey, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView1<TKey1, TKey2, TElement> : IDictionary<TKey1, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<TKey1, TElement>
#endif
    {
        readonly MemoryKey1Memory<TKey1, TKey2, TElement> innerMemory;

        internal DictionaryView1(MemoryKey1Memory<TKey1, TKey2, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[TKey1 key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(TKey1 key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(TKey1 key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<TKey1> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<TKey1> IReadOnlyDictionary<TKey1, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<TKey1, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(TKey1 key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<TKey1, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(TKey1 key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView1<TKey1, TKey2, TKey3, TElement> : IDictionary<TKey1, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<TKey1, TElement>
#endif
    {
        readonly MemoryKey1Memory<TKey1, TKey2, TKey3, TElement> innerMemory;

        internal DictionaryView1(MemoryKey1Memory<TKey1, TKey2, TKey3, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[TKey1 key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(TKey1 key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(TKey1 key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<TKey1> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<TKey1> IReadOnlyDictionary<TKey1, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<TKey1, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(TKey1 key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<TKey1, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(TKey1 key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView12<TKey1, TKey2, TKey3, TElement> : IDictionary<MemoryKey<TKey1, TKey2>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>
#endif
    {
        readonly MemoryKey12Memory<TKey1, TKey2, TKey3, TElement> innerMemory;

        internal DictionaryView12(MemoryKey12Memory<TKey1, TKey2, TKey3, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView1<TKey1, TKey2, TKey3, TKey4, TElement> : IDictionary<TKey1, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<TKey1, TElement>
#endif
    {
        readonly MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory;

        internal DictionaryView1(MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[TKey1 key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(TKey1 key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(TKey1 key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<TKey1> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<TKey1> IReadOnlyDictionary<TKey1, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<TKey1, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(TKey1 key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<TKey1, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(TKey1 key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView12<TKey1, TKey2, TKey3, TKey4, TElement> : IDictionary<MemoryKey<TKey1, TKey2>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>
#endif
    {
        readonly MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory;

        internal DictionaryView12(MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView123<TKey1, TKey2, TKey3, TKey4, TElement> : IDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>
#endif
    {
        readonly MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory;

        internal DictionaryView123(MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2, TKey3> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2, TKey3> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2, TKey3>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2, TKey3>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2, TKey3> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView1<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : IDictionary<TKey1, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<TKey1, TElement>
#endif
    {
        readonly MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory;

        internal DictionaryView1(MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[TKey1 key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(TKey1 key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(TKey1 key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<TKey1> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<TKey1> IReadOnlyDictionary<TKey1, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<TKey1, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(TKey1 key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<TKey1, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(TKey1 key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView12<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : IDictionary<MemoryKey<TKey1, TKey2>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>
#endif
    {
        readonly MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory;

        internal DictionaryView12(MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView123<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : IDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>
#endif
    {
        readonly MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory;

        internal DictionaryView123(MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2, TKey3> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2, TKey3> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2, TKey3>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2, TKey3>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2, TKey3> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>
#endif
    {
        readonly MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory;

        internal DictionaryView1234(MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2, TKey3, TKey4> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2, TKey3, TKey4>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2, TKey3, TKey4>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView1<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : IDictionary<TKey1, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<TKey1, TElement>
#endif
    {
        readonly MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory;

        internal DictionaryView1(MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[TKey1 key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(TKey1 key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(TKey1 key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<TKey1> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<TKey1> IReadOnlyDictionary<TKey1, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<TKey1, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(TKey1 key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<TKey1, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(TKey1 key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView12<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : IDictionary<MemoryKey<TKey1, TKey2>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>
#endif
    {
        readonly MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory;

        internal DictionaryView12(MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView123<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : IDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>
#endif
    {
        readonly MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory;

        internal DictionaryView123(MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2, TKey3> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2, TKey3> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2, TKey3>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2, TKey3>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2, TKey3> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>
#endif
    {
        readonly MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory;

        internal DictionaryView1234(MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2, TKey3, TKey4> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2, TKey3, TKey4>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2, TKey3, TKey4>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView12345<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>
#endif
    {
        readonly MemoryKey12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory;

        internal DictionaryView12345(MemoryKey12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView1<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IDictionary<TKey1, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<TKey1, TElement>
#endif
    {
        readonly MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal DictionaryView1(MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[TKey1 key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(TKey1 key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(TKey1 key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<TKey1> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<TKey1> IReadOnlyDictionary<TKey1, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<TKey1, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(TKey1 key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<TKey1, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TKey1, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(TKey1 key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView12<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IDictionary<MemoryKey<TKey1, TKey2>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>
#endif
    {
        readonly MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal DictionaryView12(MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView123<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>
#endif
    {
        readonly MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal DictionaryView123(MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2, TKey3> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2, TKey3> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2, TKey3>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2, TKey3>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2, TKey3> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>
#endif
    {
        readonly MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal DictionaryView1234(MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2, TKey3, TKey4> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2, TKey3, TKey4>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2, TKey3, TKey4>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView12345<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>
#endif
    {
        readonly MemoryKey12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal DictionaryView12345(MemoryKey12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }


    public class DictionaryView123456<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>   
#if !UNITY_5    
    , IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>
#endif
    {
        readonly MemoryKey123456Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal DictionaryView123456(MemoryKey123456Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public TElement this[MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key]
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
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return innerMemory.Count;
            }
        }

        public bool ContainsKey(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key)
        {
            TElement v;
            return TryGetValue(key, out v);
        }

        public bool TryGetValue(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key, out TElement value)
        {
            return innerMemory.TryFind(key, out value);
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public ICollection<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>> Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public ICollection<TElement> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#if !UNITY_5

        IEnumerable<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>.Keys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        IEnumerable<TElement> IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>.Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }
#endif

        public void Add(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void Add(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key, TElement value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }

}