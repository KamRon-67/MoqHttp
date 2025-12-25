using Xunit;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace MoqHttp.Test
{
    public class HttpServerTests
    {
        private HttpServer? _mockServer;
        private readonly Dictionary<string, string> _headers;
        private readonly HttpClient _httpClient;
        private const string Host = "localhost";
        private int Port;
        private string _address;

        public HttpServerTests()
        {
            //Arrange
            Port = new Random().Next(30000, 50000);
            _address = $"http://{Host}:{Port}";

            _headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"testHeader", "TestHeaderValue"}
            };

            _httpClient = new HttpClient();
        }

        [Fact]
        public async Task Server_With_No_RouteTable_Should_Return_Default_Response()
        {
            //Arrange
            _mockServer = new HttpServer(Port);
            _mockServer.Run();

            //Act
            var defaultResponse = await _httpClient.GetAsync(_address);
            _mockServer.Dispose();

            //Assert
            Assert.Equal("It Works!", await defaultResponse.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)defaultResponse.StatusCode);
        }

        [Fact]
        public async Task Post_Should_Work_Correctly()
        {
            //Arrange
            _mockServer = new HttpServer(Port);
            _mockServer.Run();

            //Act
            _mockServer.Config.Post("/havij/123").Send("It Not Works!", 503);
            HttpContent postData = new StringContent("{\"data\":\"Test\"}");
            var responsePost = await _httpClient.PostAsync($"{_address}/havij/123", postData);
            _mockServer.Dispose();

            //Assert
            Assert.Equal(503, (int)responsePost.StatusCode);
        }

        [Fact]

        public async Task Put_Should_Work_Correctly()
        {
            //Arrange
            _mockServer = new HttpServer(Port);
            _mockServer.Run();

            //Act
            _mockServer.Config.Put("/testPut/456").Send("{\"status\":\"isWorking\"}", 200, _headers);
            HttpContent putData = new StringContent("{\"data\":\"Test\"}");
            var putMessage = new HttpRequestMessage(HttpMethod.Put, $"{_address}/testPut/456") { Content = putData };
            var responsePut = await _httpClient.SendAsync(putMessage);
            _mockServer.Dispose();

            //Assert
            Assert.Equal("{\"status\":\"isWorking\"}", await responsePut.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)responsePut.StatusCode);
            Assert.Equal("application/json", responsePut.Content.Headers.GetValues("Content-Type").First());
            Assert.Equal("TestHeaderValue", responsePut.Headers.GetValues("testHeader").First());
        }

        [Fact]
        public async Task Delete_Should_Work_Correctly()
        {
            //Arrange
            _mockServer = new HttpServer(Port);
            _mockServer.Run();

            //Act
            _mockServer.Config.Delete("/testDel/456").Send("Deleted", 200);
            var deleteMessage = new HttpRequestMessage(HttpMethod.Delete, $"{_address}/testDel/456");
            var responseDelete = await _httpClient.SendAsync(deleteMessage);
            _mockServer.Dispose();

            //Assert
            Assert.Equal("Deleted", await responseDelete.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)responseDelete.StatusCode);
        }

        [Fact]
        public async Task Get_Using_Json_Should_Work()
        {
            //Arrange
            _mockServer = new HttpServer(Port);
            _mockServer.Run();
            var filename = "./../../../test.json";

            //Act
            _mockServer.Config.Get("/test/123").ReadJSONFromFile(filename);
            var responseGet = await _httpClient.GetAsync($"{_address}/test/123");

            _mockServer.Config.Get("/testAction/123").Send(context =>
            {
                context.Response.StatusCode = 200;
                const string response = "Action Test";
                var buffer = System.Text.Encoding.UTF8.GetBytes(response);
                context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            });
            var responseGetAction = await _httpClient.GetAsync($"{_address}/testAction/123");
            _mockServer.Dispose();

            var jsonString = File.ReadAllText(filename);
            using var doc = JsonDocument.Parse(jsonString);
            var expectedJson = doc.RootElement.GetRawText();

            // Assert
            // System.Text.Json GetRawText might have slightly different formatting than the original file or Newtonsoft
            // so we parse both to ensure they are semantically equivalent
            var actualJsonString = await responseGet.Content.ReadAsStringAsync();
            using var actualDoc = JsonDocument.Parse(actualJsonString);
            
            Assert.Equal(expectedJson, actualDoc.RootElement.GetRawText());
            Assert.Equal(200, (int)responseGet.StatusCode);

            Assert.Equal("Action Test", await responseGetAction.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)responseGetAction.StatusCode);
        }
    }
}

