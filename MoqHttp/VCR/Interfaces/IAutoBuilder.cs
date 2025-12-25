namespace MoqHttp.VCR.Interfaces
{
    /// <summary>
    /// Fluent interface for configuring VCR auto mode
    /// </summary>
    public interface IAutoBuilder
    {
        /// <summary>
        /// Specify the file path for the cassette
        /// </summary>
        /// <param name="path">Path to the cassette file</param>
        /// <returns>The builder for method chaining</returns>
        IAutoBuilder FromFile(string path);

        /// <summary>
        /// Specify the target URL to proxy requests to (when recording)
        /// </summary>
        /// <param name="url">The target API URL</param>
        /// <returns>The builder for method chaining</returns>
        IAutoBuilder ProxyTo(string url);
    }
}
