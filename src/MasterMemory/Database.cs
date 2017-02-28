using MasterMemory;
using MessagePack;
using System;
using System.Collections.Generic;

namespace MasterMemory
{
    public class Database
    {
        readonly Dictionary<string, IInternalMemory> memories; // as readonly

        public int MemoryCount
        {
            get { return memories.Count; }
        }

        public ICollection<string> Keys
        {
            get { return memories.Keys; }
        }

        public Memory<TKey, TElement> GetMemory<TKey, TElement>(string memoryKey, Func<TElement, TKey> indexSelector)
        {
            lock (memories)
            {
                IInternalMemory obj;
                if (!memories.TryGetValue(memoryKey, out obj))
                {
                    throw new KeyNotFoundException("MemoryKey Not Found:" + memoryKey);
                }

                if (!obj.IsRegisteredSelector)
                {
                    obj.InternalSetSelect(indexSelector);
                }

                var memory = obj as Memory<TKey, TElement>;
                if (memory == null)
                {
                    throw new ArgumentException("Cast type is invalid, actual memory type:" + obj.GetType().Name);
                }
                return memory;
            }
        }

        internal Database(IEnumerable<KeyValuePair<string, IInternalMemory>> memories)
        {
            this.memories = new Dictionary<string, IInternalMemory>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in memories)
            {
                this.memories.Add(item.Key, item.Value);
            }
        }
        
        /// <summary>
        /// Create database from underlying bytes.
        /// If all objects are readonly, set guaranteedAllObjectsAreReadonly = true for improve performance.
        /// </summary>
        public static Database Open(byte[] bytes, bool guaranteedAllObjectsAreReadonly = false)
        {
            var memoryCount = BinaryUtil.ReadInt32(ref bytes, 0);
            var stringFormatter = ZeroFormatter.Formatters.Formatter<DefaultResolver, string>.Default;

            var memories = new Dictionary<string, ISerializableMemory>(memoryCount, StringComparer.OrdinalIgnoreCase);
            var offset = 4;

            string prevKeyName = null;
            int prevMemoryOffset = 0;
            for (int i = 0; i < memoryCount; i++)
            {
                int byteSize;
                var keyName = stringFormatter.Deserialize(ref bytes, offset, DirtyTracker.NullTracker, out byteSize);
                offset += byteSize;
                var memoryOffset = BinaryUtil.ReadInt32(ref bytes, offset);
                offset += 4;

                if (memoryCount == 1 || i == memoryCount - 1)
                {
                    memories.Add(keyName, new ArraySegmentMemory(new ArraySegment<byte>(bytes, memoryOffset, bytes.Length - memoryOffset)));
                }
                if (i != 0)
                {
                    memories.Add(prevKeyName, new ArraySegmentMemory(new ArraySegment<byte>(bytes, prevMemoryOffset, memoryOffset - prevMemoryOffset)));
                }

                prevKeyName = keyName;
                prevMemoryOffset = memoryOffset;
            }

            return new Database(memories, guaranteedAllObjectsAreReadonly);
        }

        public byte[] Save()
        {
            lock (memories)
            {
                var bytes = new byte[255]; // initial
                var list = new List<HeaderRecord>();

                var offset = 0;


                offset += MessagePackBinary.WriteMapHeader(ref bytes, offset, memories.Count);
                
                foreach (var item in memories)
                {
                    offset += MessagePackBinary.WriteString(ref bytes, offset, item.Key);

                    


                }

                BinaryUtil.FastResize(ref bytes, offset);
                return bytes;
            }
        }

        public DatabaseBuilder ToBuilder()
        {
            var builder = new DatabaseBuilder();
            foreach (var item in memories)
            {
                builder.InternalAdd(item.Key, item.Value);
            }
            return builder;
        }

        public static Tuple<string, int>[] ReportDiagnostics(byte[] bytes)
        {
            var list = new List<Tuple<string, int>>();

            // TODO:...
            //var db = Database.Open(bytes, true);
            //foreach (var item in db.memories)
            //{
            //    var arraySegmentMemory = item.Value as ArraySegmentMemory;
            //    list.Add(Tuple.Create(item.Key, arraySegmentMemory.GetBuffer().Count));
            //}

            return list.ToArray();
        }

        struct HeaderRecord
        {
            public int HeaderOffset;
            public ISerializableMemory Memory;
        }
    }

    public class DatabaseBuilder
    {
        readonly Dictionary<string, IInternalMemory> memories = new Dictionary<string, IInternalMemory>(StringComparer.OrdinalIgnoreCase);

        public DatabaseBuilder()
        {

        }

        internal void InternalAdd(string key, IInternalMemory memory)
        {
            memories.Add(key, memory);
        }

        public void Add<TKey, TElement>(string key, IEnumerable<TElement> datasource, Func<TElement, TKey> primaryIndexSelector)
        {
            memories.Add(key, new Memory<TKey, TElement>(datasource, primaryIndexSelector));
        }

        public void Replace<TKey, TElement>(string key, IEnumerable<TElement> datasource, Func<TElement, TKey> primaryIndexSelector)
        {
            memories[key] = new Memory<TKey, TElement>(datasource, primaryIndexSelector);
        }

        public void Remove(string key)
        {
            memories.Remove(key);
        }

        public Database Build()
        {
            return new Database(memories);
        }
    }
}