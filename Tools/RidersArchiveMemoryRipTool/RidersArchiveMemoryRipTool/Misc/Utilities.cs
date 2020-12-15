using System;
using System.Collections.Generic;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Structs;

namespace RidersArchiveMemoryRipTool.Misc
{
    public static class Utilities
    {
        /// <summary>
        /// Returns a list of all patterns inside a given block of data.
        /// </summary>
        /// <param name="scanner">The scanner for which to find all patterns in.</param>
        /// <param name="scanPattern">The pattern to be scanned.</param>
        /// <returns>All occurrences of the pattern.</returns>
        public static List<PatternScanResult> FindAllPatterns(Scanner scanner, CompiledScanPattern scanPattern)
        {
            var results = new List<PatternScanResult>();
            var result = new PatternScanResult(-1);

            do
            {
                result = scanner.CompiledFindPattern(scanPattern, result.Offset + 1);
                if (result.Found)
                    results.Add(result);
            }
            while (result.Found);

            return results;
        }

        /// <summary>
        /// Converts a given set of bytes to a scan pattern.
        /// </summary>
        public static string BytesToScanPattern(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace('-', ' ');
        }

        /// <summary>
        /// Rounds a number up to the next multiple unless the number is already a multiple.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="multiple">The multiple.</param>
        public static int RoundUp(int number, int multiple)
        {
            if (multiple == 0)
                return number;

            int remainder = number % multiple;
            if (remainder == 0)
                return number;

            return number + multiple - remainder;
        }
    }
}
