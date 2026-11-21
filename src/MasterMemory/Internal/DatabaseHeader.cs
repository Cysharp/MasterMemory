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
            var header = new Dictionary<string, (int offset, int count)>(count);
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
                header.Add(tableName, (offset, length));
            }

            headerLength = checked((int)reader.BytesConsumed);
            return header;
        }
    }
}
