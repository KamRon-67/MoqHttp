using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using MoqHttp.Interfaces;

namespace MoqHttp.Models
{
    public class RequestHandler : IRequestHandler, IJsonRequestHandler
    {
        public RouteTableItem RouteTable { get; }
        public HttpResponse Response { get; private set; }
        public JsonElement JsonObject { get; set; }

        public RequestHandler(RouteTableItem route)
        {
            RouteTable = route;
            Response = new HttpResponse();
        }

        public void Send(string body)
        {
            Send(body, 200, null);
        }

        public void Send(string body, int statusCode)
        {
            Send(body, statusCode, null);
        }

        public void Send(Action<HttpContext> context)
        {
            Send("", 200, null, context);
        }

        public void Send(string body, int statusCode, Dictionary<string, string>? headers)
        {
            Send(body, statusCode, headers, null);
        }

        public void Send (string body, int statusCode, Dictionary<string, string>? headers, Action<HttpContext>? context)
        {
            Response = new HttpResponse() { Body = body, StatusCode = statusCode, Headers = headers, Handler = context };
            RouteTable.Response = Response;
        }

        public void ReadJSONFromFile(string path)
        {
            // read JSON directly from a file
            var jsonString = File.ReadAllText(path);
            using (var doc = JsonDocument.Parse(jsonString))
            {
                JsonObject = doc.RootElement.Clone();
            }

            Send(JsonObject.GetRawText(), 200, null);
        }
    }
}

