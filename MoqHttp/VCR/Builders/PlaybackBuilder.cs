using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using MoqHttp.VCR.Interfaces;
using MoqHttp.VCR.Models;

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

        // Phase 2: Advanced Matching

        public IPlaybackBuilder MatchOn(Func<HttpRequest, RecordedRequest, bool> matcher)
        {
            _config.MatchingConfig.AddMatcher(matcher);
            return this;
        }

        public IPlaybackBuilder MatchHeaders(params string[] headerNames)
        {
            foreach (var header in headerNames)
            {
                _config.MatchingConfig.AddHeaderToMatch(header);
            }
            return this;
        }

        public IPlaybackBuilder MatchBody()
        {
            _config.MatchingConfig.MatchBody = true;
            return this;
        }

        public IPlaybackBuilder MatchQueryString()
        {
            _config.MatchingConfig.MatchQueryString = true;
            return this;
        }

        // Phase 2: Response Transformation

        public IPlaybackBuilder Transform(Func<RecordedResponse, RecordedResponse> transformer)
        {
            _config.ResponseTransform.AddTransformer(transformer);
            return this;
        }
    }
}

