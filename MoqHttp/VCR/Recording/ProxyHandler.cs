using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MoqHttp.VCR.Models;

namespace MoqHttp.VCR.Recording
{
    /// <summary>
    /// Proxies HTTP requests to a real API and captures the interactions
    /// </summary>
    public class ProxyHandler
    {
        private readonly string _targetUrl;
        private readonly HttpClient _httpClient;

        public ProxyHandler(string targetUrl)
        {
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                throw new ArgumentException("Target URL cannot be empty", nameof(targetUrl));
            }

            _targetUrl = targetUrl.TrimEnd('/');
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// Proxy the request to the target URL and capture the interaction
        /// </summary>
        /// <param name="context">The HTTP context from the mock server</param>
        /// <returns>The captured interaction</returns>
        public async Task<Interaction> ProxyRequest(HttpContext context)
        {
            var interaction = new Interaction();

            // Capture request
            interaction.Request = await CaptureRequest(context.Request);

            // Forward to target API
            var response = await ForwardRequest(context.Request);

            // Capture response
            interaction.Response = await CaptureResponse(response);

            return interaction;
        }

        private async Task<RecordedRequest> CaptureRequest(HttpRequest request)
        {
            var recorded = new RecordedRequest
            {
                Method = request.Method,
                Uri = _targetUrl + request.Path + request.QueryString
            };

            // Capture headers (excluding host-specific headers)
            foreach (var header in request.Headers.Where(h => !IsHostHeader(h.Key)))
            {
                recorded.Headers[header.Key] = header.Value.ToString();
            }

            // Capture query parameters
            foreach (var query in request.Query)
            {
                recorded.QueryParameters[query.Key] = query.Value.ToString();
            }

            // Capture body
            if (request.ContentLength > 0)
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                recorded.Body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            return recorded;
        }

        private async Task<HttpResponseMessage> ForwardRequest(HttpRequest request)
        {
            var targetUri = _targetUrl + request.Path + request.QueryString;
            var httpRequest = new HttpRequestMessage(new HttpMethod(request.Method), targetUri);

            // Copy headers
            foreach (var header in request.Headers.Where(h => !IsHostHeader(h.Key)))
            {
                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            // Copy body
            if (request.ContentLength > 0)
            {
                request.EnableBuffering();
                var memoryStream = new MemoryStream();
                await request.Body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                request.Body.Position = 0;
                httpRequest.Content = new StreamContent(memoryStream);

                if (request.ContentType != null)
                {
                    httpRequest.Content.Headers.TryAddWithoutValidation("Content-Type", request.ContentType);
                }
            }

            return await _httpClient.SendAsync(httpRequest);
        }

        private async Task<RecordedResponse> CaptureResponse(HttpResponseMessage response)
        {
            var recorded = new RecordedResponse
            {
                Status = (int)response.StatusCode
            };

            // Capture headers
            foreach (var header in response.Headers)
            {
                recorded.Headers[header.Key] = string.Join(", ", header.Value);
            }

            if (response.Content?.Headers != null)
            {
                foreach (var header in response.Content.Headers)
                {
                    recorded.Headers[header.Key] = string.Join(", ", header.Value);
                }
            }

            // Capture body
            if (response.Content != null)
            {
                recorded.Body = await response.Content.ReadAsStringAsync();
            }

            return recorded;
        }

        private static bool IsHostHeader(string headerName)
        {
            // Headers that shouldn't be copied
            var excluded = new[] { "Host", "Connection", "Content-Length" };
            return excluded.Contains(headerName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
