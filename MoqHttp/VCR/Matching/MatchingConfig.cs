using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using MoqHttp.VCR.Models;

namespace MoqHttp.VCR.Matching
{
    /// <summary>
    /// Configuration for request matching behavior
    /// </summary>
    public class MatchingConfig
    {
        /// <summary>
        /// Custom matcher functions
        /// </summary>
        public List<Func<HttpRequest, RecordedRequest, bool>> Matchers { get; }

        /// <summary>
        /// Whether to match on request body
        /// </summary>
        public bool MatchBody { get; set; }

        /// <summary>
        /// Whether to match on query string
        /// </summary>
        public bool MatchQueryString { get; set; }

        /// <summary>
        /// Specific headers to match on
        /// </summary>
        public List<string> HeadersToMatch { get; set; }

        public MatchingConfig()
        {
            Matchers = new List<Func<HttpRequest, RecordedRequest, bool>>();
            HeadersToMatch = new List<string>();
            MatchBody = false;
            MatchQueryString = false;
        }

        /// <summary>
        /// Add a custom matcher function
        /// </summary>
        public void AddMatcher(Func<HttpRequest, RecordedRequest, bool> matcher)
        {
            Matchers.Add(matcher);
        }

        /// <summary>
        /// Add header to match on
        /// </summary>
        public void AddHeaderToMatch(string headerName)
        {
            if (!HeadersToMatch.Contains(headerName))
            {
                HeadersToMatch.Add(headerName);
            }
        }
    }
}
