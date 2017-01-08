 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ZeroFormatter;

namespace MasterMemory.Internal
{
    public class LookupView<TKey, TElement> : ILookup<TKey, TElement>
    {
        readonly Memory<TKey, TElement> innerMemory;

        internal LookupView(Memory<TKey, TElement> innerMemory)
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


    public class LookupView1<TKey1, TKey2, TElement> : ILookup<TKey1, TElement>
    {
        readonly KeyTuple1Memory<TKey1, TKey2, TElement> innerMemory;

        internal LookupView1(KeyTuple1Memory<TKey1, TKey2, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[TKey1 key]
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

        public bool Contains(TKey1 key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView1<TKey1, TKey2, TKey3, TElement> : ILookup<TKey1, TElement>
    {
        readonly KeyTuple1Memory<TKey1, TKey2, TKey3, TElement> innerMemory;

        internal LookupView1(KeyTuple1Memory<TKey1, TKey2, TKey3, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[TKey1 key]
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

        public bool Contains(TKey1 key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView12<TKey1, TKey2, TKey3, TElement> : ILookup<KeyTuple<TKey1, TKey2>, TElement>
    {
        readonly KeyTuple12Memory<TKey1, TKey2, TKey3, TElement> innerMemory;

        internal LookupView12(KeyTuple12Memory<TKey1, TKey2, TKey3, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView1<TKey1, TKey2, TKey3, TKey4, TElement> : ILookup<TKey1, TElement>
    {
        readonly KeyTuple1Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory;

        internal LookupView1(KeyTuple1Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[TKey1 key]
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

        public bool Contains(TKey1 key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView12<TKey1, TKey2, TKey3, TKey4, TElement> : ILookup<KeyTuple<TKey1, TKey2>, TElement>
    {
        readonly KeyTuple12Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory;

        internal LookupView12(KeyTuple12Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView123<TKey1, TKey2, TKey3, TKey4, TElement> : ILookup<KeyTuple<TKey1, TKey2, TKey3>, TElement>
    {
        readonly KeyTuple123Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory;

        internal LookupView123(KeyTuple123Memory<TKey1, TKey2, TKey3, TKey4, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2, TKey3> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2, TKey3> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2, TKey3>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView1<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : ILookup<TKey1, TElement>
    {
        readonly KeyTuple1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory;

        internal LookupView1(KeyTuple1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[TKey1 key]
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

        public bool Contains(TKey1 key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView12<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : ILookup<KeyTuple<TKey1, TKey2>, TElement>
    {
        readonly KeyTuple12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory;

        internal LookupView12(KeyTuple12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView123<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : ILookup<KeyTuple<TKey1, TKey2, TKey3>, TElement>
    {
        readonly KeyTuple123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory;

        internal LookupView123(KeyTuple123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2, TKey3> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2, TKey3> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2, TKey3>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> : ILookup<KeyTuple<TKey1, TKey2, TKey3, TKey4>, TElement>
    {
        readonly KeyTuple1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory;

        internal LookupView1234(KeyTuple1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2, TKey3, TKey4> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2, TKey3, TKey4> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2, TKey3, TKey4>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView1<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : ILookup<TKey1, TElement>
    {
        readonly KeyTuple1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory;

        internal LookupView1(KeyTuple1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[TKey1 key]
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

        public bool Contains(TKey1 key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView12<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : ILookup<KeyTuple<TKey1, TKey2>, TElement>
    {
        readonly KeyTuple12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory;

        internal LookupView12(KeyTuple12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView123<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : ILookup<KeyTuple<TKey1, TKey2, TKey3>, TElement>
    {
        readonly KeyTuple123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory;

        internal LookupView123(KeyTuple123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2, TKey3> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2, TKey3> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2, TKey3>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : ILookup<KeyTuple<TKey1, TKey2, TKey3, TKey4>, TElement>
    {
        readonly KeyTuple1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory;

        internal LookupView1234(KeyTuple1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2, TKey3, TKey4> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2, TKey3, TKey4> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2, TKey3, TKey4>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView12345<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> : ILookup<KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>
    {
        readonly KeyTuple12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory;

        internal LookupView12345(KeyTuple12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView1<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : ILookup<TKey1, TElement>
    {
        readonly KeyTuple1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal LookupView1(KeyTuple1Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[TKey1 key]
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

        public bool Contains(TKey1 key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<TKey1, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView12<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : ILookup<KeyTuple<TKey1, TKey2>, TElement>
    {
        readonly KeyTuple12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal LookupView12(KeyTuple12Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView123<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : ILookup<KeyTuple<TKey1, TKey2, TKey3>, TElement>
    {
        readonly KeyTuple123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal LookupView123(KeyTuple123Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2, TKey3> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2, TKey3> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2, TKey3>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView1234<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : ILookup<KeyTuple<TKey1, TKey2, TKey3, TKey4>, TElement>
    {
        readonly KeyTuple1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal LookupView1234(KeyTuple1234Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2, TKey3, TKey4> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2, TKey3, TKey4> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2, TKey3, TKey4>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView12345<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : ILookup<KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>
    {
        readonly KeyTuple12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal LookupView12345(KeyTuple12345Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class LookupView123456<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> : ILookup<KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>
    {
        readonly KeyTuple123456Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory;

        internal LookupView123456(KeyTuple123456Memory<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TElement> innerMemory)
        {
            this.innerMemory = innerMemory;
        }

        public IEnumerable<TElement> this[KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key]
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

        public bool Contains(KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> key)
        {
            TElement v;
            return innerMemory.TryFind(key, out v);
        }

        public IEnumerator<IGrouping<KeyTuple<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TElement>> GetEnumerator()
        {
            throw new NotSupportedException("LookupView does not support iterate all.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}