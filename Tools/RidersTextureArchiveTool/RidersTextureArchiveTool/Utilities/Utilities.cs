using System;
using System.Runtime.CompilerServices;
using System.Text;
using Reloaded.Memory.Streams;

namespace RidersTextureArchiveTool.Utilities
{
    public static class Utilities
    {
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

        /// <summary>
        /// Reads a null terminated ASCII string, up to 1024 characters.
        /// </summary>
        public static string ReadString(this BufferedStreamReader reader)
        {
            Span<byte> data = stackalloc byte[1024];
            int numCharactersRead = 0;

            byte currentByte = 0;
            while ((currentByte = reader.Read<byte>()) != 0)
                data[numCharactersRead++] = currentByte;

            return Encoding.ASCII.GetString(data.Slice(0, numCharactersRead));
        }

        /// <summary>
        /// Replaces slashes with Unicode Lookalikes.
        /// </summary>
        public static string UnicodeReplaceSlash(this string text) => text.Replace('/', '∕').Replace('\\', '⧵');

        /// <summary>
        /// Unreplaces slashes with Unicode Lookalikes.
        /// </summary>
        public static string UnicodeUnReplaceSlash(this string text) => text.Replace('∕', '/').Replace('⧵', '\\');
    }
}
