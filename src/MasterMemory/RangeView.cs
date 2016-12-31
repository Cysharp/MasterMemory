using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMemory
{
    // TODO:impl all.

    public struct RangeView<T> : IEnumerable<T>, IList<T>, IReadOnlyList<T>
    {
        readonly IList<T> orderedData;
        readonly int left;
        readonly int right;
        readonly bool ascendant;

        public int Count
        {
            get
            {
                return right - left;
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
            this.orderedData = orderedData;
            this.left = left;
            this.right = right;
            this.ascendant = ascendant;
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        int IList<T>.IndexOf(T item)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
