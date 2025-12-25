using MoqHttp.VCR.Interfaces;

namespace MoqHttp.VCR.Builders
{
    /// <summary>
    /// Builder for VCR auto mode
    /// </summary>
    public class AutoBuilder : IAutoBuilder
    {
        private readonly VCRConfig _config;

        public AutoBuilder(VCRConfig config)
        {
            _config = config;
            _config.Mode = VCRMode.Auto;
        }

        public IAutoBuilder FromFile(string path)
        {
            _config.FilePath = path;
            return this;
        }

        public IAutoBuilder ProxyTo(string url)
        {
            _config.ProxyUrl = url;
            return this;
        }
    }
}
