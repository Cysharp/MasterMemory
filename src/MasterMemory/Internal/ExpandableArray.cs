using System;

namespace MasterMemory.Internal
{
    internal struct ExpandableArray<TElement>
    {
        internal TElement[] items;
        internal int count;

        public ExpandableArray(object dummy)
        {
            items = Array.Empty<TElement>();
            count = 0;
        }

        internal void Add(TElement item)
        {
            if (items == null || items.Length == 0)
            {
                items = new TElement[4];
            }
            else if (items.Length == (count + 1))
            {
                Array.Resize(ref items, checked(count * 2));
            }
            items[count++] = item;
        }
    }
}