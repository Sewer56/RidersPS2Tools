using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CommandLine;
using CommandLine.Text;
using Reloaded.Memory.Streams;
using RidersPS2ArchiveTool.Modules;
using RidersPS2ArchiveTool.Structs;
using RidersPS2ArchiveTool.Structs.Managed;
using RidersPS2ArchiveTool.Utilities;

namespace RidersPS2ArchiveTool
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

            var parserResult = parser.ParseArguments<ScanOptions, ExtractOptions, PackOptions, InjectOptions, VerifyOptions>(args);
            parserResult.WithParsed<ScanOptions>(Scan)
                        .WithParsed<ExtractOptions>(Extract)
                        .WithParsed<PackOptions>(Pack)
                        .WithParsed<InjectOptions>(Inject)
                        .WithParsed<VerifyOptions>(Verify)
                        .WithNotParsed(errs => HandleParseError(parserResult, errs));
        }

        private static void Verify(VerifyOptions options)
        {
            var jsonFile = JsonDatFile.FromFile(options.JsonPath);
            var sourceFiles = Directory.GetFiles(options.SourceFolder).ToDictionary(Path.GetFileName, StringComparer.OrdinalIgnoreCase);

            foreach (var file in jsonFile.Files)
            {
                if (sourceFiles.TryGetValue(file.Name, out _))
                    continue;

                Console.WriteLine($"Missing File: {file.Name}");
            }

            Console.WriteLine($"Verify Complete");
        }

        static unsafe void Inject(InjectOptions options)
        {
            var elf      = File.ReadAllBytes(options.File);
            var scanner  = new DatScanner(elf);
            var files    = scanner.FindFiles(options.Dat);
            var jsonFile = JsonDatFile.FromFile(options.JsonPath);

            using var memoryStream = new MemoryStream(elf, true);
            using var streamReader = new BufferedStreamReader(memoryStream, 2048);
            using var extendedMemoryStream = new ExtendedMemoryStream(elf, true);

            // Patch all table entries.
            foreach (var offset in files.Keys)
            {
                Console.WriteLine($"Patching table at: {offset:X}, RAM: {scanner.RawToMemoryAddress(offset):X}");

                // Go to 2nd entry and get initial file name write pointer.
                streamReader.Seek(offset, SeekOrigin.Begin);
                streamReader.Read(out DatFileEntry firstEntry);

                // Get file name write pointer.
                streamReader.Peek(out DatFileEntry secondEntry);
                int fileNameWritePointer = scanner.MemoryToRawAddress(secondEntry.NamePtr);

                // Write archive entry
                var newFirstEntry = new DatFileEntry(firstEntry.NamePtr, jsonFile.Files[0].Offset / DatFileEntry.SECTOR_SIZE_BYTES, jsonFile.Files[0].SizeBytes);
                extendedMemoryStream.Seek(offset, SeekOrigin.Begin);
                extendedMemoryStream.Write(newFirstEntry);

                // Now write each file in order, while keeping track of the pointer.
                foreach (var entry in jsonFile.Files)
                {
                    // Make entry for the file.
                    var datEntry = new DatFileEntry(scanner.RawToMemoryAddress(fileNameWritePointer), entry.Offset / DatFileEntry.SECTOR_SIZE_BYTES, entry.SizeBytes);
                    extendedMemoryStream.Write(datEntry);

                    // Get bytes attached to the name (w/ Null Terminator).
                    var alignedTextLength = Utilities.Utilities.RoundUp(entry.Name.Length + 1, 8); // Alignment of 8
                    var nameBytes = new byte[alignedTextLength];
                    Encoding.ASCII.GetBytes(entry.Name, nameBytes);
                    
                    // Write bytes to pointer.
                    Array.Copy(nameBytes, 0, elf, fileNameWritePointer, nameBytes.Length);

                    // Align text to next predetermined value like in original ELF.
                    fileNameWritePointer += alignedTextLength;
                }
            }

            // Write new executable to file.
            memoryStream.Dispose();
            streamReader.Dispose();
            extendedMemoryStream.Dispose();

            Console.WriteLine($"Writing patched file to: {options.File}");
            File.WriteAllBytes(options.File, elf);
        }

        static void Pack(PackOptions options)
        {
            var data             = new List<byte>(150 * 1000 * 1000);
            var files            = Directory.GetFiles(options.SourceFolder).ToDictionary(Path.GetFileName, StringComparer.OrdinalIgnoreCase);
            var originalJsonFile = JsonDatFile.FromFile(options.SourceJson);
            var jsonFile         = new JsonDatFile();
            
            Directory.CreateDirectory(Path.GetDirectoryName(options.JsonPath));
            Directory.CreateDirectory(Path.GetDirectoryName(options.DatPath));

            foreach (var origFile in originalJsonFile.Files)
            {
                if (files.TryGetValue(origFile.Name, out var file))
                {
                    var fileBytes = File.ReadAllBytes(file);
                    int currentOffset = data.Count;
                    var fileEntry = new JsonFileEntry(Path.GetFileName(file), fileBytes.Length, currentOffset);

                    data.AddRange(fileBytes);
                    jsonFile.Files.Add(fileEntry);
                    data.AddPadding(2048);
                }
            }

            // Save (Note: Using FileStream to avoid copying large array)
            File.WriteAllText(options.JsonPath, jsonFile.ToFile());
            using var fileStream = new FileStream(options.DatPath, FileMode.Create);
            fileStream.Write(CollectionsMarshal.AsSpan(data));

            Console.WriteLine($"JSON file for injection written to: {options.JsonPath}");
            Console.WriteLine($"New DAT file written to: {options.DatPath}");
        }

        static void Extract(ExtractOptions options)
        {
            var datFile  = File.ReadAllBytes(options.File);
            var jsonFile = JsonDatFile.FromFile(options.JsonPath);
            Directory.CreateDirectory(options.OutputFolder);

            var datSpan  = datFile.AsSpan();
            foreach (var entry in jsonFile.Files)
            {
                var filePath = Path.Combine(options.OutputFolder, entry.Name);
                
                // Check if first and last byte are within file size.
                if (entry.Offset > datFile.Length || entry.Offset + entry.SizeBytes > datFile.Length)
                {
                    Console.WriteLine($"Warning: Skipping file {filePath} because the start or end of file is outside of the archive.");
                    continue;
                }

                // Write file
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                fileStream.Write(datSpan.Slice(entry.Offset, entry.SizeBytes));
                Console.WriteLine($"Written File {filePath}.");
            }
        }

        static void Scan(ScanOptions options)
        {
            var data    = File.ReadAllBytes(options.File);
            var scanner = new DatScanner(data);
            var files   = scanner.FindFiles(options.Dat);
            var json    = scanner.GetFiles(files);

            if (json.Count > 0)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(options.OutputPath));
                if (options.AllowDuplicates)
                {
                    for (int x = 0; x < json.Count; x++)
                    {
                        var filePath = Path.GetFileNameWithoutExtension(options.OutputPath) + "_" + ".json";
                        var jsonText = json[x].ToFile();
                        File.WriteAllText(filePath, jsonText);
                        PrintFileOutput(filePath);
                    }
                }
                else
                {
                    var jsonFile = json[0];
                    var jsonText = jsonFile.ToFile();
                    File.WriteAllText(options.OutputPath, jsonText);
                    PrintFileOutput(options.OutputPath);
                }
            }

            void PrintFileOutput(string path) { Console.WriteLine($"Saved file to {path}"); }
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
    }
}
