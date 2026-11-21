using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;

namespace MasterMemory.Internal
{
    // The header is a map of table name to [offset, count]. It is written and read token by token, so it needs no
    // formatter, and nothing that Native AOT or IL2CPP would have to construct at run time.
    internal static class DatabaseHeader
    {
        public static void Write(Stream stream, Dictionary<string, (int offset, int count)> header)
        {
            using var writer = new MessagePackWriter();
            writer.WriteMapHeader(header.Count);
            foreach (var item in header)
            {
                writer.WriteString(item.Key);
                writer.WriteArrayHeader(2);
                // the fixed-width form, as every earlier version wrote it: the header stays byte for byte the same
                writer.WriteForcedInt32(item.Value.offset);
                writer.WriteForcedInt32(item.Value.count);
            }
            writer.WriteTo(stream);
        }

        public static Dictionary<string, (int offset, int count)> Read(byte[] databaseBinary, out int headerLength)
        {
            using var reader = new MessagePackReader(new ReadOnlyMemory<byte>(databaseBinary));

            var count = reader.ReadMapHeader();

            // The count is what the binary says, so it is not what the dictionary is sized by: an entry takes at least
            // four bytes (a name, an array header, two numbers), and a damaged count must not be an allocation.
            var header = new Dictionary<string, (int offset, int count)>(Math.Min(count, databaseBinary.Length / 4));
            for (int i = 0; i < count; i++)
            {
                var tableName = reader.ReadString();
                if (tableName == null)
                {
                    throw new InvalidOperationException("Invalid database header, table name is nil.");
                }

                if (reader.ReadArrayHeader() != 2)
                {
                    throw new InvalidOperationException("Invalid database header, the table location must be [offset, count]. TableName: " + tableName);
                }

                var offset = reader.ReadInt32();
                var length = reader.ReadInt32();
                if (header.ContainsKey(tableName))
                {
                    throw new InvalidOperationException("Invalid database header, a table is listed twice. TableName: " + tableName);
                }
                header.Add(tableName, (offset, length));
            }

            headerLength = checked((int)reader.BytesConsumed);
            return header;
        }

        // A header that places a table outside the data is what a file cut short looks like, or a damaged one. Slicing
        // would say only that an argument was out of range.
        public static void ThrowIfOutside(string tableName, int offset, int count, int dataLength)
        {
            if (offset < 0 || count < 0 || offset > dataLength || count > dataLength - offset)
            {
                throw new InvalidOperationException($"Invalid database, the header places the table '{tableName}' at offset {offset} with {count} bytes, and the data after the header is {dataLength} bytes. The binary may be cut short or damaged.");
            }
        }
    }
}
