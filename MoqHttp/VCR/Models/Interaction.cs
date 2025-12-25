using System;

namespace MoqHttp.VCR.Models
{
    /// <summary>
    /// Represents a single HTTP request/response interaction
    /// </summary>
    public class Interaction
    {
        /// <summary>
        /// The recorded request
        /// </summary>
        public RecordedRequest Request { get; set; }

        /// <summary>
        /// The recorded response
        /// </summary>
        public RecordedResponse Response { get; set; }

        /// <summary>
        /// When this interaction was recorded
        /// </summary>
        public DateTime RecordedAt { get; set; }

        public Interaction()
        {
            RecordedAt = DateTime.UtcNow;
        }
    }
}
