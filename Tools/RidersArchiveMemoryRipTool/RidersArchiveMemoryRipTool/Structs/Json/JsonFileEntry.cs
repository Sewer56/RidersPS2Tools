namespace RidersArchiveMemoryRipTool.Structs.Json
{
    public class JsonFileEntry
    {
        /// <summary>
        /// Name of the overall file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Uncompressed size of the file.
        /// </summary>
        public int UncompressedSize { get; set; }

        /// <summary>
        /// Compressed size of tile.
        /// </summary>
        public int CompressedSize { get; set; }

        /// <summary>
        /// Checksum for the file.
        /// </summary>
        public uint Checksum { get; set; }

        public JsonFileEntry(string name, int uncompressedSize, int compressedSize, uint checksum)
        {
            Name = name;
            UncompressedSize = uncompressedSize;
            CompressedSize = compressedSize;
            Checksum = checksum;
        }
    }
}
