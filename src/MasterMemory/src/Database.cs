using MessagePack;
using System;
using System.Collections.Generic;

namespace MasterMemory
{
    // layout: array of (key, byte[]) tuple. (serialize valid messagepack-binary)
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
            return GetMemory(memoryKey, indexSelector, MessagePackSerializer.DefaultResolver);
        }

        public Memory<TKey, TElement> GetMemory<TKey, TElement>(string memoryKey, Func<TElement, TKey> indexSelector, IFormatterResolver resolver = null)
        {
            lock (memories)
            {
                IInternalMemory obj;
                if (!memories.TryGetValue(memoryKey, out obj))
                {
                    throw new KeyNotFoundException("MemoryKey Not Found:" + memoryKey);
                }

                var raw = obj as InternalRawMemory;
                if (raw != null)
                {
                    var orderedData = MessagePackSerializer.Deserialize<TElement[]>(raw.RawMemory, resolver);

                    var mem = new Memory<TKey, TElement>(orderedData, indexSelector, true, true);

                    memories[memoryKey] = mem;
                    obj = raw = null;
                    return mem;
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

        internal Database(IEnumerable<KeyValuePair<string, IInternalMemory>> memories)
        {
            this.memories = new Dictionary<string, IInternalMemory>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in memories)
            {
                this.memories.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Open the database.
        /// </summary>
        public static Database Open(byte[] bytes)
        {
            var offset = 0;
            int readSize;
            var memoryCount = MessagePackBinary.ReadArrayHeader(bytes, 0, out readSize);
            offset += readSize;

            var memories = new KeyValuePair<string, IInternalMemory>[memoryCount];

            for (int i = 0; i < memoryCount; i++)
            {
                var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
                offset += readSize;

                if (len != 2) throw new InvalidOperationException("Invalid MsgPack Binary of Database.");

                var keyName = MessagePackBinary.ReadString(bytes, offset, out readSize);
                offset += readSize;

                var beginOffset = offset;
                var arrayLen = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
                offset += readSize;
                for (int j = 0; j < arrayLen; j++)
                {
                    offset += MessagePackBinary.ReadNextBlock(bytes, offset);
                }
                readSize = offset - beginOffset;
                var binary = new byte[readSize];
                Buffer.BlockCopy(bytes, beginOffset, binary, 0, readSize);

                var memory = new InternalRawMemory(binary);
                memories[i] = new KeyValuePair<string, IInternalMemory>(keyName, memory);
            }

            return new Database(memories);
        }

        // Memo: imple Open(FileStream) for stream read on future.

        public byte[] Save()
        {
            return Save(MessagePackSerializer.DefaultResolver);
        }

        public byte[] Save(IFormatterResolver resolver)
        {
            lock (memories)
            {
                var bytes = new byte[255]; // initial

                var offset = 0;
                offset += MessagePackBinary.WriteArrayHeader(ref bytes, 0, MemoryCount);

                foreach (var item in memories)
                {
                    offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 2);
                    offset += MessagePackBinary.WriteString(ref bytes, offset, item.Key);
                    offset += item.Value.Serialize(ref bytes, offset, resolver);
                }

                MessagePackBinary.FastResize(ref bytes, offset);
                return bytes;
            }
        }

        public int SaveToFile(System.IO.FileStream fileStream)
        {
            return SaveToFile(fileStream, MessagePackSerializer.DefaultResolver);
        }

        public int SaveToFile(System.IO.FileStream fileStream, IFormatterResolver resolver)
        {
            lock (memories)
            {
                var bytes = new byte[255]; // buffer

                var size = 0;
                var offset = 0;
                offset += MessagePackBinary.WriteArrayHeader(ref bytes, 0, MemoryCount);

                fileStream.Write(bytes, 0, offset); // write header.
                size += offset;

                foreach (var item in memories)
                {
                    offset = 0;
                    offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, 2);
                    offset += MessagePackBinary.WriteString(ref bytes, offset, item.Key);
                    offset += item.Value.Serialize(ref bytes, offset, resolver);

                    fileStream.Write(bytes, 0, offset);
                    size += offset;
                }

                fileStream.Flush();

                return size;
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

        /// <summary>
        /// Database diagnostics. If allowDump = true, can use MemoryAnalysis.DumpRows(but if true, holds the raw byte[] in memory).
        /// </summary>
        public static MemoryAnalysis[] ReportDiagnostics(byte[] bytes, bool allowDump = false)
        {
            var list = new List<MemoryAnalysis>();

            var db = Database.Open(bytes);
            foreach (var item in db.memories)
            {
                var rawMemory = item.Value as InternalRawMemory;

                int readSize;
                var count = MessagePackBinary.ReadArrayHeader(rawMemory.RawMemory, 0, out readSize);

                var analysis = new MemoryAnalysis(item.Key, count, rawMemory.RawMemory.Length, allowDump ? rawMemory.RawMemory : null);

                list.Add(analysis);
            }

            return list.ToArray();
        }
    }

    public class MemoryAnalysis
    {
        public string KeyName { get; private set; }
        public int Count { get; private set; }
        public long Size { get; private set; }

        readonly byte[] rawArrayBytes;

        public MemoryAnalysis(string keyName, int count, long size, byte[] rawArrayBytes)
        {
            this.KeyName = keyName;
            this.Count = count;
            this.Size = size;
            this.rawArrayBytes = rawArrayBytes;
        }

        public IEnumerable<string> DumpRows()
        {
            if (rawArrayBytes == null) yield break;

            byte[] buffer = new byte[10];

            var offset = 0;
            int readSize;
            var length = MessagePackBinary.ReadArrayHeader(rawArrayBytes, 0, out readSize);
            offset += readSize;

            for (int i = 0; i < length; i++)
            {
                readSize = MessagePackBinary.ReadNextBlock(rawArrayBytes, offset);

                MessagePackBinary.EnsureCapacity(ref buffer, 0, readSize);
                Buffer.BlockCopy(rawArrayBytes, offset, buffer, 0, readSize);

                yield return MessagePackSerializer.ToJson(buffer);
                offset += readSize;
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}]Count:{1}, Size:{2}", KeyName, Count, ToHumanReadableSize(Size));
        }

        static string ToHumanReadableSize(long size)
        {
            double bytes = size;

            if (bytes <= 1024) return bytes.ToString("f2") + " B";

            bytes = bytes / 1024;
            if (bytes <= 1024) return bytes.ToString("f2") + " KB";

            bytes = bytes / 1024;
            if (bytes <= 1024) return bytes.ToString("f2") + " MB";

            bytes = bytes / 1024;
            if (bytes <= 1024) return bytes.ToString("f2") + " GB";

            bytes = bytes / 1024;
            if (bytes <= 1024) return bytes.ToString("f2") + " TB";

            bytes = bytes / 1024;
            if (bytes <= 1024) return bytes.ToString("f2") + " PB";

            bytes = bytes / 1024;
            if (bytes <= 1024) return bytes.ToString("f2") + " EB";

            bytes = bytes / 1024;
            return bytes + " ZB";
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