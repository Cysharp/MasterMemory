using MessagePack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MasterMemory.Internal
{
    // The header is a map of table name to [offset, count]. It is written and read token by token, so it needs no
    // formatter, and nothing that Native AOT or IL2CPP would have to construct at run time.
    internal static class DatabaseHeader
    {
        // the longest form of a map header, of the header of a string and of an int32, and the one form of an array of two
        const int MaxHeaderLength = 5;
        const int ForcedInt32Length = 5;
        const int ArrayOfTwoLength = 1;

        // Written with MessagePackPrimitives, like it is read: each writes one token into a span and says how long it was.
        public static void Write(Stream stream, Dictionary<string, (int offset, int count)> header)
        {
            // the most the header can be, so that it is written into one buffer and no token has to ask for room
            var capacity = MaxHeaderLength;
            foreach (var item in header)
            {
                capacity = checked(capacity + MaxHeaderLength + Encoding.UTF8.GetByteCount(item.Key) + ArrayOfTwoLength + ForcedInt32Length + ForcedInt32Length);
            }

            var buffer = ArrayPool<byte>.Shared.Rent(capacity);
            try
            {
                var written = 0;
                Expect(MessagePackPrimitives.TryWriteMapHeader(buffer.AsSpan(written), header.Count, out var tokenSize));
                written += tokenSize;

                foreach (var item in header)
                {
                    Expect(MessagePackPrimitives.TryWriteString(buffer.AsSpan(written), item.Key, out tokenSize));
                    written += tokenSize;
                    Expect(MessagePackPrimitives.TryWriteArrayHeader(buffer.AsSpan(written), 2, out tokenSize));
                    written += tokenSize;

                    // the fixed-width form, as every earlier version wrote it: the header stays byte for byte the same
                    Expect(MessagePackPrimitives.TryWriteForcedInt32(buffer.AsSpan(written), item.Value.offset, out tokenSize));
                    written += tokenSize;
                    Expect(MessagePackPrimitives.TryWriteForcedInt32(buffer.AsSpan(written), item.Value.count, out tokenSize));
                    written += tokenSize;
                }

                stream.Write(buffer, 0, written);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        // a write primitive says false when the span is too short for the token, which the capacity above rules out
        static void Expect(bool written)
        {
            if (!written)
            {
                throw new InvalidOperationException("The database header did not fit the buffer it was sized for.");
            }
        }

        // an array converts to memory and to a span alike, so it has an overload of its own
        public static Dictionary<string, (int offset, int count)> Read(byte[] databaseBinary, out int headerLength)
        {
            return Read(new ReadOnlySpan<byte>(databaseBinary), out headerLength);
        }

        public static Dictionary<string, (int offset, int count)> Read(ReadOnlyMemory<byte> databaseBinary, out int headerLength)
        {
            return Read(databaseBinary.Span, out headerLength);
        }

        // Read with MessagePackPrimitives, which decode one token from a span and say how long it was: the same code on
        // every target framework, with no reader object and nothing between it and the bytes. The primitives do not
        // throw; what is wrong with a header is said here.
        public static Dictionary<string, (int offset, int count)> Read(ReadOnlySpan<byte> databaseBinary, out int headerLength)
        {
            var unread = databaseBinary;

            Expect(MessagePackPrimitives.TryReadMapHeader(unread, out var count, out var tokenSize), "the number of tables");
            unread = unread.Slice(tokenSize);

            // The count is what the binary says. An entry takes at least four bytes (a name, an array header, two
            // numbers), so a count the rest of the binary has no room for is a damaged one: it must be neither the
            // size of an allocation nor the length of a loop.
            if (count > unread.Length / 4)
            {
                throw new InvalidOperationException($"Invalid database header, it lists {count} tables and the binary is {databaseBinary.Length} bytes. The binary may be cut short or damaged.");
            }

            var header = new Dictionary<string, (int offset, int count)>(count);
            for (int i = 0; i < count; i++)
            {
                Expect(MessagePackPrimitives.TryReadString(unread, out var tableName, out tokenSize), "a table name");
                unread = unread.Slice(tokenSize);
                if (tableName == null)
                {
                    throw new InvalidOperationException("Invalid database header, table name is nil.");
                }

                Expect(MessagePackPrimitives.TryReadArrayHeader(unread, out var locationLength, out tokenSize), "the location of the table '" + tableName + "'");
                unread = unread.Slice(tokenSize);
                if (locationLength != 2)
                {
                    throw new InvalidOperationException("Invalid database header, the table location must be [offset, count]. TableName: " + tableName);
                }

                Expect(MessagePackPrimitives.TryReadInt32(unread, out var offset, out tokenSize), "the offset of the table '" + tableName + "'");
                unread = unread.Slice(tokenSize);
                Expect(MessagePackPrimitives.TryReadInt32(unread, out var length, out tokenSize), "the length of the table '" + tableName + "'");
                unread = unread.Slice(tokenSize);

                if (header.ContainsKey(tableName))
                {
                    throw new InvalidOperationException("Invalid database header, a table is listed twice. TableName: " + tableName);
                }
                header.Add(tableName, (offset, length));
            }

            headerLength = databaseBinary.Length - unread.Length;
            return header;
        }

        static void Expect(DecodeResult result, string what)
        {
            if (result == DecodeResult.Success)
            {
                return;
            }

            throw new InvalidOperationException(result == DecodeResult.InsufficientBuffer
                ? $"Invalid database header, the binary ends where {what} should be. The binary may be cut short or damaged."
                : $"Invalid database header, what is where {what} should be is something else. The binary may be damaged, or not a database.");
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
