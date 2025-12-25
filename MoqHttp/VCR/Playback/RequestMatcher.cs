using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using MoqHttp.VCR.Models;

namespace MoqHttp.VCR.Playback
{
    /// <summary>
    /// Matches incoming HTTP requests to recorded interactions
    /// </summary>
    public class RequestMatcher
    {
        /// <summary>
        /// Find a matching interaction for the given request
        /// </summary>
        /// <param name="request">The incoming HTTP request</param>
        /// <param name="interactions">List of recorded interactions</param>
        /// <returns>The first matching interaction, or null if no match found</returns>
        public static Interaction FindMatch(HttpRequest request, List<Interaction> interactions)
        {
            if (request == null || interactions == null || interactions.Count == 0)
            {
                return null;
            }

            // Default matching strategy: Method + Path
            return interactions.FirstOrDefault(interaction =>
                MatchesMethod(request, interaction) &&
                MatchesPath(request, interaction)
            );
        }

        private static bool MatchesMethod(HttpRequest request, Interaction interaction)
        {
            return request.Method.Equals(interaction.Request.Method, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool MatchesPath(HttpRequest request, Interaction interaction)
        {
            // Extract path from the recorded URI
            var recordedUri = interaction.Request.Uri;
            var recordedPath = recordedUri;

            // If it's a full URI, extract just the path
            if (recordedUri.StartsWith("http://") || recordedUri.StartsWith("https://"))
            {
                var uri = new System.Uri(recordedUri);
                recordedPath = uri.PathAndQuery;
            }

            var requestPath = request.Path.ToString();
            if (request.QueryString.HasValue)
            {
                requestPath += request.QueryString.ToString();
            }

            return requestPath.Equals(recordedPath, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
