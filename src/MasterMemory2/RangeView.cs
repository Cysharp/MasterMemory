using System;
using System.Collections;
using System.Collections.Generic;

namespace MasterMemory
{
    public class RangeView<T> : IEnumerable<T>, IReadOnlyList<T>
    {
        readonly T[] orderedData;
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

        internal RangeView(T[] orderedData, int left, int right, bool ascendant)
        {
            if (right < left) hasValue = false;

            this.orderedData = orderedData;
            this.left = left;
            this.right = right;
            this.ascendant = ascendant;
            this.hasValue = orderedData.Length != 0;
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
    }
}