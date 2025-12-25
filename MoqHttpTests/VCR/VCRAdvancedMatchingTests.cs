using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MoqHttpTests.VCR
{
    public class VCRAdvancedMatchingTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _testCassettePath;
        private MoqHttp.HttpServer _mockServer;
        private const int Port = 8766;
        private readonly string _address;

        public VCRAdvancedMatchingTests()
        {
            _httpClient = new HttpClient();
            _address = $"http://localhost:{Port}";
            _testCassettePath = Path.Combine(Path.GetTempPath(), $"vcr_matching_{Guid.NewGuid()}.json");
        }

        public void Dispose()
        {
            _mockServer?.Dispose();
            _httpClient?.Dispose();
            
            if (File.Exists(_testCassettePath))
            {
                File.Delete(_testCassettePath);
            }
        }

        [Fact]
        public async Task MatchHeaders_Should_Match_By_Authorization()
        {
            // Arrange - Record with different auth headers
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer TOKEN123");
            await _httpClient.GetAsync($"{_address}/headers");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Playback with header matching
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath)
                .MatchHeaders("Authorization");
            _mockServer.Run();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer TOKEN123");
            var response = await client.GetAsync($"{_address}/headers");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("TOKEN123", content);
        }

        [Fact]
        public async Task MatchQueryString_Should_Distinguish_Query_Parameters()
        {
            // Arrange - Record multiple requests with different query params
            _mockServer = new MoqHttp.HttpServer (Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/get?param=value1");
            await _httpClient.GetAsync($"{_address}/get?param=value2");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Playback with query string matching
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath)
                .MatchQueryString();
            _mockServer.Run();

            var response1 = await _httpClient.GetAsync($"{_address}/get?param=value1");
            var response2 = await _httpClient.GetAsync($"{_address}/get?param=value2");

            // Assert
            var content1 = await response1.Content.ReadAsStringAsync();
            var content2 = await response2.Content.ReadAsStringAsync();
            
            Assert.True(response1.IsSuccessStatusCode);
            Assert.True(response2.IsSuccessStatusCode);
            Assert.Contains("value1", content1);
            Assert.Contains("value2", content2);
        }

        [Fact]
        public async Task MatchOn_Custom_Matcher_Should_Work()
        {
            // Arrange - Record
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/uuid");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Playback with custom matcher
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath)
                .MatchOn((request, recorded) => 
                    request.Method == recorded.Method && 
                    request.Path.ToString() == "/uuid");
            _mockServer.Run();

            var response = await _httpClient.GetAsync($"{_address}/uuid");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Multiple_Matchers_Should_All_Apply()
        {
            // Arrange - Record with headers and query
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            _httpClient.DefaultRequestHeaders.Add("X-Custom", "test-value");
            await _httpClient.GetAsync($"{_address}/get?id=123");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Playback with multiple matchers
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath)
                .MatchHeaders("X-Custom")
                .MatchQueryString();
            _mockServer.Run();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Custom", "test-value");
            var response = await client.GetAsync($"{_address}/get?id=123");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task MatchHeaders_Wrong_Value_Should_Not_Match()
        {
            // Arrange - Record
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer CORRECT");
            await _httpClient.GetAsync($"{_address}/headers");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Try to match with wrong header value
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath)
                .MatchHeaders("Authorization");
            _mockServer.Run();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer WRONG");
            var response = await client.GetAsync($"{_address}/headers");

            // Assert - Should not find a match
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("No matching interaction", content);
        }
    }
}
