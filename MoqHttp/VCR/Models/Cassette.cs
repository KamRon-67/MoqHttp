using System;
using System.Collections.Generic;

namespace MoqHttp.VCR.Models
{
    /// <summary>
    /// Represents a VCR cassette containing recorded HTTP interactions
    /// </summary>
    public class Cassette
    {
        /// <summary>
        /// Cassette format version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// List of recorded interactions
        /// </summary>
        public List<Interaction> Interactions { get; set; }

        /// <summary>
        /// When this cassette was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }

        public Cassette()
        {
            Version = "1.0";
            Interactions = new List<Interaction>();
            CreatedAt = DateTime.UtcNow;
            Metadata = new Dictionary<string, string>();
        }
    }
}
