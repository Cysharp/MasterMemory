using MasterMemory.Internal;
using System;
using System.Collections.Generic;

namespace MasterMemory
{
    public abstract class ImmutableBuilderBase
    {
        static protected TElement[] CloneAndSortBy<TElement, TKey>(IList<TElement> elementData, Func<TElement, TKey> indexSelector, IComparer<TKey> comparer)
        {
            var array = new TElement[elementData.Count];
            var sortSource = new TKey[elementData.Count];
            for (int i = 0; i < elementData.Count; i++)
            {
                array[i] = elementData[i];
                sortSource[i] = indexSelector(elementData[i]);
            }

            Array.Sort(sortSource, array, 0, array.Length, comparer);
            return array;
        }

        static protected List<TElement> RemoveCore<TElement, TKey>(TElement[] array, TKey[] keys, Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            var removeIndexes = new HashSet<int>();
            foreach (var key in keys)
            {
                var index = BinarySearch.FindFirst(array, key, keySelector, comparer);
                if (index != -1)
                {
                    removeIndexes.Add(index);
                }
            }

            var newList = new List<TElement>(array.Length - removeIndexes.Count);
            for (int i = 0; i < array.Length; i++)
            {
                if (!removeIndexes.Contains(i))
                {
                    newList.Add(array[i]);
                }
            }

            return newList;
        }

        static protected List<TElement> DiffCore<TElement, TKey>(TElement[] array, TElement[] addOrReplaceData, Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            var newList = new List<TElement>(array.Length);
            var replaceIndexes = new Dictionary<int, TElement>();
            foreach (var data in addOrReplaceData)
            {
                var index = BinarySearch.FindFirst(array, keySelector(data), keySelector, comparer);
                if (index != -1)
                {
                    replaceIndexes.Add(index, data);
                }
                else
                {
                    newList.Add(data);
                }
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (replaceIndexes.TryGetValue(i, out var data))
                {
                    newList.Add(data);
                }
                else
                {
                    newList.Add(array[i]);
                }
            }

            return newList;
        }
    }
}