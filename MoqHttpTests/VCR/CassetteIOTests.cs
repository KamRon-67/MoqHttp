using System;
using System.IO;
using MoqHttp.VCR.IO;
using MoqHttp.VCR.Models;
using Xunit;

namespace MoqHttpTests.VCR
{
    public class CassetteIOTests : IDisposable
    {
        private readonly string _testFilePath;

        public CassetteIOTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_cassette_{Guid.NewGuid()}.json");
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Fact]
        public void SaveToFile_Should_Create_Valid_JSON()
        {
            // Arrange
            var cassette = new Cassette();
            cassette.Interactions.Add(new Interaction
            {
                Request = new RecordedRequest
                {
                    Method = "GET",
                    Uri = "https://api.example.com/test",
                    Body = null
                },
                Response = new RecordedResponse
                {
                    Status = 200,
                    Body = "{\"test\": \"data\"}"
                }
            });

            // Act
            CassetteWriter.SaveToFile(cassette, _testFilePath);

            // Assert
            Assert.True(File.Exists(_testFilePath));
            var content = File.ReadAllText(_testFilePath);
            Assert.Contains("\"Version\"", content);
            Assert.Contains("\"Interactions\"", content);
            Assert.Contains("GET", content);
        }

        [Fact]
        public void LoadFromFile_Should_Load_Cassette_Successfully()
        {
            // Arrange
            var originalCassette = new Cassette();
            originalCassette.Interactions.Add(new Interaction
            {
                Request = new RecordedRequest
                {
                    Method = "POST",
                    Uri = "https://api.example.com/create",
                    Body = "{\"name\": \"test\"}"
                },
                Response = new RecordedResponse
                {
                    Status = 201,
                    Body = "{\"id\": 123}"
                }
            });
            CassetteWriter.SaveToFile(originalCassette, _testFilePath);

            // Act
            var loadedCassette = CassetteReader.LoadFromFile(_testFilePath);

            // Assert
            Assert.NotNull(loadedCassette);
            Assert.Equal("1.0", loadedCassette.Version);
            Assert.Single(loadedCassette.Interactions);
            Assert.Equal("POST", loadedCassette.Interactions[0].Request.Method);
            Assert.Equal(201, loadedCassette.Interactions[0].Response.Status);
        }

        [Fact]
        public void LoadFromFile_Should_Throw_When_File_Not_Found()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "does_not_exist.json");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => CassetteReader.LoadFromFile(nonExistentPath));
        }

        [Fact]
        public void Exists_Should_Return_True_When_File_Exists()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "{}");

            // Act
            var exists = CassetteReader.Exists(_testFilePath);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public void Exists_Should_Return_False_When_File_Does_Not_Exist()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "does_not_exist.json");

            // Act
            var exists = CassetteReader.Exists(nonExistentPath);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public void SaveToFile_Should_Create_Directory_If_Not_Exists()
        {
            // Arrange
            var nestedPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nested", "cassette.json");
            var cassette = new Cassette();

            try
            {
                // Act
                CassetteWriter.SaveToFile(cassette, nestedPath);

                // Assert
                Assert.True(File.Exists(nestedPath));
            }
            finally
            {
                // Cleanup
                if (File.Exists(nestedPath))
                {
                    var directory = Path.GetDirectoryName(nestedPath);
                    if (directory != null && Directory.Exists(directory))
                    {
                        Directory.Delete(directory, true);
                    }
                }
            }
        }
    }
}
