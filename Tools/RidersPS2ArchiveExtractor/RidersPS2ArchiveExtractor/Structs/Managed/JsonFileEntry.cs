using System;
using Reloaded.Memory.Streams;

namespace RidersPS2ArchiveTool.Structs.Managed
{
    public class JsonFileEntry
    {
        /// <summary>
        /// Name of file.
        /// </summary>
        public string Name { get; set; }
 
        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        public int SizeBytes { get; set; }

        /// <summary>
        /// Absolute file offset.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Creates a new Json entry given a DAT file entry.
        /// </summary>
        public JsonFileEntry(DatFileEntry entry, BufferedStreamReader stream, Func<int, int> memoryToRawAddress)
        {
            Name = entry.GetName(stream, memoryToRawAddress);
            SizeBytes = entry.SizeBytes;
            Offset = entry.Sector * DatFileEntry.SECTOR_SIZE_BYTES;
        }

        public JsonFileEntry(string name, int sizeBytes, int offset)
        {
            Name = name;
            SizeBytes = sizeBytes;
            Offset = offset;
        }

        public JsonFileEntry() { }
    }
}
