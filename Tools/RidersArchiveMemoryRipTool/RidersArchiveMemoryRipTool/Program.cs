using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using HashDepot;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Structs;
using Reloaded.Memory.Sources;
using Reloaded.Memory.Streams;
using RidersArchiveMemoryRipTool.Misc;
using RidersArchiveMemoryRipTool.Structs.Json;

namespace RidersArchiveMemoryRipTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(with =>
            {
                with.AutoHelp = true;
                with.CaseSensitive = false;
                with.CaseInsensitiveEnumValues = true;
                with.EnableDashDash = true;
                with.HelpWriter = null;
            });

            var parserResult = parser.ParseArguments<ScanOptions, RipOptions>(args);
            parserResult.WithParsed<ScanOptions>(Scan)
                        .WithParsed<RipOptions>(Rip)
                        .WithParsed<LegacyRipOptions>(LegacyRip)
                        .WithNotParsed(errs => HandleParseError(parserResult, errs));
        }

        private static void Rip(RipOptions options)
        {
            using var fileStream = new FileStream(options.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fileStream.Seek(0, SeekOrigin.End);
            var buffer = new byte[10000];

            Console.WriteLine("Beginning AutoRip, CTRL+C to Exit.");
            while (true)
            {
                var streamPos            = fileStream.Position;
                var bytesSinceLastUpdate = fileStream.Length - streamPos;

                // Resize buffer if necessary.
                if (bytesSinceLastUpdate > buffer.Length)
                    buffer = new byte[bytesSinceLastUpdate];

                // Read new log text.
                fileStream.Read(buffer);
                var text = Encoding.UTF8.GetString(buffer);

                // RIP if there is new error text.
                if (text.Contains("Addr:"))
                {
                    NewRip(options);
                    Array.Fill<byte>(buffer, 0x00);
                }
                else
                    fileStream.Position = streamPos;

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// The new implementation of Rip.
        /// </summary>
        private static void NewRip(RipOptions options)
        {
            // Read emulated memory.
            var process = new ExternalMemory(Process.GetProcessesByName(options.ProcessName)[0]);

            // Parse known file list.
            var sizeToFileMap = JsonFile.FromFileAsSizeToFileMap(options.JsonPath);

            // Read PCSX2 Logs
            using var fileStream = new FileStream(options.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var logData = new byte[fileStream.Length];
            fileStream.Read(logData);

            // Get size, address from log.
            var pcsx2Log = Encoding.UTF8.GetString(logData);
            var memoryDumpRegex = new Regex(@"^Addr: ([\d]*) , Size: ([\d]*), DumpFlagAddr: ([\d]*) [\n\r]+", RegexOptions.Multiline);
            var lastMatch       = memoryDumpRegex.Matches(pcsx2Log).Last();
            var archiveOffset   = Convert.ToInt32(lastMatch.Groups[1].Value);
            var fileSize        = Convert.ToInt32(lastMatch.Groups[2].Value);
            var dumpFlagAddr    = Convert.ToInt32(lastMatch.Groups[3].Value);

            // Extract match from memory.
            if (sizeToFileMap.TryGetValue(fileSize, out var fileInfoList))
            {
                var entry    = RipUserSelectEntryFromList(fileInfoList);
                process.ReadRaw((IntPtr) (options.MinRamAddress + archiveOffset), out var file, entry.UncompressedSize);
                RipWriteFileToFolder(options.OutputPath, entry, file);
            }
            else
            {
                Console.WriteLine($"No known file in JSON found. Allowing game to advance.");
            }

            process.SafeWrite<int>((IntPtr)(options.MinRamAddress + dumpFlagAddr), 1);
        }

        /// <summary>
        /// The legacy implementation of Rip.
        /// </summary>
        private static void LegacyRip(LegacyRipOptions options)
        {
            // Read emulated memory.
            var process = new ExternalMemory(Process.GetProcessesByName(options.ProcessName)[0]);
            process.ReadRaw((IntPtr)options.MinRamAddress, out var ps2Memory, 0x2000000);

            // Parse known file list.
            var sizeToFileMap = JsonFile.FromFileAsSizeToFileMap(options.JsonPath);

            // Read PCSX2 Logs
            using var fileStream = new FileStream(options.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var logData = new byte[fileStream.Length];
            fileStream.Read(logData);

            var pcsx2CrashLog     = Encoding.UTF8.GetString(logData);
            var pcsx2CrashLogInfo = new ParsedCrashLog(pcsx2CrashLog.Replace("\r\n", "\n"));

            // Pattern scan for archive:
            // Build search string.
            var searchPatternBuilder = new StringBuilder($"{(options.GroupNum != -1 ? options.GroupNum.ToString("X8") : "??")} 00 00 00 "); // Note: Assuming less than 255 items!

            var groupItemCountBytes = pcsx2CrashLogInfo.Metadata.Select(x => x.NoOfItems).ToArray();
            var itemCountPattern = Utilities.BytesToScanPattern(groupItemCountBytes);
            searchPatternBuilder.Append(itemCountPattern);

            // Scan for potential matches.
            var scanner = new Scanner(ps2Memory);
            var patterns = Utilities.FindAllPatterns(scanner, new CompiledScanPattern(searchPatternBuilder.ToString()));
            int archiveOffset = patterns[0].Offset;

            // Handle multiple matches.
            if (patterns.Count > 1)
            {
                // Note: Not tested.
                Console.WriteLine("More than 1 match for possible archive data. Trying to reconstruct header and searching for ID. (Note: This code is not yet implemented, exiting!)");
                return;
            }

            // Extract match from memory.
            if (sizeToFileMap.TryGetValue(pcsx2CrashLogInfo.FileSize, out var fileInfoList))
            {
                // Get user picked entry from list.
                var entry = RipUserSelectEntryFromList(fileInfoList);

                // Save file.
                var slice = ps2Memory.AsSpan().Slice(archiveOffset, entry.UncompressedSize);
                RipWriteFileToFolder(options.OutputPath, entry, slice);
            }
            else
            {
                Console.WriteLine($"No known file in JSON found. File Size from Crashlog: 0x{pcsx2CrashLogInfo.FileSize:X}");
            }
        }

        static void Scan(ScanOptions options)
        {
            var files    = Directory.GetFiles(options.Source, "*.*", SearchOption.AllDirectories);
            var entries  = new List<JsonFileEntry>();

            foreach (var file in files)
            {
                using var fileStream   = new FileStream(file, FileMode.Open, FileAccess.Read);
                using var streamReader = new BufferedStreamReader(fileStream, 8);

                // Check if compressed archive (magic header)
                var header = streamReader.Read<uint>();
                if (header == 0x80000001)
                {
                    Console.WriteLine($"Found: {Path.GetFileName(file)}, hashing and adding.");
                    var wholeFile = File.ReadAllBytes(file);
                    var checksum  = XXHash.Hash32(wholeFile);
                    entries.Add(new JsonFileEntry(Path.GetFileName(file), streamReader.Read<int>(), (int) fileStream.Length, checksum));
                }
            }

            var jsonFile = new JsonFile(entries);
            File.WriteAllText(options.SavePath, jsonFile.ToFile());
        }

        /// <summary>
        /// Errors or --help or --version.
        /// </summary>
        static void HandleParseError(ParserResult<object> options, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(options, help =>
            {
                help.Copyright = "Created by Sewer56, licensed under GNU LGPL V3";
                help.AutoHelp = false;
                help.AutoVersion = false;
                help.AddDashesToOption = true;
                help.AddEnumValuesToHelpText = true;
                help.AdditionalNewLineAfterOption = true;
                return HelpText.DefaultParsingErrorsHandler(options, help);
            }, example => example, true);

            Console.WriteLine(helpText);
        }

        private static void RipWriteFileToFolder(string outputPath, JsonFileEntry entry, Span<byte> slice)
        {
            Directory.CreateDirectory(outputPath);
            var outputFilePath = Path.Combine(outputPath, entry.Name);

            using var file = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
            file.Write(slice);
            Console.WriteLine($"Written file to {outputFilePath}");
        }

        private static JsonFileEntry RipUserSelectEntryFromList(List<JsonFileEntry> fileInfoList)
        {
            JsonFileEntry entry = fileInfoList[0];

            // Handle duplicate entries.
            if (fileInfoList.Count > 1)
            {
                Console.WriteLine("Multiple files detected with matching compressed sizes.");
                Console.WriteLine("Select the file you think you're ripping: ");
                for (var x = 0; x < fileInfoList.Count; x++)
                {
                    var fileInfo = fileInfoList[x];
                    Console.WriteLine($"[{x:00}] {fileInfo.Name,20} | Checksum {fileInfo.Checksum:X}");
                }

                entry = fileInfoList[Convert.ToInt32(Console.ReadLine())];
            }

            return entry;
        }
    }
}
