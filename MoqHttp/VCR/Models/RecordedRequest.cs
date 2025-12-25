using System.Collections.Generic;

namespace MoqHttp.VCR.Models
{
    /// <summary>
    /// Represents a captured HTTP request for recording in a cassette
    /// </summary>
    public class RecordedRequest
    {
        /// <summary>
        /// HTTP method (GET, POST, PUT, DELETE, etc.)
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Full request URI
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Request headers
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Request body content as string
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Query string parameters
        /// </summary>
        public Dictionary<string, string> QueryParameters { get; set; }

        public RecordedRequest()
        {
            Headers = new Dictionary<string, string>();
            QueryParameters = new Dictionary<string, string>();
        }
    }
}
