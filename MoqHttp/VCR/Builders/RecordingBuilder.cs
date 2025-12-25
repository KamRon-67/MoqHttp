using MoqHttp.VCR.Interfaces;

namespace MoqHttp.VCR.Builders
{
    /// <summary>
    /// Builder for VCR recording mode
    /// </summary>
    public class RecordingBuilder : IRecordingBuilder
    {
        private readonly VCRConfig _config;

        public RecordingBuilder(VCRConfig config)
        {
            _config = config;
            _config.Mode = VCRMode.Record;
        }

        public IRecordingBuilder ToFile(string path)
        {
            _config.FilePath = path;
            return this;
        }

        public IRecordingBuilder ProxyTo(string url)
        {
            _config.ProxyUrl = url;
            return this;
        }
    }
}
