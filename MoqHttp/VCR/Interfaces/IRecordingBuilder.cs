using System;
using Microsoft.AspNetCore.Http;

namespace MoqHttp.VCR.Interfaces
{
    /// <summary>
    /// Fluent interface for configuring VCR recording mode
    /// </summary>
    public interface IRecordingBuilder
    {
        /// <summary>
        /// Specify the file path where the cassette will be saved
        /// </summary>
        /// <param name="path">Path to the cassette file</param>
        /// <returns>The builder for method chaining</returns>
        IRecordingBuilder ToFile(string path);

        /// <summary>
        /// Specify the target URL to proxy requests to
        /// </summary>
        /// <param name="url">The target API URL</param>
        /// <returns>The builder for method chaining</returns>
        IRecordingBuilder ProxyTo(string url);

        // Phase 2: Selective Recording

        /// <summary>
        /// Add a filter predicate for selective recording
        /// </summary>
        /// <param name="predicate">Function to determine if request should be recorded</param>
        /// <returns>The builder for method chaining</returns>
        IRecordingBuilder Filter(Func<HttpRequest, bool> predicate);

        /// <summary>
        /// Only record specific HTTP methods
        /// </summary>
        /// <param name="methods">HTTP methods to record (e.g., "GET", "POST")</param>
        /// <returns>The builder for method chaining</returns>
        IRecordingBuilder OnlyMethods(params string[] methods);

        /// <summary>
        /// Only record paths starting with specific prefixes
        /// </summary>
        /// <param name="pathPrefixes">Path prefixes to record (e.g., "/api/", "/v1/")</param>
        /// <returns>The builder for method chaining</returns>
        IRecordingBuilder OnlyPaths(params string[] pathPrefixes);
    }
}

