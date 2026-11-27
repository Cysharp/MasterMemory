using System;

namespace MasterMemory.Internal
{
    // A table is one MessagePack message: an array of rows. MasterMemory adds nothing to it. Compression is the
    // application's choice, made by the processor in the MemoryDatabaseOptions it hands over; by default a table is
    // written as it is. This is how a table that is compressed is told from one that is not, and what is said when
    // there is no processor to read it with.
    //
    // The formatter of a table array comes from MessagePack's own registry of generated formatters, where the code
    // generated for a database puts it (RegisterHarvested). The chains MessagePack builds itself (Default, DefaultAot)
    // look there first, and a factory an application gives is a chain around one of those.
    internal static class TableSerialization
    {
        // A table that was compressed with LZ4 (every table of a database built by MasterMemory v3 that is large
        // enough, and any that a v4 builder wrote with an LZ4 processor) is an ext of type 99 or 98 where the array of
        // rows would be. An array never starts with an ext header, so this is not mistaken.
        public static bool LooksCompressed(ReadOnlySpan<byte> table)
        {
            if (table.Length < 3)
            {
                return false;
            }

            // ext 8, ext 16, ext 32: the type follows the length
            int typeAt;
            switch (table[0])
            {
                case 0xC7: typeAt = 2; break;
                case 0xC8: typeAt = 3; break;
                case 0xC9: typeAt = 5; break;
                default: return false;
            }

            return typeAt < table.Length && (table[typeAt] == 99 || table[typeAt] == 98);
        }

        public static InvalidOperationException CompressedWithoutProcessor(string tableName)
        {
            return new InvalidOperationException(
                $"The table '{tableName}' is compressed with LZ4, and the options have no MessageProcessor to read it with. " +
                "A database built by MasterMemory v3 is compressed, and so is one that a builder was given an LZ4 processor for. " +
                "Reference the MessagePack.LZ4 package and pass options that have the processor: " +
                "new MemoryDatabase(binary, new MemoryDatabaseOptions { MessageProcessor = new Lz4BlockProcessor() }). " +
                "The processor reads a table of up to 64 MB by default; new Lz4BlockProcessor(maxDecompressedSize) reads a larger one.");
        }
    }
}
