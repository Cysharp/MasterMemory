using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MasterMemory
{
    interface IMemoryKey
    {
        string ToString();
    }

    public static class MemoryKey
    {
        public static MemoryKey<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>
           (
            T1 item1,
            T2 item2,
            T3 item3,
            T4 item4,
            T5 item5,
            T6 item6,
            T7 item7)
        {
            return new MemoryKey<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }

        public static MemoryKey<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>
            (
             T1 item1,
             T2 item2,
             T3 item3,
             T4 item4,
             T5 item5,
             T6 item6)
        {
            return new MemoryKey<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }

        public static MemoryKey<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>
            (
             T1 item1,
             T2 item2,
             T3 item3,
             T4 item4,
             T5 item5)
        {
            return new MemoryKey<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }

        public static MemoryKey<T1, T2, T3, T4> Create<T1, T2, T3, T4>
            (
             T1 item1,
             T2 item2,
             T3 item3,
             T4 item4)
        {
            return new MemoryKey<T1, T2, T3, T4>(item1, item2, item3, item4);
        }

        public static MemoryKey<T1, T2, T3> Create<T1, T2, T3>
            (
             T1 item1,
             T2 item2,
             T3 item3)
        {
            return new MemoryKey<T1, T2, T3>(item1, item2, item3);
        }

        public static MemoryKey<T1, T2> Create<T1, T2>
            (
             T1 item1,
             T2 item2)
        {
            return new MemoryKey<T1, T2>(item1, item2);
        }
    }

    public class MemoryKey<T1, T2> : IMemoryKey, IEquatable<MemoryKey<T1, T2>>
    {
        T1 item1;
        T2 item2;

        public MemoryKey(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }

        public T1 Item1
        {
            get { return item1; }
        }

        public T2 Item2
        {
            get { return item2; }
        }

        public override int GetHashCode()
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;

            int h0;
            h0 = comparer1.GetHashCode(item1);
            h0 = (h0 << 5) + h0 ^ comparer2.GetHashCode(item2);
            return h0;
        }

        string IMemoryKey.ToString()
        {
            return String.Format("{0}, {1}", item1, item2);
        }

        public override string ToString()
        {
            return "(" + ((IMemoryKey)this).ToString() + ")";
        }

        public bool Equals(MemoryKey<T1, T2> other)
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;

            return comparer1.Equals(item1, other.item1) &&
                comparer2.Equals(item2, other.item2);
        }

        public override bool Equals(object obj)
        {
            return obj is MemoryKey<T1, T2>
                ? Equals((MemoryKey<T1, T2>)obj)
                : false;
        }
    }

    
    public class MemoryKey<T1, T2, T3> : IMemoryKey, IEquatable<MemoryKey<T1, T2, T3>>
    {
        T1 item1;
        T2 item2;
        T3 item3;

        public MemoryKey(T1 item1, T2 item2, T3 item3)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
        }

        public T1 Item1
        {
            get { return item1; }
        }

        public T2 Item2
        {
            get { return item2; }
        }

        public T3 Item3
        {
            get { return item3; }
        }

        public override int GetHashCode()
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;
            var comparer3 = EqualityComparer<T3>.Default;

            int h0;
            h0 = comparer1.GetHashCode(item1);
            h0 = (h0 << 5) + h0 ^ comparer2.GetHashCode(item2);
            h0 = (h0 << 5) + h0 ^ comparer3.GetHashCode(item3);
            return h0;
        }

        string IMemoryKey.ToString()
        {
            return String.Format("{0}, {1}, {2}", item1, item2, item3);
        }

        public override string ToString()
        {
            return "(" + ((IMemoryKey)this).ToString() + ")";
        }

        public bool Equals(MemoryKey<T1, T2, T3> other)
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;
            var comparer3 = EqualityComparer<T3>.Default;

            return comparer1.Equals(item1, other.item1) &&
                comparer2.Equals(item2, other.item2) &&
                comparer3.Equals(item3, other.item3);
        }

        public override bool Equals(object obj)
        {
            return obj is MemoryKey<T1, T2, T3>
                ? Equals((MemoryKey<T1, T2, T3>)obj)
                : false;
        }
    }

    
    public class MemoryKey<T1, T2, T3, T4> : IMemoryKey, IEquatable<MemoryKey<T1, T2, T3, T4>>
    {
        T1 item1;
        T2 item2;
        T3 item3;
        T4 item4;

        public MemoryKey(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
            this.item4 = item4;
        }

        public T1 Item1
        {
            get { return item1; }
        }

        public T2 Item2
        {
            get { return item2; }
        }

        public T3 Item3
        {
            get { return item3; }
        }

        public T4 Item4
        {
            get { return item4; }
        }

        public override int GetHashCode()
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;
            var comparer3 = EqualityComparer<T3>.Default;
            var comparer4 = EqualityComparer<T4>.Default;

            int h0, h1;
            h0 = comparer1.GetHashCode(item1);
            h0 = (h0 << 5) + h0 ^ comparer2.GetHashCode(item2);
            h1 = comparer3.GetHashCode(item3);
            h1 = (h1 << 5) + h1 ^ comparer4.GetHashCode(item4);
            h0 = (h0 << 5) + h0 ^ h1;
            return h0;
        }

        string IMemoryKey.ToString()
        {
            return String.Format("{0}, {1}, {2}, {3}", item1, item2, item3, item4);
        }

        public override string ToString()
        {
            return "(" + ((IMemoryKey)this).ToString() + ")";
        }

        public bool Equals(MemoryKey<T1, T2, T3, T4> other)
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;
            var comparer3 = EqualityComparer<T3>.Default;
            var comparer4 = EqualityComparer<T4>.Default;

            return comparer1.Equals(item1, other.item1) &&
                comparer2.Equals(item2, other.item2) &&
                comparer3.Equals(item3, other.item3) &&
                comparer4.Equals(item4, other.item4);
        }

        public override bool Equals(object obj)
        {
            return obj is MemoryKey<T1, T2, T3, T4>
                ? Equals((MemoryKey<T1, T2, T3, T4>)obj)
                : false;
        }
    }

    
    public class MemoryKey<T1, T2, T3, T4, T5> : IMemoryKey, IEquatable<MemoryKey<T1, T2, T3, T4, T5>>
    {
        T1 item1;
        T2 item2;
        T3 item3;
        T4 item4;
        T5 item5;

        public MemoryKey(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
            this.item4 = item4;
            this.item5 = item5;
        }

        public T1 Item1
        {
            get { return item1; }
        }

        public T2 Item2
        {
            get { return item2; }
        }

        public T3 Item3
        {
            get { return item3; }
        }

        public T4 Item4
        {
            get { return item4; }
        }

        public T5 Item5
        {
            get { return item5; }
        }

        public override int GetHashCode()
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;
            var comparer3 = EqualityComparer<T3>.Default;
            var comparer4 = EqualityComparer<T4>.Default;
            var comparer5 = EqualityComparer<T5>.Default;

            int h0, h1;
            h0 = comparer1.GetHashCode(item1);
            h0 = (h0 << 5) + h0 ^ comparer2.GetHashCode(item2);
            h1 = comparer3.GetHashCode(item3);
            h1 = (h1 << 5) + h1 ^ comparer4.GetHashCode(item4);
            h0 = (h0 << 5) + h0 ^ h1;
            h0 = (h0 << 5) + h0 ^ comparer5.GetHashCode(item5);
            return h0;
        }

        string IMemoryKey.ToString()
        {
            return String.Format("{0}, {1}, {2}, {3}, {4}", item1, item2, item3, item4, item5);
        }

        public override string ToString()
        {
            return "(" + ((IMemoryKey)this).ToString() + ")";
        }

        public bool Equals(MemoryKey<T1, T2, T3, T4, T5> other)
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;
            var comparer3 = EqualityComparer<T3>.Default;
            var comparer4 = EqualityComparer<T4>.Default;
            var comparer5 = EqualityComparer<T5>.Default;

            return comparer1.Equals(item1, other.Item1) &&
                comparer2.Equals(item2, other.Item2) &&
                comparer3.Equals(item3, other.Item3) &&
                comparer4.Equals(item4, other.Item4) &&
                comparer5.Equals(item5, other.Item5);
        }

        public override bool Equals(object obj)
        {
            return obj is MemoryKey<T1, T2, T3, T4, T5>
                ? Equals((MemoryKey<T1, T2, T3, T4, T5>)obj)
                : false;
        }
    }

    
    public class MemoryKey<T1, T2, T3, T4, T5, T6> : IMemoryKey, IEquatable<MemoryKey<T1, T2, T3, T4, T5, T6>>
    {
        T1 item1;
        T2 item2;
        T3 item3;
        T4 item4;
        T5 item5;
        T6 item6;

        public MemoryKey(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
            this.item4 = item4;
            this.item5 = item5;
            this.item6 = item6;
        }

        public T1 Item1
        {
            get { return item1; }
        }

        public T2 Item2
        {
            get { return item2; }
        }

        public T3 Item3
        {
            get { return item3; }
        }

        public T4 Item4
        {
            get { return item4; }
        }

        public T5 Item5
        {
            get { return item5; }
        }

        public T6 Item6
        {
            get { return item6; }
        }

        public override int GetHashCode()
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;
            var comparer3 = EqualityComparer<T3>.Default;
            var comparer4 = EqualityComparer<T4>.Default;
            var comparer5 = EqualityComparer<T5>.Default;
            var comparer6 = EqualityComparer<T6>.Default;

            int h0, h1;
            h0 = comparer1.GetHashCode(item1);
            h0 = (h0 << 5) + h0 ^ comparer2.GetHashCode(item2);
            h1 = comparer3.GetHashCode(item3);
            h1 = (h1 << 5) + h1 ^ comparer4.GetHashCode(item4);
            h0 = (h0 << 5) + h0 ^ h1;
            h1 = comparer5.GetHashCode(item5);
            h1 = (h1 << 5) + h1 ^ comparer6.GetHashCode(item6);
            h0 = (h0 << 5) + h0 ^ h1;
            return h0;
        }

        string IMemoryKey.ToString()
        {
            return String.Format("{0}, {1}, {2}, {3}, {4}, {5}", item1, item2, item3, item4, item5, item6);
        }

        public override string ToString()
        {
            return "(" + ((IMemoryKey)this).ToString() + ")";
        }

        public bool Equals(MemoryKey<T1, T2, T3, T4, T5, T6> other)
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;
            var comparer3 = EqualityComparer<T3>.Default;
            var comparer4 = EqualityComparer<T4>.Default;
            var comparer5 = EqualityComparer<T5>.Default;
            var comparer6 = EqualityComparer<T6>.Default;

            return comparer1.Equals(item1, other.Item1) &&
                comparer2.Equals(item2, other.Item2) &&
                comparer3.Equals(item3, other.Item3) &&
                comparer4.Equals(item4, other.Item4) &&
                comparer5.Equals(item5, other.Item5) &&
                comparer6.Equals(item6, other.Item6);
        }

        public override bool Equals(object obj)
        {
            return obj is MemoryKey<T1, T2, T3, T4, T5, T6>
                ? Equals((MemoryKey<T1, T2, T3, T4, T5, T6>)obj)
                : false;
        }
    }

    
    public class MemoryKey<T1, T2, T3, T4, T5, T6, T7> : IMemoryKey, IEquatable<MemoryKey<T1, T2, T3, T4, T5, T6, T7>>
    {
        T1 item1;
        T2 item2;
        T3 item3;
        T4 item4;
        T5 item5;
        T6 item6;
        T7 item7;

        public MemoryKey(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;
            this.item4 = item4;
            this.item5 = item5;
            this.item6 = item6;
            this.item7 = item7;
        }

        public T1 Item1
        {
            get { return item1; }
        }

        public T2 Item2
        {
            get { return item2; }
        }

        public T3 Item3
        {
            get { return item3; }
        }

        public T4 Item4
        {
            get { return item4; }
        }

        public T5 Item5
        {
            get { return item5; }
        }

        public T6 Item6
        {
            get { return item6; }
        }

        public T7 Item7
        {
            get { return item7; }
        }

        public override int GetHashCode()
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;
            var comparer3 = EqualityComparer<T3>.Default;
            var comparer4 = EqualityComparer<T4>.Default;
            var comparer5 = EqualityComparer<T5>.Default;
            var comparer6 = EqualityComparer<T6>.Default;
            var comparer7 = EqualityComparer<T7>.Default;

            int h0, h1;
            h0 = comparer1.GetHashCode(item1);
            h0 = (h0 << 5) + h0 ^ comparer2.GetHashCode(item2);
            h1 = comparer3.GetHashCode(item3);
            h1 = (h1 << 5) + h1 ^ comparer4.GetHashCode(item4);
            h0 = (h0 << 5) + h0 ^ h1;
            h1 = comparer5.GetHashCode(item5);
            h1 = (h1 << 5) + h1 ^ comparer6.GetHashCode(item6);
            h1 = (h1 << 5) + h1 ^ comparer7.GetHashCode(item7);
            h0 = (h0 << 5) + h0 ^ h1;
            return h0;
        }


        string IMemoryKey.ToString()
        {
            return String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}", item1, item2, item3, item4, item5, item6, item7);
        }

        public override string ToString()
        {
            return "(" + ((IMemoryKey)this).ToString() + ")";
        }

        public bool Equals(MemoryKey<T1, T2, T3, T4, T5, T6, T7> other)
        {
            var comparer1 = EqualityComparer<T1>.Default;
            var comparer2 = EqualityComparer<T2>.Default;
            var comparer3 = EqualityComparer<T3>.Default;
            var comparer4 = EqualityComparer<T4>.Default;
            var comparer5 = EqualityComparer<T5>.Default;
            var comparer6 = EqualityComparer<T6>.Default;
            var comparer7 = EqualityComparer<T7>.Default;

            return comparer1.Equals(item1, other.Item1) &&
                comparer2.Equals(item2, other.Item2) &&
                comparer3.Equals(item3, other.Item3) &&
                comparer4.Equals(item4, other.Item4) &&
                comparer5.Equals(item5, other.Item5) &&
                comparer6.Equals(item6, other.Item6) &&
                comparer7.Equals(item7, other.Item7);
        }

        public override bool Equals(object obj)
        {
            return obj is MemoryKey<T1, T2, T3, T4, T5, T6, T7>
                ? Equals((MemoryKey<T1, T2, T3, T4, T5, T6, T7>)obj)
                : false;
        }
    }

    public static class MemoryKeyExtensions
    {
        public static IEnumerable<TValue> Get<TKey1, TKey2, TValue>(this ILookup<MemoryKey<TKey1, TKey2>, TValue> lookup, TKey1 tKey1, TKey2 tKey2)
        {
            return lookup[MemoryKey.Create(tKey1, tKey2)];
        }

        public static IEnumerable<TValue> Get<TKey1, TKey2, TKey3, TValue>(this ILookup<MemoryKey<TKey1, TKey2, TKey3>, TValue> lookup, TKey1 tKey1, TKey2 tKey2, TKey3 tKey3)
        {
            return lookup[MemoryKey.Create(tKey1, tKey2, tKey3)];
        }

        public static IEnumerable<TValue> Get<TKey1, TKey2, TKey3, TKey4, TValue>(this ILookup<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TValue> lookup, TKey1 tKey1, TKey2 tKey2, TKey3 tKey3, TKey4 tKey4)
        {
            return lookup[MemoryKey.Create(tKey1, tKey2, tKey3, tKey4)];
        }

        public static IEnumerable<TValue> Get<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(this ILookup<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TValue> lookup, TKey1 tKey1, TKey2 tKey2, TKey3 tKey3, TKey4 tKey4, TKey5 tKey5)
        {
            return lookup[MemoryKey.Create(tKey1, tKey2, tKey3, tKey4, tKey5)];
        }

        public static IEnumerable<TValue> Get<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TValue>(this ILookup<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TValue> lookup, TKey1 tKey1, TKey2 tKey2, TKey3 tKey3, TKey4 tKey4, TKey5 tKey5, TKey6 tKey6)
        {
            return lookup[MemoryKey.Create(tKey1, tKey2, tKey3, tKey4, tKey5, tKey6)];
        }

        public static IEnumerable<TValue> Get<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue>(this ILookup<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TValue> lookup, TKey1 tKey1, TKey2 tKey2, TKey3 tKey3, TKey4 tKey4, TKey5 tKey5, TKey6 tKey6, TKey7 tKey7)
        {
            return lookup[MemoryKey.Create(tKey1, tKey2, tKey3, tKey4, tKey5, tKey6, tKey7)];
        }

        public static TValue GetValueOrDefault<TKey1, TKey2, TValue>(this IDictionary<MemoryKey<TKey1, TKey2>, TValue> dictionary, TKey1 tKey1, TKey2 tKey2, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(MemoryKey.Create(tKey1, tKey2), out value)
                ? value
                : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey1, TKey2, TKey3, TValue>(this IDictionary<MemoryKey<TKey1, TKey2, TKey3>, TValue> dictionary, TKey1 tKey1, TKey2 tKey2, TKey3 tKey3, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(MemoryKey.Create(tKey1, tKey2, tKey3), out value)
                ? value
                : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey1, TKey2, TKey3, TKey4, TValue>(this IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4>, TValue> dictionary, TKey1 tKey1, TKey2 tKey2, TKey3 tKey3, TKey4 tKey4, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(MemoryKey.Create(tKey1, tKey2, tKey3, tKey4), out value)
                ? value
                : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(this IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5>, TValue> dictionary, TKey1 tKey1, TKey2 tKey2, TKey3 tKey3, TKey4 tKey4, TKey5 tKey5, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(MemoryKey.Create(tKey1, tKey2, tKey3, tKey4, tKey5), out value)
                ? value
                : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TValue>(this IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>, TValue> dictionary, TKey1 tKey1, TKey2 tKey2, TKey3 tKey3, TKey4 tKey4, TKey5 tKey5, TKey6 tKey6, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(MemoryKey.Create(tKey1, tKey2, tKey3, tKey4, tKey5, tKey6), out value)
                ? value
                : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TValue>(this IDictionary<MemoryKey<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7>, TValue> dictionary, TKey1 tKey1, TKey2 tKey2, TKey3 tKey3, TKey4 tKey4, TKey5 tKey5, TKey6 tKey6, TKey7 tKey7, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(MemoryKey.Create(tKey1, tKey2, tKey3, tKey4, tKey5, tKey6, tKey7), out value)
                ? value
                : defaultValue;
        }
    }

}
