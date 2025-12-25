using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using MoqHttp.VCR.Models;
using MoqHttp.VCR.Playback;
using Xunit;

namespace MoqHttpTests.VCR
{
    public class RequestMatcherTests
    {
        [Fact]
        public void FindMatch_Should_Match_By_Method_And_Path()
        {
            // Arrange
            var interactions = new List<Interaction>
            {
                new Interaction
                {
                    Request = new RecordedRequest
                    {
                        Method = "GET",
                        Uri = "/api/users"
                    },
                    Response = new RecordedResponse
                    {
                        Status = 200,
                        Body = "[]"
                    }
                },
                new Interaction
                {
                    Request = new RecordedRequest
                    {
                        Method = "POST",
                        Uri = "/api/users"
                    },
                    Response = new RecordedResponse
                    {
                        Status = 201,
                        Body = "{\"id\": 1}"
                    }
                }
            };

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/users";

            // Act
            var match = RequestMatcher.FindMatch(context.Request, interactions);

            // Assert
            Assert.NotNull(match);
            Assert.Equal(200, match.Response.Status);
        }

        [Fact]
        public void FindMatch_Should_Match_POST_Request()
        {
            // Arrange
            var interactions = new List<Interaction>
            {
                new Interaction
                {
                    Request = new RecordedRequest
                    {
                        Method = "GET",
                        Uri = "/api/data"
                    },
                    Response = new RecordedResponse
                    {
                        Status = 200
                    }
                },
                new Interaction
                {
                    Request = new RecordedRequest
                    {
                        Method = "POST",
                        Uri = "/api/data"
                    },
                    Response = new RecordedResponse
                    {
                        Status = 201
                    }
                }
            };

            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Path = "/api/data";

            // Act
            var match = RequestMatcher.FindMatch(context.Request, interactions);

            // Assert
            Assert.NotNull(match);
            Assert.Equal(201, match.Response.Status);
        }

        [Fact]
        public void FindMatch_Should_Return_Null_When_No_Match()
        {
            // Arrange
            var interactions = new List<Interaction>
            {
                new Interaction
                {
                    Request = new RecordedRequest
                    {
                        Method = "GET",
                        Uri = "/api/users"
                    },
                    Response = new RecordedResponse
                    {
                        Status = 200
                    }
                }
            };

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/posts";

            // Act
            var match = RequestMatcher.FindMatch(context.Request, interactions);

            // Assert
            Assert.Null(match);
        }

        [Fact]
        public void FindMatch_Should_Match_With_Query_String()
        {
            // Arrange
            var interactions = new List<Interaction>
            {
                new Interaction
                {
                    Request = new RecordedRequest
                    {
                        Method = "GET",
                        Uri = "/api/search?q=test"
                    },
                    Response = new RecordedResponse
                    {
                        Status = 200,
                        Body = "{\"results\": []}"
                    }
                }
            };

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/search";
            context.Request.QueryString = new QueryString("?q=test");

            // Act
            var match = RequestMatcher.FindMatch(context.Request, interactions);

            // Assert
            Assert.NotNull(match);
            Assert.Equal(200, match.Response.Status);
        }

        [Fact]
        public void FindMatch_Should_Handle_Full_URI_In_Recording()
        {
            // Arrange
            var interactions = new List<Interaction>
            {
                new Interaction
                {
                    Request = new RecordedRequest
                    {
                        Method = "GET",
                        Uri = "https://api.example.com/v1/data"
                    },
                    Response = new RecordedResponse
                    {
                        Status = 200,
                        Body = "{}"
                    }
                }
            };

            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/v1/data";

            // Act
            var match = RequestMatcher.FindMatch(context.Request, interactions);

            // Assert
            Assert.NotNull(match);
            Assert.Equal(200, match.Response.Status);
        }

        [Fact]
        public void FindMatch_Should_Return_Null_For_Empty_Interactions()
        {
            // Arrange
            var interactions = new List<Interaction>();
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/test";

            // Act
            var match = RequestMatcher.FindMatch(context.Request, interactions);

            // Assert
            Assert.Null(match);
        }
    }
}
