 
using System;
using System.Collections.Generic;
using System.Linq;
using MasterMemory.Internal;

namespace MasterMemory.Internal
{

    internal class MemoryKey1Memory<TKey1, TKey2, TElement> : IMemoryFinder<TKey1, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2>, TElement> memory;
        const int ComparerCount = 1;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey1Memory(Memory<MemoryKey<TKey1, TKey2>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(TKey1 key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(TKey1 key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(TKey1 key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(TKey1 key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(TKey1 key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<TKey1, TElement> ToLookupView()
        {
            return new LookupView1<TKey1, TKey2, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<TKey1, TElement> ToDictionaryView()
#else
        public IDictionary<TKey1, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView1<TKey1, TKey2, TElement>(this);
        }

        MemoryKey<TKey1, TKey2> CreateKey(TKey1 key)
        {
            return MemoryKey.Create(key, default(TKey2));
        }
    }            


    internal class MemoryKey1Memory<TKey1, TKey2, TKey3, TElement> : IMemoryFinder<TKey1, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3>, TElement> memory;
        const int ComparerCount = 1;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey1Memory(Memory<MemoryKey<TKey1, TKey2, TKey3>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(TKey1 key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(TKey1 key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(TKey1 key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(TKey1 key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(TKey1 key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<TKey1, TElement> ToLookupView()
        {
            return new LookupView1<TKey1, TKey2, TKey3, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<TKey1, TElement> ToDictionaryView()
#else
        public IDictionary<TKey1, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView1<TKey1, TKey2, TKey3, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3> CreateKey(TKey1 key)
        {
            return MemoryKey.Create(key, default(TKey2), default(TKey3));
        }
    }            


    internal class MemoryKey12Memory<TKey1, TKey2, TKey3, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3>, TElement> memory;
        const int ComparerCount = 2;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey12Memory(Memory<MemoryKey<TKey1, TKey2, TKey3>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2>, TElement> ToLookupView()
        {
            return new LookupView12<TKey1, TKey2, TKey3, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView12<TKey1, TKey2, TKey3, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3> CreateKey(MemoryKey<TKey1, TKey2> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, default(TKey3));
        }
    }            


    internal class MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TElement> : IMemoryFinder<TKey1, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory;
        const int ComparerCount = 1;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey1Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(TKey1 key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(TKey1 key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(TKey1 key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(TKey1 key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(TKey1 key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<TKey1, TElement> ToLookupView()
        {
            return new LookupView1<TKey1, TKey2, TKey3, TKey4, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<TKey1, TElement> ToDictionaryView()
#else
        public IDictionary<TKey1, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView1<TKey1, TKey2, TKey3, TKey4, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4> CreateKey(TKey1 key)
        {
            return MemoryKey.Create(key, default(TKey2), default(TKey3), default(TKey4));
        }
    }            


    internal class MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory;
        const int ComparerCount = 2;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey12Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2>, TElement> ToLookupView()
        {
            return new LookupView12<TKey1, TKey2, TKey3, TKey4, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView12<TKey1, TKey2, TKey3, TKey4, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4> CreateKey(MemoryKey<TKey1, TKey2> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, default(TKey3), default(TKey4));
        }
    }            


    internal class MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory;
        const int ComparerCount = 3;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey123Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2, TKey3> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2, TKey3> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2, TKey3> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2, TKey3> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToLookupView()
        {
            return new LookupView123<TKey1, TKey2, TKey3, TKey4, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView123<TKey1, TKey2, TKey3, TKey4, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4> CreateKey(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, key.Item3, default(TKey4));
        }
    }            


    internal class MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : IMemoryFinder<TKey1, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory;
        const int ComparerCount = 1;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey1Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(TKey1 key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(TKey1 key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(TKey1 key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(TKey1 key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(TKey1 key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<TKey1, TElement> ToLookupView()
        {
            return new LookupView1<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<TKey1, TElement> ToDictionaryView()
#else
        public IDictionary<TKey1, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView1<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> CreateKey(TKey1 key)
        {
            return MemoryKey.Create(key, default(TKey2), default(TKey3), default(TKey4), default(TKey5));
        }
    }            


    internal class MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory;
        const int ComparerCount = 2;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey12Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2>, TElement> ToLookupView()
        {
            return new LookupView12<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView12<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> CreateKey(MemoryKey<TKey1, TKey2> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, default(TKey3), default(TKey4), default(TKey5));
        }
    }            


    internal class MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory;
        const int ComparerCount = 3;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey123Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2, TKey3> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2, TKey3> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2, TKey3> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2, TKey3> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToLookupView()
        {
            return new LookupView123<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView123<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> CreateKey(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, key.Item3, default(TKey4), default(TKey5));
        }
    }            


    internal class MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory;
        const int ComparerCount = 4;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey1234Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> ToLookupView()
        {
            return new LookupView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> CreateKey(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, key.Item3, key.Item4, default(TKey5));
        }
    }            


    internal class MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : IMemoryFinder<TKey1, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory;
        const int ComparerCount = 1;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey1Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(TKey1 key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(TKey1 key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(TKey1 key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(TKey1 key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(TKey1 key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<TKey1, TElement> ToLookupView()
        {
            return new LookupView1<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<TKey1, TElement> ToDictionaryView()
#else
        public IDictionary<TKey1, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView1<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> CreateKey(TKey1 key)
        {
            return MemoryKey.Create(key, default(TKey2), default(TKey3), default(TKey4), default(TKey5), default(TKey6));
        }
    }            


    internal class MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory;
        const int ComparerCount = 2;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey12Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2>, TElement> ToLookupView()
        {
            return new LookupView12<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView12<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> CreateKey(MemoryKey<TKey1, TKey2> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, default(TKey3), default(TKey4), default(TKey5), default(TKey6));
        }
    }            


    internal class MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory;
        const int ComparerCount = 3;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey123Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2, TKey3> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2, TKey3> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2, TKey3> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2, TKey3> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToLookupView()
        {
            return new LookupView123<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView123<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> CreateKey(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, key.Item3, default(TKey4), default(TKey5), default(TKey6));
        }
    }            


    internal class MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory;
        const int ComparerCount = 4;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey1234Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> ToLookupView()
        {
            return new LookupView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> CreateKey(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, key.Item3, key.Item4, default(TKey5), default(TKey6));
        }
    }            


    internal class MemoryKey12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory;
        const int ComparerCount = 5;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey12345Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> ToLookupView()
        {
            return new LookupView12345<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView12345<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> CreateKey(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, key.Item3, key.Item4, key.Item5, default(TKey6));
        }
    }            


    internal class MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IMemoryFinder<TKey1, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory;
        const int ComparerCount = 1;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey1Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(TKey1 key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(TKey1 key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(TKey1 key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(TKey1 key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(TKey1 key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<TKey1, TElement> ToLookupView()
        {
            return new LookupView1<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<TKey1, TElement> ToDictionaryView()
#else
        public IDictionary<TKey1, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView1<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7> CreateKey(TKey1 key)
        {
            return MemoryKey.Create(key, default(TKey2), default(TKey3), default(TKey4), default(TKey5), default(TKey6), default(TKey7));
        }
    }            


    internal class MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory;
        const int ComparerCount = 2;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey12Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2>, TElement> ToLookupView()
        {
            return new LookupView12<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView12<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7> CreateKey(MemoryKey<TKey1, TKey2> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, default(TKey3), default(TKey4), default(TKey5), default(TKey6), default(TKey7));
        }
    }            


    internal class MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory;
        const int ComparerCount = 3;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey123Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2, TKey3> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2, TKey3> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2, TKey3> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2, TKey3> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToLookupView()
        {
            return new LookupView123<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2, TKey3>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView123<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7> CreateKey(MemoryKey<TKey1, TKey2, TKey3> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, key.Item3, default(TKey4), default(TKey5), default(TKey6), default(TKey7));
        }
    }            


    internal class MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory;
        const int ComparerCount = 4;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey1234Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2, TKey3, TKey4> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> ToLookupView()
        {
            return new LookupView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7> CreateKey(MemoryKey<TKey1, TKey2, TKey3, TKey4> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, key.Item3, key.Item4, default(TKey5), default(TKey6), default(TKey7));
        }
    }            


    internal class MemoryKey12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory;
        const int ComparerCount = 5;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey12345Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> ToLookupView()
        {
            return new LookupView12345<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView12345<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7> CreateKey(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, key.Item3, key.Item4, key.Item5, default(TKey6), default(TKey7));
        }
    }            


    internal class MemoryKey123456Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>
    {
        readonly Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory;
        const int ComparerCount = 6;

        public int Count
        {
            get
            {
                return memory.Count;
            }
        }

        public MemoryKey123456Memory(Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            this.memory = memory;
        }

        public TElement Find(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key)
        {
            return memory.InternalFind(CreateKey(key), ComparerCount);
        }

        public RangeView<TElement> FindAll(bool ascendant = true)
        {
            return memory.FindAll(ascendant);
        }

        public RangeView<TElement> FindMany(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key, bool ascendant = true)
        {
            return memory.InternalFindMany(CreateKey(key), ascendant, ComparerCount);
        }

        public TElement FindClosest(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key, bool selectLower = true)
        {
            return memory.InternalFindClosest(CreateKey(key), selectLower, ComparerCount);
        }

        public TElement FindOrDefault(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key, TElement defaultValue = default(TElement))
        {
            return memory.InternalFindOrDefault(CreateKey(key), defaultValue, ComparerCount);
        }

        public bool TryFind(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key, out TElement value)
        {
            return memory.InternalTryFind(CreateKey(key), ComparerCount, out value);
        }

        public ILookup<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> ToLookupView()
        {
            return new LookupView123456<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }
#if !UNITY_5
        public IReadOnlyDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> ToDictionaryView()
#else
        public IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> ToDictionaryView()
#endif
        {
            return new DictionaryView123456<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this);
        }

        MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7> CreateKey(MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key)
        {
            return MemoryKey.Create(key.Item1, key.Item2, key.Item3, key.Item4, key.Item5, key.Item6, default(TKey7));
        }
    }            

}

namespace MasterMemory
{
    public static class MemoryUseIndexExtensions
    {

        public static IMemoryFinder<TKey1, TElement> UseIndex1<TKey1, TKey2, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement> memory)
        {
            return new MemoryKey1Memory<TKey1, TKey2, TElement>(memory as Memory<MemoryKey<TKey1, TKey2>, TElement>);
        }


        public static IMemoryFinder<TKey1, TElement> UseIndex1<TKey1, TKey2, TKey3, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement> memory)
        {
            return new MemoryKey1Memory<TKey1, TKey2, TKey3, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement> UseIndex12<TKey1, TKey2, TKey3, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement> memory)
        {
            return new MemoryKey12Memory<TKey1, TKey2, TKey3, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3>, TElement>);
        }


        public static IMemoryFinder<TKey1, TElement> UseIndex1<TKey1, TKey2, TKey3, TKey4, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory)
        {
            return new MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement> UseIndex12<TKey1, TKey2, TKey3, TKey4, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory)
        {
            return new MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement> UseIndex123<TKey1, TKey2, TKey3, TKey4, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory)
        {
            return new MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement>);
        }


        public static IMemoryFinder<TKey1, TElement> UseIndex1<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory)
        {
            return new MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement> UseIndex12<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory)
        {
            return new MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement> UseIndex123<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory)
        {
            return new MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> UseIndex1234<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory)
        {
            return new MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>);
        }


        public static IMemoryFinder<TKey1, TElement> UseIndex1<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory)
        {
            return new MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement> UseIndex12<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory)
        {
            return new MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement> UseIndex123<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory)
        {
            return new MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> UseIndex1234<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory)
        {
            return new MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> UseIndex12345<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory)
        {
            return new MemoryKey12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>);
        }


        public static IMemoryFinder<TKey1, TElement> UseIndex1<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            return new MemoryKey1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement> UseIndex12<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            return new MemoryKey12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement> UseIndex123<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            return new MemoryKey123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> UseIndex1234<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            return new MemoryKey1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> UseIndex12345<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            return new MemoryKey12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement>);
        }


        public static IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> UseIndex123456<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory)
        {
            return new MemoryKey123456Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(memory as Memory<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement>);
        }

    }

    public static class MemoryKeyConstructExtensions
    {

        public static bool TryFind<TKey1, TKey2, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement> memory, TKey1 item1, TKey2 item2, out TElement value)
        {
            return memory.TryFind(MemoryKey.Create(item1, item2), out value);
        }

        public static TElement Find<TKey1, TKey2, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement> memory, TKey1 item1, TKey2 item2)
        {
            return memory.Find(MemoryKey.Create(item1, item2));
        }

        public static TElement FindOrDefault<TKey1, TKey2, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement> memory, TKey1 item1, TKey2 item2, TElement defaultValue = default(TElement))
        {
            return memory.FindOrDefault(MemoryKey.Create(item1, item2), defaultValue);
        }

        public static TElement FindClosest<TKey1, TKey2, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement> memory, TKey1 item1, TKey2 item2, bool selectLower = true)
        {
            return memory.FindClosest(MemoryKey.Create(item1, item2), selectLower);
        }

        public static RangeView<TElement> FindMany<TKey1, TKey2, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2>, TElement> memory, TKey1 item1, TKey2 item2, bool ascendant = true)
        {
            return memory.FindMany(MemoryKey.Create(item1, item2), ascendant);
        }


        public static bool TryFind<TKey1, TKey2, TKey3, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, out TElement value)
        {
            return memory.TryFind(MemoryKey.Create(item1, item2, item3), out value);
        }

        public static TElement Find<TKey1, TKey2, TKey3, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3)
        {
            return memory.Find(MemoryKey.Create(item1, item2, item3));
        }

        public static TElement FindOrDefault<TKey1, TKey2, TKey3, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TElement defaultValue = default(TElement))
        {
            return memory.FindOrDefault(MemoryKey.Create(item1, item2, item3), defaultValue);
        }

        public static TElement FindClosest<TKey1, TKey2, TKey3, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, bool selectLower = true)
        {
            return memory.FindClosest(MemoryKey.Create(item1, item2, item3), selectLower);
        }

        public static RangeView<TElement> FindMany<TKey1, TKey2, TKey3, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, bool ascendant = true)
        {
            return memory.FindMany(MemoryKey.Create(item1, item2, item3), ascendant);
        }


        public static bool TryFind<TKey1, TKey2, TKey3, TKey4, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, out TElement value)
        {
            return memory.TryFind(MemoryKey.Create(item1, item2, item3, item4), out value);
        }

        public static TElement Find<TKey1, TKey2, TKey3, TKey4, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4)
        {
            return memory.Find(MemoryKey.Create(item1, item2, item3, item4));
        }

        public static TElement FindOrDefault<TKey1, TKey2, TKey3, TKey4, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TElement defaultValue = default(TElement))
        {
            return memory.FindOrDefault(MemoryKey.Create(item1, item2, item3, item4), defaultValue);
        }

        public static TElement FindClosest<TKey1, TKey2, TKey3, TKey4, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, bool selectLower = true)
        {
            return memory.FindClosest(MemoryKey.Create(item1, item2, item3, item4), selectLower);
        }

        public static RangeView<TElement> FindMany<TKey1, TKey2, TKey3, TKey4, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, bool ascendant = true)
        {
            return memory.FindMany(MemoryKey.Create(item1, item2, item3, item4), ascendant);
        }


        public static bool TryFind<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, out TElement value)
        {
            return memory.TryFind(MemoryKey.Create(item1, item2, item3, item4, item5), out value);
        }

        public static TElement Find<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5)
        {
            return memory.Find(MemoryKey.Create(item1, item2, item3, item4, item5));
        }

        public static TElement FindOrDefault<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, TElement defaultValue = default(TElement))
        {
            return memory.FindOrDefault(MemoryKey.Create(item1, item2, item3, item4, item5), defaultValue);
        }

        public static TElement FindClosest<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, bool selectLower = true)
        {
            return memory.FindClosest(MemoryKey.Create(item1, item2, item3, item4, item5), selectLower);
        }

        public static RangeView<TElement> FindMany<TKey1, TKey2, TKey3, TKey4, TKey5, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, bool ascendant = true)
        {
            return memory.FindMany(MemoryKey.Create(item1, item2, item3, item4, item5), ascendant);
        }


        public static bool TryFind<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, TKey6 item6, out TElement value)
        {
            return memory.TryFind(MemoryKey.Create(item1, item2, item3, item4, item5, item6), out value);
        }

        public static TElement Find<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, TKey6 item6)
        {
            return memory.Find(MemoryKey.Create(item1, item2, item3, item4, item5, item6));
        }

        public static TElement FindOrDefault<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, TKey6 item6, TElement defaultValue = default(TElement))
        {
            return memory.FindOrDefault(MemoryKey.Create(item1, item2, item3, item4, item5, item6), defaultValue);
        }

        public static TElement FindClosest<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, TKey6 item6, bool selectLower = true)
        {
            return memory.FindClosest(MemoryKey.Create(item1, item2, item3, item4, item5, item6), selectLower);
        }

        public static RangeView<TElement> FindMany<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, TKey6 item6, bool ascendant = true)
        {
            return memory.FindMany(MemoryKey.Create(item1, item2, item3, item4, item5, item6), ascendant);
        }


        public static bool TryFind<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, TKey6 item6, TKey7 item7, out TElement value)
        {
            return memory.TryFind(MemoryKey.Create(item1, item2, item3, item4, item5, item6, item7), out value);
        }

        public static TElement Find<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, TKey6 item6, TKey7 item7)
        {
            return memory.Find(MemoryKey.Create(item1, item2, item3, item4, item5, item6, item7));
        }

        public static TElement FindOrDefault<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, TKey6 item6, TKey7 item7, TElement defaultValue = default(TElement))
        {
            return memory.FindOrDefault(MemoryKey.Create(item1, item2, item3, item4, item5, item6, item7), defaultValue);
        }

        public static TElement FindClosest<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, TKey6 item6, TKey7 item7, bool selectLower = true)
        {
            return memory.FindClosest(MemoryKey.Create(item1, item2, item3, item4, item5, item6, item7), selectLower);
        }

        public static RangeView<TElement> FindMany<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement>(this IMemoryFinder<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TElement> memory, TKey1 item1, TKey2 item2, TKey3 item3, TKey4 item4, TKey5 item5, TKey6 item6, TKey7 item7, bool ascendant = true)
        {
            return memory.FindMany(MemoryKey.Create(item1, item2, item3, item4, item5, item6, item7), ascendant);
        }

    }
}