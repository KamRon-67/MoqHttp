using Microsoft.AspNetCore.Http;
using MoqHttp.Models;
using HttpResponse = MoqHttp.Models.HttpResponse;

namespace MoqHttp.Interfaces
{
    public interface IRequestHandler : IJsonRequestHandler
    {
        RouteTableItem RouteTable { get; }
        HttpResponse Response { get; }
        void Send(string body);
        void Send(string body, int statusCode);
        void Send(string body, int statusCode, Dictionary<string, string> headers);
        void Send(Action<HttpContext> context);
    }
}
