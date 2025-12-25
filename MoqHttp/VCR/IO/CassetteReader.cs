using System;
using System.IO;
using MoqHttp.VCR.Models;
using Newtonsoft.Json;

namespace MoqHttp.VCR.IO
{
    /// <summary>
    /// Reads VCR cassettes from disk
    /// </summary>
    public class CassetteReader
    {
        /// <summary>
        /// Load a cassette from a JSON file
        /// </summary>
        /// <param name="filePath">Path to the cassette file</param>
        /// <returns>The loaded cassette</returns>
        /// <exception cref="FileNotFoundException">If the cassette file doesn't exist</exception>
        /// <exception cref="InvalidOperationException">If the cassette format is invalid</exception>
        public static Cassette LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Cassette file not found: {filePath}");
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var cassette = JsonConvert.DeserializeObject<Cassette>(json);

                if (cassette == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize cassette from: {filePath}");
                }

                // Validate cassette version (future-proofing)
                if (string.IsNullOrEmpty(cassette.Version))
                {
                    throw new InvalidOperationException($"Cassette is missing version information: {filePath}");
                }

                return cassette;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid cassette format in: {filePath}", ex);
            }
        }

        /// <summary>
        /// Check if a cassette file exists
        /// </summary>
        /// <param name="filePath">Path to check</param>
        /// <returns>True if the file exists</returns>
        public static bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
