using System;
using System.Linq;
using System.Text.RegularExpressions;
using RidersArchiveMemoryRipTool.Structs;

namespace RidersArchiveMemoryRipTool
{
    /// <summary>
    /// Parses the crash log on failure to load a PackMan Archive.
    /// </summary>
    public class ParsedCrashLog
    {
        /// <summary>
        /// Name of the archive loaded.
        /// </summary>
        public string FileName;

        /// <summary>
        /// Size of the last loaded archive file before crashing.
        /// </summary>
        public int FileSize;

        /// <summary>
        /// Metadata for PackMan groups.
        /// </summary>
        public PackManGroupMetadata[] Metadata;

        public ParsedCrashLog(string crashLog)
        {
            // Trim the log to the last archive read and get filename.
            var discReadRegex = new Regex(@"^ReadAsyncEStart : (.*) $", RegexOptions.Multiline);
            var lastMatch     = discReadRegex.Matches(crashLog).Last();
            FileName          = lastMatch.Groups[1].Value;
            crashLog          = crashLog.Substring(lastMatch.Index);

            // Export the file size.
            var fileSizeRegex = new Regex(@"^ReadAsyncEnd : .* Size = ([0-9]+).*$", RegexOptions.Multiline);
            var match         = fileSizeRegex.Match(crashLog);
            FileSize          = Convert.ToInt32(match.Groups[1].Value);

            // Get all of the package properties.
            var groupRegex = new Regex(@"^\+\+\+\+Group\[[0-9]{1,4}\], BinNum\[([0-9]{1,2})\] : ID=([0-9]*)$", RegexOptions.Multiline);
            var matches    = groupRegex.Matches(crashLog);
            Metadata       = matches.Select(x => new PackManGroupMetadata(Convert.ToByte(x.Groups[1].Value), Convert.ToUInt16(x.Groups[2].Value))).ToArray();
        }
    }
}
