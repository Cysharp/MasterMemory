using System;
using System.Collections;
using System.Collections.Generic;

namespace MasterMemory
{
    public struct RangeView<T> : IEnumerable<T>, IList<T>, IReadOnlyList<T>
    {
        readonly IList<T> orderedData;
        readonly int left;
        readonly int right;
        readonly bool ascendant;
        readonly bool hasValue;

        public int Count
        {
            get
            {
                return (!hasValue) ? 0 : (right - left) + 1;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        T IList<T>.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public T this[int index]
        {
            get
            {
                if (!hasValue) throw new ArgumentOutOfRangeException("view is empty");
                if (index < 0) throw new ArgumentOutOfRangeException("index < 0");
                if (Count <= index) throw new ArgumentOutOfRangeException("count <= index");

                if (ascendant)
                {
                    return orderedData[left + index];
                }
                else
                {
                    return orderedData[right - index];
                }
            }
        }

        public RangeView(IList<T> orderedData, int left, int right, bool ascendant)
        {
            if (right < left) throw new ArgumentException("right < left," + "right:" + right + " left:" + left);
            this.orderedData = orderedData;
            this.left = left;
            this.right = right;
            this.ascendant = ascendant;
            this.hasValue = orderedData.Count != 0;
        }

        public static RangeView<T> Empty()
        {
            return default(RangeView<T>);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int IList<T>.IndexOf(T item)
        {
            var comaprer = EqualityComparer<T>.Default;
            for (int i = 0; i < Count; i++)
            {
                if (comaprer.Equals(this[i], item)) return i;
            }
            return -1;
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Contains(T item)
        {
            return ((IList<T>)this).IndexOf(item) != -1;
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            var index = 0;
            var c = Count;
            for (int i = arrayIndex; index < c; i++)
            {
                array[i] = this[index++];
            }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
