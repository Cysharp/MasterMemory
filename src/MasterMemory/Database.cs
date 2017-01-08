using MasterMemory;
using System;
using System.Collections.Generic;
using ZeroFormatter;
using ZeroFormatter.Formatters;
using ZeroFormatter.Internal;

namespace MasterMemory
{
    // Memory Layout:
    // [MemoryCount:int][(key:string,memoryOffset:index)][memory...]

    public class Database
    {
        readonly Dictionary<string, ISerializableMemory> memories; // as readonly
        readonly bool useNullTracker;

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
                ISerializableMemory obj;
                if (!memories.TryGetValue(memoryKey, out obj))
                {
                    throw new KeyNotFoundException("MemoryKey Not Found:" + memoryKey);
                }

                var byteOffsetMemory = obj as ArraySegmentMemory;
                if (byteOffsetMemory != null)
                {
                    var tracker = useNullTracker ? DirtyTracker.NullTracker : new DirtyTracker();
                    var memory = byteOffsetMemory.ToMemory<TKey, TElement>(tracker, indexSelector);
                    memories[memoryKey] = memory;
                    return memory;
                }
                else
                {
                    var memory = obj as Memory<TKey, TElement>;
                    if (memory == null)
                    {
                        throw new ArgumentException("Cast type is invalid, actual memory type:" + obj.GetType().Name);
                    }
                    return memory;
                }
            }
        }

        internal Database(IEnumerable<KeyValuePair<string, ISerializableMemory>> memories)
        {
            this.memories = new Dictionary<string, ISerializableMemory>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in memories)
            {
                this.memories.Add(item.Key, item.Value);
            }
            this.useNullTracker = true;
        }

        Database(Dictionary<string, ISerializableMemory> memories, bool useNullTracker)
        {
            this.memories = memories;
            this.useNullTracker = useNullTracker;
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
                offset += ZeroFormatter.Internal.BinaryUtil.WriteInt32(ref bytes, 0, MemoryCount);

                // headers
                foreach (var item in memories)
                {
                    var stringFormatter = ZeroFormatter.Formatters.Formatter<DefaultResolver, string>.Default;
                    offset += stringFormatter.Serialize(ref bytes, offset, item.Key);
                    list.Add(new HeaderRecord { HeaderOffset = offset, Memory = item.Value });
                    offset += 4;
                }

                // memories
                foreach (var item in list)
                {
                    BinaryUtil.WriteInt32Unsafe(ref bytes, item.HeaderOffset, offset);
                    offset += item.Memory.Serialize(ref bytes, offset);
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

        struct HeaderRecord
        {
            public int HeaderOffset;
            public ISerializableMemory Memory;
        }
    }

    public class DatabaseBuilder
    {
        readonly Dictionary<string, ISerializableMemory> memories = new Dictionary<string, ISerializableMemory>(StringComparer.OrdinalIgnoreCase);

        public DatabaseBuilder()
        {

        }

        internal void InternalAdd(string key, ISerializableMemory memory)
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