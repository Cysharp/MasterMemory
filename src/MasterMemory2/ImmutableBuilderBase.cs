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

    public sealed class ImmutableBuilder : ImmutableBuilderBase
    {
        MemoryDatabase memory;

        public ImmutableBuilder(MemoryDatabase memory)
        {
            this.memory = memory;
        }

        public MemoryDatabase Build()
        {
            return memory;
        }

        public void ReplaceAll(System.Collections.Generic.IList<MyClass> data)
        {
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new MyClassTable(newData);
            memory = new MemoryDatabase(
                table

            );
        }

        public void RemoveMyClass(int[] keys)
        {
            var data = RemoveCore(memory.MyClassTable.GetRawDataUnsafe(), keys, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new MyClassTable(newData);
            memory = new MemoryDatabase(
                table

            );
        }

        public void Diff(MyClass[] addOrReplaceData)
        {
            var data = DiffCore(memory.MyClassTable.GetRawDataUnsafe(), addOrReplaceData, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var newData = CloneAndSortBy(data, x => x.Id, System.Collections.Generic.Comparer<int>.Default);
            var table = new MyClassTable(newData);
            memory = new MemoryDatabase(
                table

            );
        }

    }
}