using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MoqHttpTests.VCR
{
    public class VCRTransformationTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _testCassettePath;
        private MoqHttp.HttpServer _mockServer;
        private const int Port = 8768;
        private readonly string _address;

        public VCRTransformationTests()
        {
            _httpClient = new HttpClient();
            _address = $"http://localhost:{Port}";
            _testCassettePath = Path.Combine(Path.GetTempPath(), $"vcr_transform_{Guid.NewGuid()}.json");
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
        public async Task Transform_Should_Modify_Response_Body()
        {
            // Arrange - Record
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/json");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Playback with transformation
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath)
                .Transform(response => {
                    var json = JObject.Parse(response.Body);
                    json["injected_field"] = "test_value";
                    response.Body = json.ToString();
                    return response;
                });
            _mockServer.Run();

            var playbackResponse = await _httpClient.GetAsync($"{_address}/json");
            var content = await playbackResponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("injected_field", content);
            Assert.Contains("test_value", content);
        }

        [Fact]
        public async Task Transform_Should_Inject_Dynamic_Timestamp()
        {
            // Arrange - Record
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/json");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Add timestamp
            var beforeTime = DateTime.UtcNow;
            
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath)
                .Transform(response => {
                    var json = JObject.Parse(response.Body);
                    json["server_time"] = DateTime.UtcNow.ToString("o");
                    response.Body = json.ToString();
                    return response;
                });
            _mockServer.Run();

            var playbackResponse = await _httpClient.GetAsync($"{_address}/json");
            var content = await playbackResponse.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            // Assert
            Assert.NotNull(json["server_time"]);
            var serverTime = DateTime.Parse(json["server_time"].ToString());
            Assert.True(serverTime >= beforeTime);
            Assert.True(serverTime <= DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public async Task Transform_Should_Add_Custom_Headers()
        {
            // Arrange - Record
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/get");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Add custom header
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath)
                .Transform(response => {
                    response.Headers["X-Custom-Header"] = "CustomValue";
                    response.Headers["X-Test-Mode"] = "true";
                    return response;
                });
            _mockServer.Run();

            var playbackResponse = await _httpClient.GetAsync($"{_address}/get");

            // Assert
            Assert.True(playbackResponse.Headers.Contains("X-Custom-Header"));
            Assert.True(playbackResponse.Headers.Contains("X-Test-Mode"));
            Assert.Equal("CustomValue", playbackResponse.Headers.GetValues("X-Custom-Header").First());
        }

        [Fact]
        public async Task Multiple_Transforms_Should_Chain()
        {
            // Arrange - Record
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/json");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Multiple transformations
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath)
                .Transform(response => {
                    var json = JObject.Parse(response.Body);
                    json["field1"] = "value1";
                    response.Body = json.ToString();
                    return response;
                })
                .Transform(response => {
                    var json = JObject.Parse(response.Body);
                    json["field2"] = "value2";
                    response.Body = json.ToString();
                    return response;
                })
                .Transform(response => {
                    response.Headers["X-Transformed"] = "true";
                    return response;
                });
            _mockServer.Run();

            var playbackResponse = await _httpClient.GetAsync($"{_address}/json");
            var content = await playbackResponse.Content.ReadAsStringAsync();

            // Assert - All transformations applied
            Assert.Contains("field1", content);
            Assert.Contains("value1", content);
            Assert.Contains("field2", content);
            Assert.Contains("value2", content);
            Assert.True(playbackResponse.Headers.Contains("X-Transformed"));
        }

        [Fact]
        public async Task Transform_Should_Not_Affect_Original_Cassette()
        {
            // Arrange - Record
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/json");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Transform during playback
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath)
                .Transform(response => {
                    var json = JObject.Parse(response.Body);
                    json["modified"] = true;
                    response.Body = json.ToString();
                    return response;
                });
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/json");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Assert - Cassette file unchanged
            var cassette = MoqHttp.VCR.IO.CassetteReader.LoadFromFile(_testCassettePath);
            var originalBody = cassette.Interactions[0].Response.Body;
            Assert.DoesNotContain("modified", originalBody);
        }

        [Fact]
        public async Task Transform_Can_Modify_Status_Code()
        {
            // Arrange - Record
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/status/200");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Change status code
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath)
                .Transform(response => {
                    response.Status = 418; // I'm a teapot
                    return response;
                });
            _mockServer.Run();

            var playbackResponse = await _httpClient.GetAsync($"{_address}/status/200");

            // Assert
            Assert.Equal(418, (int)playbackResponse.StatusCode);
        }
    }
}
