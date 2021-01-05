using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Writers;
using RidersArchiveTool.Structs.Parser;

namespace RidersArchiveTool
{
    public class ArchiveWriter
    {
        /// <summary>
        /// A map of all group IDs to their group data.
        /// Stores all groups for the archive to be written.
        /// </summary>
        public Dictionary<byte, ManagedGroup> Groups { get; } = new Dictionary<byte, ManagedGroup>();

        /// <summary>
        /// Adds a group to be written to the archive.
        /// </summary>
        /// <param name="groupNo">Id of the group to add.</param>
        /// <param name="group">The group to add to the list of groups.</param>
        public void AddGroup(byte groupNo, ManagedGroup group) => Groups[groupNo] = (group);

        /// <summary>
        /// Writes the contents of the archive to be generated to the stream.
        /// </summary>
        public void Write(Stream writeStream)
        {
            using var stream = new ExtendedMemoryStream();

            // Number of items.
            stream.Write<int>(Groups.Keys.Count);

            // Number of items for each id.
            foreach (var group in Groups)
                stream.Write<byte>((byte)group.Value.Files.Count);

            stream.AddPadding(0x00, 4);

            // Write first item index for each group. 
            ushort totalItems = 0;
            foreach (var group in Groups)
            {
                stream.Write<ushort>(totalItems);
                totalItems += (ushort)group.Value.Files.Count;
            }

            // Write ID for each group.
            foreach (var group in Groups)
                stream.Write<ushort>(group.Value.Id);

            // Write offsets for each file and pad.
            int firstWriteOffset = Utilities.Utilities.RoundUp((int)stream.Position + (sizeof(int) * totalItems), 16);
            int fileWriteOffset  = firstWriteOffset;
            foreach (var group in Groups)
            {
                foreach (var file in group.Value.Files)
                {
                    stream.Write<int>(file.Data.Length <= 0 ? 0 : fileWriteOffset);
                    fileWriteOffset += file.Data.Length;
                }
            }

            // Write files.
            stream.Write(new byte[(int)(firstWriteOffset - stream.Position)]); // Alignment
            foreach (var file in Groups.SelectMany(x => x.Value.Files))
                stream.Write(file.Data);

            writeStream.Write(stream.ToArray());
        }

    }
}
