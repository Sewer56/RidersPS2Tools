using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Structs;
using Reloaded.Memory.Streams;

namespace RidersPS2ArchiveTool.Utilities
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
        /// Gets the length of a null terminated string pointer.
        /// </summary>
        public static unsafe string GetString(BufferedStreamReader reader, long offset, Encoding? encoding = null)
        {
            encoding ??= Encoding.ASCII;

            var bytes = new List<byte>(64);
            reader.Seek(offset, SeekOrigin.Begin);

            byte currentByte;
            while ((currentByte = reader.Read<byte>()) != 0)
                bytes.Add(currentByte);

            return encoding.GetString(CollectionsMarshal.AsSpan(bytes));
        }

        /// <summary>
        /// Gets an ASCII string from a given address.
        /// </summary>
        public static unsafe string GetString(byte* stringPtr)
        {
            return Encoding.ASCII.GetString(stringPtr, Strlen(stringPtr));
        }

        /// <summary>
        /// Gets the length of a null terminated string pointer.
        /// </summary>
        public static unsafe int Strlen(byte* stringPtr)
        {
            int length = 0;
            while (stringPtr[length] != 0x00) 
                length++;

            return length;
        }

        /// <summary>
        /// Pads a given list of bytes such that the byte count is a multiple of a given alignment.
        /// </summary>
        public static void AddPadding(this List<byte> list, int alignment = 4096)
        {
            var padding = RoundUp(list.Count, alignment) - list.Count;
            if (padding <= 0)
                return;

            list.AddRange(new byte[padding]);
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
