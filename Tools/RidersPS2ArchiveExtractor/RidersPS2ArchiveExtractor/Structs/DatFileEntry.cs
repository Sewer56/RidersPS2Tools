using System;
using System.Text;
using Reloaded.Memory.Streams;

namespace RidersPS2ArchiveTool.Structs
{
    public unsafe struct DatFileEntry
    {
        public const int SECTOR_SIZE_BYTES = 2048;

        /// <summary>
        /// Pointer to the name of the file, a null terminated C string.
        /// </summary>
        public int NamePtr;

        /// <summary>
        /// <see cref="SizeBytes"/> multiplied by 16.
        /// </summary>
        public int SizeMulBy16;

        /// <summary>
        /// File offset in .DAT divided by 2048.
        /// </summary>
        public int Sector;

        /// <summary>
        /// Size of the file.
        /// </summary>
        public int SizeBytes;

        public DatFileEntry(int namePtr, int sector, int sizeBytes)
        {
            NamePtr = namePtr;
            SizeMulBy16 = sizeBytes * 16;
            Sector = sector;
            SizeBytes = sizeBytes;
        }

        /// <summary>
        /// Converts the name behind the pointer to a string.
        /// </summary>
        public string GetName(BufferedStreamReader streamReader, Func<int, int> memoryToRawAddress)
        {
            return Utilities.Utilities.GetString(streamReader, (long)memoryToRawAddress(NamePtr), Encoding.ASCII);
        }

        /// <summary>
        /// Returns true if the current archive is an entry for the specified archive name.
        /// </summary>
        public bool IsFirstEntry(BufferedStreamReader streamReader, Func<int, int> memoryToRawAddress, DatFileEntry nextEntry, string expectedArchiveName)
        {
            return Sector == 0 && (GetName(streamReader, memoryToRawAddress) == expectedArchiveName) 
                               && SizeBytes == nextEntry.SizeBytes 
                               && SizeMulBy16 == nextEntry.SizeMulBy16;
        }

        /// <summary>
        /// True if this is the last/dummy entry, else false.
        /// </summary>
        public bool IsLastEntry()
        {
            return (long)NamePtr == 0 && Sector == 0 && SizeBytes == 0 && SizeMulBy16 == 0;
        }
    }
}
