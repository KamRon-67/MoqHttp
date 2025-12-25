using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
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

        // Phase 2: Selective Recording

        public IRecordingBuilder Filter(Func<HttpRequest, bool> predicate)
        {
            _config.RecordingFilter.AddFilter(predicate);
            return this;
        }

        public IRecordingBuilder OnlyMethods(params string[] methods)
        {
            return Filter(request => methods.Contains(request.Method, StringComparer.OrdinalIgnoreCase));
        }

        public IRecordingBuilder OnlyPaths(params string[] pathPrefixes)
        {
            return Filter(request => pathPrefixes.Any(prefix => 
                request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase)));
        }
    }
}

