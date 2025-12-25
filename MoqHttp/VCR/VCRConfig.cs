namespace MoqHttp.VCR
{
    /// <summary>
    /// Configuration for VCR functionality
    /// </summary>
    public class VCRConfig
    {
        /// <summary>
        /// Current VCR mode
        /// </summary>
        public VCRMode Mode { get; set; }

        /// <summary>
        /// Path to cassette file
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Target URL for proxying requests in record mode
        /// </summary>
        public string ProxyUrl { get; set; }

        /// <summary>
        /// Whether VCR is enabled
        /// </summary>
        public bool IsEnabled => Mode != VCRMode.None;

        public VCRConfig()
        {
            Mode = VCRMode.None;
        }
    }
}
