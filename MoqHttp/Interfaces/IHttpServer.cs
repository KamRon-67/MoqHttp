using System;

namespace MoqHttp.Interfaces
{
    public interface IHttpServer : IDisposable
    {
        void Run();
        IRequestBuilder Config { get;  }
    }
}
