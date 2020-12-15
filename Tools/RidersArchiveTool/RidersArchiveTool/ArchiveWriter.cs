using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Writers;

namespace RidersArchiveTool
{
    public class ArchiveWriter
    {
        /// <summary>
        /// Stores all groups for the archive to be written.
        /// </summary>
        private Dictionary<ushort, List<byte[]>> _groups = new Dictionary<ushort, List<byte[]>>();

        /// <summary>
        /// Adds a file to be written to the archive.
        /// </summary>
        /// <param name="id">The id (file type) of the file.</param>
        /// <param name="data">The data of the file.</param>
        public void AddFile(ushort id, byte[] data)
        {
            if (_groups.TryGetValue(id, out var files))
                files.Add(data);
            else
                _groups[id] = new List<byte[]>() { data };
        }

        /// <summary>
        /// Writes the contents of the archive to be generated to the stream.
        /// </summary>
        public void Write(Stream writeStream)
        {
            using var stream = new ExtendedMemoryStream();

            // Number of items.
            stream.Write<int>(_groups.Keys.Count);

            // Number of items for each id.
            foreach (var group in _groups)
                stream.Write<byte>((byte)group.Value.Count);

            stream.AddPadding(0x00, 4);

            // Write first item index for each group. 
            ushort totalItems = 0;
            foreach (var group in _groups)
            {
                stream.Write<ushort>(totalItems);
                totalItems += (ushort)group.Value.Count;
            }

            // Write ID for each group.
            foreach (var group in _groups)
                stream.Write<ushort>(group.Key);

            // Write offsets for each file and pad.
            int firstWriteOffset = Utilities.Utilities.RoundUp((int)stream.Position + (sizeof(int) * totalItems), 16);
            int fileWriteOffset  = firstWriteOffset;
            foreach (var group in _groups)
            {
                foreach (var file in group.Value)
                {
                    stream.Write<int>(file.Length <= 0 ? 0 : fileWriteOffset);
                    fileWriteOffset += file.Length;
                }
            }

            // Write files.
            stream.Write(new byte[(int)(firstWriteOffset - stream.Position)]); // Alignment
            foreach (var file in _groups.SelectMany(x => x.Value))
                stream.Write(file);

            writeStream.Write(stream.ToArray());
        }

    }
}
