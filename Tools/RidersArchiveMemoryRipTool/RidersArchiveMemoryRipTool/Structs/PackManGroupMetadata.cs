namespace RidersArchiveMemoryRipTool.Structs
{
    /// <summary>
    /// Stores metadata for PackMan (Sonic Riders Package/Archive Manager) groups.
    /// </summary>
    public struct PackManGroupMetadata
    {
        /// <summary>
        /// Number of items in group.
        /// </summary>
        public byte NoOfItems;

        /// <summary>
        /// Unique file type/identifier for members of the group.
        /// </summary>
        public ushort Id;

        public PackManGroupMetadata(byte noOfItems, ushort id)
        {
            NoOfItems = noOfItems;
            Id = id;
        }
    }
}