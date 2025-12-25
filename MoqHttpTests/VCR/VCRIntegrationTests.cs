using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MoqHttpTests.VCR
{
    public class VCRIntegrationTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _testCassettePath;
        private MoqHttp.HttpServer _mockServer;
        private const int Port = 8765;
        private readonly string _address;

        public VCRIntegrationTests()
        {
            _httpClient = new HttpClient();
            _address = $"http://localhost:{Port}";
            _testCassettePath = Path.Combine(Path.GetTempPath(), $"vcr_test_{Guid.NewGuid()}.json");
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
        public async Task Record_Mode_Should_Create_Cassette_File()
        {
            // Arrange
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            
            _mockServer.Run();

            // Act
            var response = await _httpClient.GetAsync($"{_address}/get?test=value");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            
            // Give a moment for disposal to save file
            _mockServer.Dispose();
            _mockServer = null;

            Assert.True(File.Exists(_testCassettePath));
            
            // Verify cassette content
            var cassetteContent = File.ReadAllText(_testCassettePath);
            Assert.Contains("\"Method\": \"GET\"", cassetteContent);
            Assert.Contains("/get?test=value", cassetteContent);
        }

        [Fact]
        public async Task Playback_Mode_Should_Replay_From_Cassette()
        {
            // Arrange - First record an interaction
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/uuid");
            _mockServer.Dispose();

            // Wait a moment to ensure file is written
            await Task.Delay(100);

            // Act - Now playback
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath);
            _mockServer.Run();

            var response = await _httpClient.GetAsync($"{_address}/uuid");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotEmpty(content);
            Assert.Contains("uuid", content.ToLower());
        }

        [Fact]
        public async Task Auto_Mode_Should_Record_On_First_Run()
        {
            // Arrange
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Auto()
                .FromFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            // Act
            var response = await _httpClient.GetAsync($"{_address}/get");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            
            _mockServer.Dispose();
            _mockServer = null;

            // Verify cassette was created
            Assert.True(File.Exists(_testCassettePath));
        }

        [Fact]
        public async Task Auto_Mode_Should_Playback_On_Subsequent_Run()
        {
            // Arrange - First run to create cassette
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Auto()
                .FromFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            var firstResponse = await _httpClient.GetAsync($"{_address}/uuid");
            var firstContent = await firstResponse.Content.ReadAsStringAsync();
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Second run should playback
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Auto()
                .FromFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            var secondResponse = await _httpClient.GetAsync($"{_address}/uuid");
            var secondContent = await secondResponse.Content.ReadAsStringAsync();

            // Assert - Should get same content (playback mode)
            Assert.Equal(firstContent, secondContent);
        }

        [Fact]
        public async Task Playback_Mode_Should_Return_404_For_Unmatched_Request()
        {
            // Arrange - Create cassette with one interaction
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Record()
                .ToFile(_testCassettePath)
                .ProxyTo("https://httpbin.org");
            _mockServer.Run();

            await _httpClient.GetAsync($"{_address}/get");
            _mockServer.Dispose();

            await Task.Delay(100);

            // Act - Request different endpoint
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config
                .Playback()
                .FromFile(_testCassettePath);
            _mockServer.Run();

            var response = await _httpClient.GetAsync($"{_address}/post");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("No matching interaction", content);
        }

        [Fact]
        public async Task VCR_Should_Not_Interfere_With_Normal_Mocking()
        {
            // Arrange
            _mockServer = new MoqHttp.HttpServer(Port);
            _mockServer.Config.Get("/test").Send("Normal Mock");
            _mockServer.Run();

            // Act
            var response = await _httpClient.GetAsync($"{_address}/test");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("Normal Mock", content);
        }
    }
}
