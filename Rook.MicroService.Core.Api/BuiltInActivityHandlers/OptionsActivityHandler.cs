using System.Net;
using Rook.Framework.Core.HttpServer;

namespace Rook.Framework.Core.Api.BuiltInActivityHandlers
{
    internal class OptionsActivityHandler : IActivityHandler
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