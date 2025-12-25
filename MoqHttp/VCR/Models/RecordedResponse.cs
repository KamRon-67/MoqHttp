using System;
using System.Collections.Generic;

namespace MoqHttp.VCR.Models
{
    /// <summary>
    /// Represents a captured HTTP response for recording in a cassette
    /// </summary>
    public class RecordedResponse
    {
        /// <summary>
        /// HTTP status code
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Response headers
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Response body content as string
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// When this response was recorded
        /// </summary>
        public DateTime RecordedAt { get; set; }

        public RecordedResponse()
        {
            Headers = new Dictionary<string, string>();
            RecordedAt = DateTime.UtcNow;
        }
    }
}
