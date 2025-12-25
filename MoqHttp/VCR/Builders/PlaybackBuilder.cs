using MoqHttp.VCR.Interfaces;

namespace MoqHttp.VCR.Builders
{
    /// <summary>
    /// Builder for VCR playback mode
    /// </summary>
    public class PlaybackBuilder : IPlaybackBuilder
    {
        private readonly VCRConfig _config;

        public PlaybackBuilder(VCRConfig config)
        {
            _config = config;
            _config.Mode = VCRMode.Playback;
        }

        public IPlaybackBuilder FromFile(string path)
        {
            _config.FilePath = path;
            return this;
        }
    }
}
