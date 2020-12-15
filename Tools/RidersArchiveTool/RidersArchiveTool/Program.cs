using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using CommandLine;
using CommandLine.Text;

namespace RidersArchiveTool
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

            var parserResult = parser.ParseArguments<ExtractOptions, PackOptions>(args);
            parserResult.WithParsed<ExtractOptions>(Extract)
                        .WithParsed<PackOptions>(Pack)
                        .WithNotParsed(errs => HandleParseError(parserResult, errs));
        }

        private static void Pack(PackOptions options)
        {
            var directories = Directory.GetDirectories(options.Source);
            var writer      = new ArchiveWriter();

            foreach (var dir in directories)
            {
                var id          = Convert.ToUInt16(Path.GetFileNameWithoutExtension(dir));
                var filesInside = Directory.GetFiles(dir);
                foreach (var file in filesInside)
                    writer.AddFile(id, File.ReadAllBytes(file));
            }

            // Write file to new location.
            Directory.CreateDirectory(Path.GetDirectoryName(options.SavePath));
            using var fileStream = new FileStream(options.SavePath, FileMode.Create, FileAccess.Write, FileShare.None);
            writer.Write(fileStream);
        }

        private static void Extract(ExtractOptions options)
        {
            Directory.CreateDirectory(options.SavePath);

            using var fileStream   = new FileStream(options.Source, FileMode.Open, FileAccess.Read);
            using var archiveReader = new ArchiveReader(fileStream, (int) fileStream.Length);
            var fileIdToData = archiveReader.GetAllFiles();

            foreach (var data in fileIdToData)
            {
                var folder = Path.Combine(options.SavePath, data.Key.ToString("00000"));
                Directory.CreateDirectory(folder);

                for (var x = 0; x < data.Value.Length; x++)
                {
                    var filePath = Path.Combine(folder, x.ToString("00000"));
                    File.WriteAllBytes(filePath, data.Value[x]);
                    Console.WriteLine($"Writing {filePath}");
                }
            }
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
