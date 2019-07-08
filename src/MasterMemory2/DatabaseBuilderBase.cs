using MasterMemory.Annotations;
using MasterMemory.Internal;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MasterMemory
{
    public abstract class DatabaseBuilderBase
    {
        byte[] byteBuffer = new byte[1024];
        int index = 0;

        // TableName, (Offset, Count)
        readonly Dictionary<string, (int offset, int count)> header = new Dictionary<string, (int offset, int count)>();
        readonly IFormatterResolver resolver;

        public DatabaseBuilderBase()
            : this(null)
        {
        }

        public DatabaseBuilderBase(IFormatterResolver resolver)
        {
            this.resolver = resolver;
        }

        protected void AppendCore<T, TKey>(IEnumerable<T> datasource, Func<T, TKey> indexSelector)
        {
            var tableName = typeof(T).GetCustomAttribute<MemoryTableAttribute>();
            if (tableName == null) throw new InvalidOperationException("Type is not annotated MemoryTableAttribute. Type:" + typeof(T).FullName);

            // sort(as indexed data-table)
            var source = FastSort(datasource, indexSelector, Comparer<TKey>.Default);

            // write data and store header-data.
            var offset = index;
            var count = LZ4MessagePackSerializer.SerializeToBlock(ref byteBuffer, index, source, resolver ?? MessagePackSerializer.DefaultResolver);
            header.Add(tableName.TableName, (offset, count));
            index += count;
        }

        static TElement[] FastSort<TElement, TKey>(IEnumerable<TElement> datasource, Func<TElement, TKey> indexSelector, IComparer<TKey> comparer)
        {
            var collection = datasource as ICollection<TElement>;
            if (collection != null)
            {
                var array = new TElement[collection.Count];
                var sortSource = new TKey[collection.Count];
                var i = 0;
                foreach (var item in collection)
                {
                    array[i] = item;
                    sortSource[i] = indexSelector(item);
                    i++;
                }
                Array.Sort(sortSource, array, 0, collection.Count, comparer);
                return array;
            }
            else
            {
                var array = new ExpandableArray<TElement>();
                var sortSource = new ExpandableArray<TKey>();
                foreach (var item in datasource)
                {
                    array.Add(item);
                    sortSource.Add(indexSelector(item));
                }

                Array.Sort(sortSource.items, array.items, 0, array.count, comparer);

                Array.Resize(ref array.items, array.count);
                return array.items;
            }
        }

        public byte[] Build()
        {
            var headerFormatter = new DictionaryFormatter<string, (int, int)>();
            byte[] result = null;
            var offset = headerFormatter.Serialize(ref result, 0, header, new HeaderFormatterResolver());

            var finalSize = offset + index;
            if (result.Length - offset < index) // TODO: is this ok?
            {
                Array.Resize(ref result, finalSize);
            }

            Buffer.BlockCopy(byteBuffer, 0, result, offset, index);

            if (result.Length != finalSize)
            {
                Array.Resize(ref result, finalSize);
            }

            return result;
        }

        public void WriteToStream(Stream stream)
        {
            MessagePackSerializer.Serialize(stream, header, new HeaderFormatterResolver());
            stream.Write(byteBuffer, 0, index);
        }
    }
}