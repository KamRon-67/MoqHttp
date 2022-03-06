using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace MoqHttp.Interfaces
{
    public interface IHttpResponse
    {
        string Body { get; set; }
        Dictionary<string, string> Headers { get; set; }
        int StatusCode { get; set; }
        Action<HttpContext> Handler { get; set; }

    }
}
