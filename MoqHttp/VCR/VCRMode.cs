namespace MoqHttp.VCR
{
    /// <summary>
    /// VCR operation mode
    /// </summary>
    public enum VCRMode
    {
        /// <summary>
        /// VCR is disabled, use normal mocking
        /// </summary>
        None,

        /// <summary>
        /// Record HTTP interactions to cassette
        /// </summary>
        Record,

        /// <summary>
        /// Replay interactions from cassette
        /// </summary>
        Playback,

        /// <summary>
        /// Automatically record if cassette doesn't exist, otherwise playback
        /// </summary>
        Auto
    }
}
