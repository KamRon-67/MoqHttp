using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
        private IHost? _host;
        private readonly string _hostname;
        private readonly int _port;
        private Cassette? _cassette;
        private ProxyHandler? _proxyHandler;

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

            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls($"http://{_hostname}:{_port}");
                    webBuilder.Configure(app =>
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

                            if (route.Response?.Handler != null)
                            {
                                route.Response.Handler(context);
                                return;
                            }

                            var responseBody = route.Response?.Body ?? string.Empty;
                            context.Response.StatusCode = route.Response?.StatusCode ?? 200;
                            if (route.Response?.Headers != null)
                            {
                                context.Response.Headers.AddRange(route.Response.Headers);
                            }
                            await context.Response.WriteAsync(responseBody, Encoding.UTF8).ConfigureAwait(false);
                        });
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
                // Phase 2: Check if request should be recorded
                if (!config.RecordingFilter.ShouldRecord(context.Request))
                {
                    // Don't record, just proxy through without saving
                    if (_proxyHandler != null)
                    {
                        var interaction = await _proxyHandler.ProxyRequest(context);
                        
                        context.Response.StatusCode = interaction.Response.Status;
                        foreach (var header in interaction.Response.Headers)
                        {
                            if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) || 
                                header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            context.Response.Headers[header.Key] = header.Value;
                        }
                        if (!string.IsNullOrEmpty(interaction.Response.Body))
                        {
                            await context.Response.WriteAsync(interaction.Response.Body, Encoding.UTF8);
                        }
                    }
                    return;
                }

                // Recording mode: proxy to real API and capture
                if (_proxyHandler != null && _cassette != null)
                {
                    var recordedInteraction = await _proxyHandler.ProxyRequest(context);
                    _cassette.Interactions.Add(recordedInteraction);

                    // Send the captured response back to the client
                    context.Response.StatusCode = recordedInteraction.Response.Status;
                    foreach (var header in recordedInteraction.Response.Headers)
                    {
                        if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) || 
                            header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        context.Response.Headers[header.Key] = header.Value;
                    }
                    if (!string.IsNullOrEmpty(recordedInteraction.Response.Body))
                    {
                        await context.Response.WriteAsync(recordedInteraction.Response.Body, Encoding.UTF8);
                    }
                }
            }
            else if (config.Mode == VCRMode.Playback && _cassette != null)
            {
                // Phase 2: Use advanced matching configuration
                var interaction = RequestMatcher.FindMatch(
                    context.Request, 
                    _cassette.Interactions, 
                    config.MatchingConfig);

                if (interaction == null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync(
                        $"VCR: No matching interaction found for {context.Request.Method} {context.Request.Path}",
                        Encoding.UTF8);
                    return;
                }

                // Phase 2: Apply response transformations
                var response = config.ResponseTransform.Apply(interaction.Response);

                // Replay recorded response
                context.Response.StatusCode = response.Status;
                foreach (var header in response.Headers)
                {
                    if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) || 
                        header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    context.Response.Headers[header.Key] = header.Value;
                }
                if (!string.IsNullOrEmpty(response.Body))
                {
                    await context.Response.WriteAsync(response.Body, Encoding.UTF8);
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

