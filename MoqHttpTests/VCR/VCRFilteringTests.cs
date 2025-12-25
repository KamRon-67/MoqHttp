using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MoqHttp.VCR.IO;
using Xunit;

namespace MoqHttpTests.VCR
{
    public class VCRFilteringTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _testCassettePath;
        private MoqHttp.HttpServer _mockServer;
        private const int Port = 8767;
        private readonly string _address;

        public VCRFilteringTests()
        {
            _httpClient = new HttpClient();
            _address = $"http://localhost:{Port}";
            _testCassettePath = Path.Combine(Path.GetTempPath(), $"vcr_filter_{Guid.NewGuid()}.json");
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
        public async Task OnlyMethods_Should_Filter_By_HTTP_Method()
        {
            // Arrange & Act
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .OnlyMethods("GET")  // Only record GET requests
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/get");        // Should record
            await _httpClient.PostAsync($"{_address}/post", null); // Should NOT record
            _mockServer.Dispose();

            await Task.Delay(100);

            // Assert
            var cassette = CassetteReader.LoadFromFile(_testCassettePath);
            Assert.Single(cassette.Interactions); // Only GET recorded
            Assert.Equal("GET", cassette.Interactions[0].Request.Method);
        }

        [Fact]
        public async Task OnlyPaths_Should_Filter_By_Path_Prefix()
        {
            // Arrange & Act
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .OnlyPaths("/get")  // Only record /get paths
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/get");     // Should record
            await _httpClient.GetAsync($"{_address}/uuid");    // Should NOT record
            _mockServer.Dispose();

            await Task.Delay(100);

            // Assert
            var cassette = CassetteReader.LoadFromFile(_testCassettePath);
            Assert.Single(cassette.Interactions); // Only /get recorded
            Assert.Contains("/get", cassette.Interactions[0].Request.Uri);
        }

        [Fact]
        public async Task Custom_Filter_Should_Work()
        {
            // Arrange & Act
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .Filter(r => r.Path.ToString().Contains("get"))  // Only paths with "get"
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/get");     // Should record
            await _httpClient.GetAsync($"{_address}/uuid");    // Should NOT record
            _mockServer.Dispose();

            await Task.Delay(100);

            // Assert
            var cassette = CassetteReader.LoadFromFile(_testCassettePath);
            Assert.Single(cassette.Interactions);
        }

        [Fact]
        public async Task Multiple_Filters_Should_All_Apply()
        {
            // Arrange & Act
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .OnlyMethods("GET")
                .OnlyPaths("/get")
                .Filter(r => !r.QueryString.ToString().Contains("skip"))
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/get");              // Should record
            await _httpClient.GetAsync($"{_address}/get?skip=true");    // Filtered by query
            await _httpClient.PostAsync($"{_address}/get", null);       // Filtered by method
            await _httpClient.GetAsync($"{_address}/uuid");             // Filtered by path
            _mockServer.Dispose();

            await Task.Delay(100);

            // Assert
            var cassette = CassetteReader.LoadFromFile(_testCassettePath);
            Assert.Single(cassette.Interactions); // Only first request recorded
        }

        [Fact]
        public async Task Filtered_Requests_Should_Still_Proxy_But_Not_Record()
        {
            // Arrange
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record ()
                .OnlyMethods("POST")  // Only record POST
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            // Act - GET should proxy but not record
            var response = await _httpClient.GetAsync($"{_address}/get");
            
            // Assert - Request succeeded (proxied)
            Assert.True(response.IsSuccessStatusCode);
            
            _mockServer.Dispose();
            await Task.Delay(100);

            // But not recorded
            var cassette = CassetteReader.LoadFromFile(_testCassettePath);
            Assert.Empty(cassette.Interactions);
        }

        [Fact]
        public async Task No_Filters_Should_Record_Everything()
        {
            // Arrange & Act
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/get");
            await _httpClient.GetAsync($"{_address}/uuid");
            await _httpClient.PostAsync($"{_address}/post", null);
            _mockServer.Dispose();

            await Task.Delay(100);

            // Assert - All recorded
            var cassette = CassetteReader.LoadFromFile(_testCassettePath);
            Assert.Equal(3, cassette.Interactions.Count);
        }
    }
}
