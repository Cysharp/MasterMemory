using System;
using System.Collections;
using System.Collections.Generic;

namespace MasterMemory
{
    public readonly struct RangeView<T> : IEnumerable<T>, IReadOnlyList<T>
    {
        public static RangeView<T> Empty => default(RangeView<T>);

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
            this.hasValue = (orderedData.Length != 0) && (left <= right); // same index is length = 1
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
    }
}