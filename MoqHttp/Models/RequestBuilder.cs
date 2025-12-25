using System.Collections.Generic;
using System.Linq;
using MoqHttp.Interfaces;
using MoqHttp.VCR;
using MoqHttp.VCR.Builders;
using MoqHttp.VCR.Interfaces;

namespace MoqHttp.Models
{
    public class RequestBuilder : IRequestBuilder
    {
        public IRequestHandler Handler { get; set; }

        public List<RouteTableItem> RouteTable { get; }

        // VCR Configuration
        public VCRConfig VCRConfig { get; }

        public RequestBuilder()
        {
            Handler = new RequestHandler(null);
            RouteTable = new List<RouteTableItem>();
            VCRConfig = new VCRConfig();
        }

        public IRequestHandler Delete(string url)
        {
            return Request("DELETE", url, null);
        }

        public IRequestHandler Get(string url)
        {
            return Request("GET", url, null);
        }

        public IRequestHandler Post(string url)
        {
            return Request("POST", url, null);
        }

        public IRequestHandler Put(string url)
        {
            return Request("PUT", url, null);
        }

        public IRequestHandler Request(string method, string url)
        {
            return Request(method, url, null);
        }

        public IRequestHandler Request(string method, string url, Dictionary<string, string> headers)
        {
            RouteTable.Add(new RouteTableItem(method, url, headers));
            Handler = new RequestHandler(RouteTable.Last());
            return Handler;
        }

        // VCR Methods
        public IRecordingBuilder Record()
        {
            return new RecordingBuilder(VCRConfig);
        }

        public IPlaybackBuilder Playback()
        {
            return new PlaybackBuilder(VCRConfig);
        }

        public IAutoBuilder Auto()
        {
            return new AutoBuilder(VCRConfig);
        }
    }
}
