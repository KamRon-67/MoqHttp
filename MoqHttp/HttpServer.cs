using System.Linq;
using System.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MoqHttp.Interfaces;
using MoqHttp.Models;
using MoqHttp.VCR;
using MoqHttp.VCR.IO;
using MoqHttp.VCR.Models;
using MoqHttp.VCR.Playback;
using MoqHttp.VCR.Recording;

namespace MoqHttp
{
    public class HttpServer : IHttpServer
    {
        private IWebHost _host;
        private readonly string _hostname;
        private readonly int _port;
        private Cassette _cassette;
        private ProxyHandler _proxyHandler;

        public IRequestBuilder Config { get; set; }

        public HttpServer(int port = 5000, string hostname = "localhost")
        {
            _hostname = hostname;
            _port = port;
            Config = new RequestBuilder();
        }

        public void Run()
        {
            // Initialize VCR if enabled
            InitializeVCR();

            _host = WebHost.CreateDefaultBuilder()
                .UseUrls($"http://{_hostname}:{_port}")
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        var requestBuilder = Config as RequestBuilder;
                        
                        // VCR takes precedence over normal routing
                        if (requestBuilder?.VCRConfig?.IsEnabled == true)
                        {
                            await HandleVCRRequest(context, requestBuilder.VCRConfig);
                            return;
                        }

                        // Normal mocking logic (backward compatible)
                        var route = Config.RouteTable?.LastOrDefault(x => x.IsMatch(context.Request));
                        
                        switch (route)
                        {
                            case null when context.Request.Path == @"/":
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("It Works!", Encoding.UTF8);
                                return;
                            case null:
                                context.Response.StatusCode = StatusCodes.Status404NotFound;
                                context.Response.ContentType = "text/plain";
                                await context.Response.WriteAsync("Page not found!", Encoding.UTF8);
                                return;
                        }

                        if (route.Response.Handler != null)
                        {
                            route.Response.Handler(context);
                            return;
                        }

                        var response = route.Response.Body;
                        context.Response.StatusCode = route.Response.StatusCode;
                        context.Response.Headers.AddRange(route.Response.Headers);
                        await context.Response.WriteAsync(response, Encoding.UTF8).ConfigureAwait(false);
                    });
                }).Build();
            _host.Start();
        }

        private void InitializeVCR()
        {
            var requestBuilder = Config as RequestBuilder;
            if (requestBuilder?.VCRConfig?.IsEnabled != true)
            {
                return;
            }

            var config = requestBuilder.VCRConfig;

            switch (config.Mode)
            {
                case VCRMode.Record:
                    _cassette = new Cassette();
                    _proxyHandler = new ProxyHandler(config.ProxyUrl);
                    break;

                case VCRMode.Playback:
                    _cassette = CassetteReader.LoadFromFile(config.FilePath);
                    break;

                case VCRMode.Auto:
                    if (CassetteReader.Exists(config.FilePath))
                    {
                        // File exists - use playback mode
                        _cassette = CassetteReader.LoadFromFile(config.FilePath);
                        config.Mode = VCRMode.Playback;
                    }
                    else
                    {
                        // File doesn't exist - use recording mode
                        _cassette = new Cassette();
                        _proxyHandler = new ProxyHandler(config.ProxyUrl);
                        config.Mode = VCRMode.Record;
                    }
                    break;
            }
        }

        private async System.Threading.Tasks.Task HandleVCRRequest(HttpContext context, VCRConfig config)
        {
            if (config.Mode == VCRMode.Record)
            {
                // Recording mode: proxy to real API and capture
                var interaction = await _proxyHandler.ProxyRequest(context);
                _cassette.Interactions.Add(interaction);

                // Send the captured response back to the client
                context.Response.StatusCode = interaction.Response.Status;
                foreach (var header in interaction.Response.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value;
                }
                if (!string.IsNullOrEmpty(interaction.Response.Body))
                {
                    await context.Response.WriteAsync(interaction.Response.Body, Encoding.UTF8);
                }
            }
            else if (config.Mode == VCRMode.Playback)
            {
                // Playback mode: find matching interaction
                var interaction = RequestMatcher.FindMatch(context.Request, _cassette.Interactions);

                if (interaction == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync(
                        $"VCR: No matching interaction found for {context.Request.Method} {context.Request.Path}",
                        Encoding.UTF8);
                    return;
                }

                // Replay recorded response
                context.Response.StatusCode = interaction.Response.Status;
                foreach (var header in interaction.Response.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value;
                }
                if (!string.IsNullOrEmpty(interaction.Response.Body))
                {
                    await context.Response.WriteAsync(interaction.Response.Body, Encoding.UTF8);
                }
            }
        }

        public void Dispose()
        {
            // Save cassette if in recording mode
            var requestBuilder = Config as RequestBuilder;
            if (requestBuilder?.VCRConfig?.Mode == VCRMode.Record && _cassette != null)
            {
                CassetteWriter.SaveToFile(_cassette, requestBuilder.VCRConfig.FilePath);
            }

            _host?.Dispose();
        }
    }
}

