using MasterMemory.Internal;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MasterMemory
{
    public abstract class DatabaseBuilderBase
    {
        readonly ByteBufferWriter bufferWriter = new ByteBufferWriter();

        // TableName, (Offset, Count)
        readonly Dictionary<string, (int offset, int count)> header = new Dictionary<string, (int offset, int count)>();
        readonly MessagePackSerializerOptions options;

        public DatabaseBuilderBase(MessagePackSerializerOptions options)
        {
            // options keep null to lazily use default options
            if (options != null)
            {
                options = options.WithCompression(MessagePackCompression.Lz4Block);
            }
        }

        public DatabaseBuilderBase(IFormatterResolver resolver)
        {
            if (resolver != null)
            {
                this.options = MessagePackSerializer.DefaultOptions
                    .WithCompression(MessagePackCompression.Lz4Block)
                    .WithResolver(resolver);
            }

        }

        protected void AppendCore<T, TKey>(IEnumerable<T> datasource, Func<T, TKey> indexSelector, IComparer<TKey> comparer)
        {
            var tableName = typeof(T).GetCustomAttribute<MemoryTableAttribute>();
            if (tableName == null) throw new InvalidOperationException("Type is not annotated MemoryTableAttribute. Type:" + typeof(T).FullName);

            if (header.ContainsKey(tableName.TableName))
            {
                throw new InvalidOperationException("TableName is already appended in builder. TableName: " + tableName.TableName + " Type:" + typeof(T).FullName);
            }

            if (datasource == null) return;

            // sort(as indexed data-table)
            var source = FastSort(datasource, indexSelector, comparer);

            // write data and store header-data.
            var useOption = options ?? MessagePackSerializer.DefaultOptions.WithCompression(MessagePackCompression.Lz4Block);

            var offset = bufferWriter.CurrentOffset;
            MessagePackSerializer.Serialize(bufferWriter, source, useOption);

            header.Add(tableName.TableName, (offset, bufferWriter.CurrentOffset - offset));
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
                var array = new ExpandableArray<TElement>(null);
                var sortSource = new ExpandableArray<TKey>(null);
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
            using (var ms = new MemoryStream())
            {
                WriteToStream(ms);
                return ms.ToArray();
            }
        }

        public void WriteToStream(Stream stream)
        {
            MessagePackSerializer.Serialize(stream, header, HeaderFormatterResolver.StandardOptions);
            MemoryMarshal.TryGetArray(bufferWriter.WrittenMemory, out var segment);
            stream.Write(segment.Array, segment.Offset, segment.Count);
        }
    }
}