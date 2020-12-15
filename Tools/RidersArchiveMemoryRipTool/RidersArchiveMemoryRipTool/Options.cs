using CommandLine;
#pragma warning disable 8618

namespace RidersArchiveMemoryRipTool
{
    [Verb("scan", HelpText = "Searches for compressed archives inside a given folder (+ subdirectories) and creates a JSON file with list of all archive sizes.")]
    internal class ScanOptions
    {
        [Option(Required = true, HelpText = "Full path to the folder containing compressed archive files.")]
        public string Source { get; private set; }

        [Option(Required = true, HelpText = "Full path to the file to be saved.")]
        public string SavePath { get; private set; }
    }


    [Verb("rip", HelpText = "Rips data from the PS2 prototype using a debug verison (SLUS_213.31_MemRip) and custom printout in PCSX2 logs.")]
    internal class RipOptions
    {
        [Option(Required = true, HelpText = "Full path to the json file knowing all file sizes.")]
        public string JsonPath { get; private set; }

        [Option(Required = true, HelpText = "Path to the log file from the PS2 Prototype before crash.")]
        public string LogPath { get; private set; }

        [Option(Required = true, HelpText = "Path to the folder where the file should be output.")]
        public string OutputPath { get; private set; }

        [Option(Required = false, Default = "pcsx2", HelpText = "Name of the PCSX2 executable/process (Default is \"pcsx2\").")]
        public string ProcessName { get; private set; }

        [Option(Required = false, Default = 0x30000000, HelpText = "Address to the start of emulated memory for PCSX2. Older versions use (0x30000000). Would suggest launching PCSX2 with Console and Dev/Verbose source enabled and get `EE Main Memory` address range from there. Convert from hex to decimal and input from there.")]

        public int MinRamAddress { get; private set; }
    }

    [Verb("legacyrip", HelpText = "Using a crash log from the PS2 prototype, rips uncompressed archive data inside game memory.")]
    internal class LegacyRipOptions
    {
        [Option(Required = true, HelpText = "Full path to the json file knowing all file sizes.")]
        public string JsonPath { get; private set; }

        [Option(Required = true, HelpText = "Path to the log file from the PS2 Prototype before crash.")]
        public string LogPath { get; private set; }

        [Option(Required = true, HelpText = "Path to the folder where the file should be output.")]
        public string OutputPath { get; private set; }

        [Option(Required = false, Default = "pcsx2", HelpText = "Name of the PCSX2 executable/process (Default is \"pcsx2\").")]
        public string ProcessName { get; private set; }

        [Option(Required = false, Default = 0x30000000, HelpText = "Address to the start of emulated memory for PCSX2. Older versions use (0x30000000). Would suggest launching PCSX2 with Console and Dev/Verbose source enabled and get `EE Main Memory` address range from there.")]

        public int MinRamAddress { get; private set; }

        [Option(Required = false, Default = -1, HelpText = "Set this if you know the number of groups in the archive you are searching, else leave blank. (Range: 0-255)")]
        public int GroupNum { get; private set; }
    }
}
