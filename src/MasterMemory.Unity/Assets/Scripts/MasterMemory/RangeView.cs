using System;
using System.Collections;
using System.Collections.Generic;

namespace MasterMemory
{
    public readonly struct RangeView<T> : IEnumerable<T>, IReadOnlyList<T>, IList<T>
    {
        public static RangeView<T> Empty => new RangeView<T>( null, -1, -1, false ); 

        readonly T[] orderedData;
        readonly int left;
        readonly int right;
        readonly bool ascendant;
        readonly bool hasValue;

        public int Count => (!hasValue) ? 0 : (right - left) + 1;
        public T First => this[0];
        public T Last => this[Count - 1];

        public RangeView<T> Reverse => new RangeView<T>(orderedData, left, right, !ascendant);

        internal int FirstIndex => ascendant ? left : right;
        internal int LastIndex => ascendant ? right : left;

        bool ICollection<T>.IsReadOnly => true;

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

        public RangeView(T[] orderedData, int left, int right, bool ascendant)
        {
            this.hasValue = (orderedData != null ) && (orderedData.Length != 0) && (left <= right); // same index is length = 1            this.orderedData = orderedData;
            this.orderedData = orderedData;
            this.left = left;
            this.right = right;
            this.ascendant = ascendant;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Any()
        {
            return Count != 0;
        }

        public int IndexOf(T item)
        {
            var i = 0;
            foreach (var v in this)
            {
                if (EqualityComparer<T>.Default.Equals(v, item))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public bool Contains(T item)
        {
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                var v = this[i];
                if (EqualityComparer<T>.Default.Equals(v, item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var count = Count;
            Array.Copy(orderedData, left, array, arrayIndex, count);
            if (!ascendant)
            {
                Array.Reverse(array, arrayIndex, count);
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
                throw new NotImplementedException();
            }
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

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }
    }
}