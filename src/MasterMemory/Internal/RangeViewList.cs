using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MasterMemory.Internal
{
    // What AsReadOnlyList() of a view returns: the rows of the view behind the interfaces that LINQ and other code take.
    // The views themselves implement none, so that a view is never boxed without the code saying so; this object is the
    // one allocation of asking for a list, and it copies no row.
    //
    // It is an IList<T> as well as the IReadOnlyList<T> it is returned as, because IList<T> and ICollection<T> are what
    // LINQ looks for to count, index and copy without enumerating. Nothing can be changed through it.
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(RangeViewListDebugView<>))]
    internal sealed class RangeViewList<T> : IReadOnlyList<T>, IList<T>
    {
        /// <summary>The list of a view that has no rows, either way round: one object for all of them.</summary>
        public static readonly RangeViewList<T> Empty = new RangeViewList<T>(default, reverse: false);

        // the rows in the order of the index, and whether this list runs the other way
        readonly RangeView<T> view;
        readonly bool reverse;

        public RangeViewList(RangeView<T> view, bool reverse)
        {
            this.view = view;
            this.reverse = reverse;
        }

        public int Count => view.Count;

        public T this[int index] => reverse ? view.Reverse[index] : view[index];

        // the enumerator of the view, boxed: it can be reset, which one written with yield cannot
        public IEnumerator<T> GetEnumerator()
        {
            return reverse ? view.Reverse.GetEnumerator() : view.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return reverse ? view.Reverse.IndexOf(item) : view.IndexOf(item);
        }

        public bool Contains(T item)
        {
            return view.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (reverse)
            {
                view.Reverse.CopyTo(array, arrayIndex);
            }
            else
            {
                view.CopyTo(array, arrayIndex);
            }
        }

        public T[] ToArray()
        {
            return reverse ? view.Reverse.ToArray() : view.ToArray();
        }

        bool ICollection<T>.IsReadOnly => true;

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

    internal sealed class RangeViewListDebugView<T>
    {
        readonly T[] items;

        public RangeViewListDebugView(RangeViewList<T> list)
        {
            this.items = list.ToArray();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => items;
    }
}
