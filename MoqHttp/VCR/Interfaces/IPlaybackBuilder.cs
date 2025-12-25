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
    }
}
