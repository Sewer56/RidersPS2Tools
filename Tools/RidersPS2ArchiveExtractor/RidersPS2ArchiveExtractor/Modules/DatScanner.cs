using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using Reloaded.Memory;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Structs;
using Reloaded.Memory.Streams;
using RidersPS2ArchiveTool.Structs;
using RidersPS2ArchiveTool.Structs.Managed;

namespace RidersPS2ArchiveTool.Modules
{
    /// <summary>
    /// Scans a PS2 binary file to attempt to find all file lists for a given DAT file name.
    /// </summary>
    public class DatScanner
    {
        private byte[]  _data;
        private Scanner _scanner;
        private Section<uint> _mainSection;
        
        /// <param name="data">The PS2 executable to scan.</param>
        public DatScanner(byte[] data)
        {
            _scanner = new Scanner(data);
            _data    = data;

            var elf      = ELFReader.Load(new MemoryStream(data), true);
            _mainSection = (Section<uint>) elf.Sections.First(x => x.Type == SectionType.ProgBits);
        }

        /// <summary>
        /// Searches the executable for a files belonging to a specified DAT file.
        /// </summary>
        /// <param name="datFileName">
        ///     Name of the dat file, e.g. "Snd.dat".
        ///     Case sensitive. Names start with capital and are lowercase.
        ///     In some versions they can start with a backslash, e.g. "\Snd.dat".
        /// </param>
        /// <returns>A dictionary of offset to all file entries.</returns>
        public Dictionary<int, List<DatFileEntry>> FindFiles(string datFileName)
        {
            var offsets = new Dictionary<int, List<DatFileEntry>>();

            using var memoryStream = new MemoryStream(_data);
            using var streamReader = new BufferedStreamReader(memoryStream, 2048);
            var fileNamePatterns   = FindFileNamePatterns(datFileName);
            
            foreach (var namePattern in fileNamePatterns)
            {
                /*
                 * We have to translate the raw address to memory address here because
                 * our offset doesn't account for the base address of the Main segment.
                 */
                var pointerPatterns = GetPointerPatterns(RawToMemoryAddress(namePattern.Offset));

                foreach (var pointerPattern in pointerPatterns)
                {
                    var entries = GetEntriesForPointerPattern(datFileName, streamReader, pointerPattern.Offset);
                    if (entries == null)
                        continue;

                    offsets[pointerPattern.Offset] = entries;
                }
            }

            return offsets;
        }

        /// <summary>
        /// Use with output of <see cref="FindFiles"/>
        /// Converts list of files from executable to a portable JSON format.
        /// </summary>
        /// <param name="offsetEntries">Dictionary mapping file table offsets to entries.</param>
        public List<JsonDatFile> GetFiles(Dictionary<int, List<DatFileEntry>> offsetEntries)
        {
            using var memoryStream = new MemoryStream(_data);
            using var streamReader = new BufferedStreamReader(memoryStream, 2048);

            return offsetEntries.Values.Select(x =>
            {
                return new JsonDatFile(x.Select(y => new JsonFileEntry(y, streamReader, MemoryToRawAddress)));
            }).ToList();
        }

        private List<DatFileEntry>? GetEntriesForPointerPattern(string datFileName, BufferedStreamReader streamReader, int offset)
        {
            // Validate if first entry
            streamReader.Seek(offset, SeekOrigin.Begin);
            streamReader.Read(out DatFileEntry headerEntry);
            streamReader.Read(out DatFileEntry firstEntry);
            var savedPos = streamReader.Position();

            if (!headerEntry.IsFirstEntry(streamReader, MemoryToRawAddress, firstEntry, datFileName))
                return null;

            streamReader.Seek(savedPos, SeekOrigin.Begin);

            // Populate other entries.
            var entries = new List<DatFileEntry>(new[] {firstEntry});
            DatFileEntry entry;
            while (!(entry = streamReader.Read<DatFileEntry>()).IsLastEntry())
            {
                entries.Add(entry);
            }

            return entries;
        }

        private List<PatternScanResult> GetPointerPatterns(int targetOffset)
        {
            // Now find all pointers to file name pattern.
            byte[] offsetBytes  = Struct.GetBytes(targetOffset);
            var scanPattern     = new CompiledScanPattern(Utilities.Utilities.BytesToScanPattern(offsetBytes));
            return Utilities.Utilities.FindAllPatterns(_scanner, scanPattern);
        }

        private List<PatternScanResult> FindFileNamePatterns(string datFileName)
        {
            // We're using ASCII so byte per character.
            byte[] fileNameBytes = new byte[datFileName.Length + 1];
            Encoding.ASCII.GetBytes(datFileName, fileNameBytes);

            // Pattern to find.
            var scanPattern = new CompiledScanPattern(Utilities.Utilities.BytesToScanPattern(fileNameBytes));
            return Utilities.Utilities.FindAllPatterns(_scanner, scanPattern);
        }

        /// <summary>
        /// Converts a mapped memory (RAM) address to a raw hex address.
        /// </summary>
        public int MemoryToRawAddress(int memoryAddress) => (int) (memoryAddress - _mainSection.LoadAddress + _mainSection.Offset);

        /// <summary>
        /// Converts a raw hex address to a mapped memory (RAM) address.
        /// </summary>
        public int RawToMemoryAddress(int memoryAddress) => (int) (memoryAddress - _mainSection.Offset + _mainSection.LoadAddress);
    }
}
