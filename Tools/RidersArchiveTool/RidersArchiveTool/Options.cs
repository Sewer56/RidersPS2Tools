using CommandLine;

namespace RidersArchiveTool
{
    [Verb("extract", HelpText = "Extracts a Riders Archive file.")]
    internal class ExtractOptions
    {
        [Option(Required = true, HelpText = "The archive file to extract.")]
        public string Source { get; private set; }

        [Option(Required = true, HelpText = "The folder to extract files to.")]
        public string SavePath { get; private set; }
    }


    [Verb("pack", HelpText = "Packs a Riders Archive file.")]
    internal class PackOptions
    {
        [Option(Required = true, HelpText = "The folder containing the files to pack in the same format as extracted. i.e. In this folder should be subfolders, each of which is an unique ID.")]
        public string Source { get; private set; }

        [Option(Required = true, HelpText = "The path to which to save the new archive.")]
        public string SavePath { get; private set; }
    }
}
