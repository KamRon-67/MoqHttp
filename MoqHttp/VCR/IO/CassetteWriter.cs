using System;
using System.IO;
using System.Text.Json;
using MoqHttp.VCR.Models;

namespace MoqHttp.VCR.IO
{
    /// <summary>
    /// Writes VCR cassettes to disk
    /// </summary>
    public class CassetteWriter
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true
        };

        /// <summary>
        /// Save a cassette to a JSON file
        /// </summary>
        /// <param name="cassette">The cassette to save</param>
        /// <param name="filePath">Path where to save the cassette</param>
        public static void SaveToFile(Cassette cassette, string filePath)
        {
            if (cassette == null)
            {
                throw new ArgumentNullException(nameof(cassette));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be empty", nameof(filePath));
            }

            try
            {
                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Serialize to JSON with pretty printing
                var json = JsonSerializer.Serialize(cassette, _options);

                // Write atomically using temp file
                var tempFile = filePath + ".tmp";
                File.WriteAllText(tempFile, json);
                
                // Replace existing file
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempFile, filePath);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new InvalidOperationException($"Failed to save cassette to: {filePath}", ex);
            }
        }
    }
}

