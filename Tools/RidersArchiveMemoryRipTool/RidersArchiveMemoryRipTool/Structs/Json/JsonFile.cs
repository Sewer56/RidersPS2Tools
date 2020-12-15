using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RidersArchiveMemoryRipTool.Misc;

namespace RidersArchiveMemoryRipTool.Structs.Json
{
    public class JsonFile
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions() { WriteIndented = true };

        /// <summary>
        /// List of files inside the .DAT archive.
        /// </summary>
        public List<JsonFileEntry> Files { get; set; } = new List<JsonFileEntry>();

        public JsonFile(IEnumerable<JsonFileEntry> entries) => Files = entries.ToList();

        public JsonFile() { }

        /// <summary>
        /// Serializes the current instance to a file.
        /// </summary>
        public string ToFile()
        {
            return JsonSerializer.Serialize(this, Options);
        }

        /// <summary>
        /// Parses a JSON from a file.
        /// </summary>
        /// <param name="filePath">Full path to a .json file</param>
        public static JsonFile? FromFile(string filePath)
        {
            return JsonSerializer.Deserialize<JsonFile>(File.ReadAllText(filePath));
        }

        /// <summary>
        /// Gets a map of file size to file info.
        /// </summary>
        /// <param name="jsonPath">Path to the json file from which to load the map.</param>
        /// <remarks>
        ///     Game rounds file sizes to next 64 bytes (unless already aligned) and then reads them.
        ///     e.g. `ReadAsyncEnd : Stage.dat , Size = 3433728` | That size is rounded.
        /// </remarks>
        public static Dictionary<int, List<JsonFileEntry>> FromFileAsSizeToFileMap(string jsonPath)
        {
            var sizeToFileMap = new Dictionary<int, List<JsonFileEntry>>();
            foreach (var file in JsonFile.FromFile(jsonPath)?.Files)
            {
                var key = Utilities.RoundUp(file.CompressedSize, 64);
                if (sizeToFileMap.ContainsKey(key))
                    sizeToFileMap[key].Add(file);
                else
                    sizeToFileMap[key] = new List<JsonFileEntry>() { file };
            }

            return sizeToFileMap;
        }

    }
}
