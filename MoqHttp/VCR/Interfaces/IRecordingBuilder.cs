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
    }
}
