using System.Net;
using Microlise.MicroService.Core.HttpServer;

namespace Microlise.MicroService.Core.Api.BuiltInActivityHandlers
{
    internal class OptionsActivityHandler : Core.HttpServer.IActivityHandler
    {
        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            response.HttpContent = new EmptyHttpContent();
            response.HttpStatusCode = HttpStatusCode.OK;
        }

        public dynamic ExampleRequestDocument { get; } = null;
        public dynamic ExampleResponseDocument { get; } = null;
    }
}