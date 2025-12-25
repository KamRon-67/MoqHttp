using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using MoqHttp.VCR.Matching;
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
        /// <param name="matchingConfig">Optional matching configuration</param>
        /// <returns>The first matching interaction, or null if no match found</returns>
        public static Interaction FindMatch(
            HttpRequest request, 
            List<Interaction> interactions,
            MatchingConfig matchingConfig = null)
        {
            if (request == null || interactions == null || interactions.Count == 0)
            {
                return null;
            }

            matchingConfig ??= new MatchingConfig();

            // Find first interaction that matches all criteria
            return interactions.FirstOrDefault(interaction =>
                MatchesMethod(request, interaction) &&
                MatchesPath(request, interaction) &&
                MatchesHeaders(request, interaction, matchingConfig) &&
                MatchesBody(request, interaction, matchingConfig) &&
                MatchesQueryString(request, interaction, matchingConfig) &&
                MatchesCustomMatchers(request, interaction, matchingConfig)
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
                recordedPath = uri.AbsolutePath;
            }
            else if (recordedUri.Contains("?"))
            {
                // Remove query string from recorded path
                recordedPath = recordedUri.Split('?')[0];
            }

            var requestPath = request.Path.ToString();

            return requestPath.Equals(recordedPath, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool MatchesHeaders(HttpRequest request, Interaction interaction, MatchingConfig config)
        {
            if (config.HeadersToMatch.Count == 0)
            {
                return true; // No header matching required
            }

            foreach (var headerName in config.HeadersToMatch)
            {
                var requestHeaderValue = request.Headers[headerName].ToString();
                
                if (!interaction.Request.Headers.TryGetValue(headerName, out var recordedHeaderValue))
                {
                    return false; // Header not in recorded request
                }

                if (!requestHeaderValue.Equals(recordedHeaderValue, System.StringComparison.OrdinalIgnoreCase))
                {
                    return false; // Header values don't match
                }
            }

            return true;
        }

        private static bool MatchesBody(HttpRequest request, Interaction interaction, MatchingConfig config)
        {
            if (!config.MatchBody)
            {
                return true; // Body matching not required
            }

            // Read request body
            string requestBody = null;
            if (request.ContentLength > 0)
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                requestBody = reader.ReadToEndAsync().GetAwaiter().GetResult();
                request.Body.Position = 0;
            }

            var recordedBody = interaction.Request.Body;

            // Both null or empty
            if (string.IsNullOrEmpty(requestBody) && string.IsNullOrEmpty(recordedBody))
            {
                return true;
            }

            // One is null/empty, other is not
            if (string.IsNullOrEmpty(requestBody) || string.IsNullOrEmpty(recordedBody))
            {
                return false;
            }

            return requestBody.Equals(recordedBody);
        }

        private static bool MatchesQueryString(HttpRequest request, Interaction interaction, MatchingConfig config)
        {
            if (!config.MatchQueryString)
            {
                return true; // Query string matching not required
            }

            var requestQueryString = request.QueryString.ToString();
            
            // Extract query string from recorded URI
            var recordedUri = interaction.Request.Uri;
            var recordedQueryString = "";
            
            if (recordedUri.Contains("?"))
            {
                recordedQueryString = "?" + recordedUri.Split('?')[1];
            }

            return requestQueryString.Equals(recordedQueryString, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool MatchesCustomMatchers(HttpRequest request, Interaction interaction, MatchingConfig config)
        {
            if (config.Matchers.Count == 0)
            {
                return true; // No custom matchers
            }

            // All custom matchers must pass
            foreach (var matcher in config.Matchers)
            {
                if (!matcher(request, interaction.Request))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

