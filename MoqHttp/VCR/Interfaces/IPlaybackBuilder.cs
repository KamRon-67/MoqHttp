using System;
using Microsoft.AspNetCore.Http;
using MoqHttp.VCR.Models;

namespace MoqHttp.VCR.Interfaces
{
    /// <summary>
    /// Fluent interface for configuring VCR playback mode
    /// </summary>
    public interface IPlaybackBuilder
    {
        /// <summary>
        /// Specify the file path from which to load the cassette
        /// </summary>
        /// <param name="path">Path to the cassette file</param>
        /// <returns>The builder for method chaining</returns>
        IPlaybackBuilder FromFile(string path);

        // Phase 2: Advanced Matching

        /// <summary>
        /// Add a custom matcher function
        /// </summary>
        /// <param name="matcher">Function to match request to recorded interaction</param>
        /// <returns>The builder for method chaining</returns>
        IPlaybackBuilder MatchOn(Func<HttpRequest, RecordedRequest, bool> matcher);

        /// <summary>
        /// Match on specific headers
        /// </summary>
        /// <param name="headerNames">Header names to match on</param>
        /// <returns>The builder for method chaining</returns>
        IPlaybackBuilder MatchHeaders(params string[] headerNames);

        /// <summary>
        /// Match on request body
        /// </summary>
        /// <returns>The builder for method chaining</returns>
        IPlaybackBuilder MatchBody();

        /// <summary>
        /// Match on query string
        /// </summary>
        /// <returns>The builder for method chaining</returns>
        IPlaybackBuilder MatchQueryString();

        // Phase 2: Response Transformation

        /// <summary>
        /// Add a response transformation function
        /// </summary>
        /// <param name="transformer">Function to transform response</param>
        /// <returns>The builder for method chaining</returns>
        IPlaybackBuilder Transform(Func<RecordedResponse, RecordedResponse> transformer);
    }
}

