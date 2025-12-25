using MoqHttp.VCR.Matching;
using MoqHttp.VCR.Playback;
using MoqHttp.VCR.Recording;

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

        // Phase 2: Advanced Features

        /// <summary>
        /// Configuration for request matching behavior
        /// </summary>
        public MatchingConfig MatchingConfig { get; set; }

        /// <summary>
        /// Filter for selective recording
        /// </summary>
        public RecordingFilter RecordingFilter { get; set; }

        /// <summary>
        /// Response transformation configuration
        /// </summary>
        public ResponseTransform ResponseTransform { get; set; }

        public VCRConfig()
        {
            Mode = VCRMode.None;
            MatchingConfig = new MatchingConfig();
            RecordingFilter = new RecordingFilter();
            ResponseTransform = new ResponseTransform();
        }
    }
}

