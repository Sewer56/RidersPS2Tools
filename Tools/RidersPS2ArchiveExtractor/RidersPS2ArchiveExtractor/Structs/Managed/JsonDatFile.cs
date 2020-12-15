using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RidersPS2ArchiveTool.Structs.Managed
{
    public class JsonDatFile
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions() { WriteIndented = true };

        /// <summary>
        /// List of files inside the .DAT archive.
        /// </summary>
        public List<JsonFileEntry> Files { get; set; } = new List<JsonFileEntry>();

        public JsonDatFile(IEnumerable<JsonFileEntry> entries) => Files = entries.ToList();

        public JsonDatFile() {}

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
        public static JsonDatFile? FromFile(string filePath)
        {
            return JsonSerializer.Deserialize<JsonDatFile>(File.ReadAllText(filePath));
        }
    }
}
