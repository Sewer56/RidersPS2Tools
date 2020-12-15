namespace RidersArchiveTool.Structs.Parser
{
    /// <summary>
    /// Defines members of one group.
    /// </summary>
    public struct Group
    {
        /// <summary>
        /// Unique file type/identifier for members of the group.
        /// </summary>
        public ushort Id;

        /// <summary>
        /// Offsets and sizes of all members of the group.
        /// </summary>
        public File[] Files;
    }

    /// <summary>
    /// Represents an individual file inside a group.
    /// </summary>
    public struct File
    {
        /// <summary>
        /// The offset of the file.
        /// </summary>
        public int Offset; 

        /// <summary>
        /// The size of the file.
        /// </summary>
        public int Size;
    }
}
