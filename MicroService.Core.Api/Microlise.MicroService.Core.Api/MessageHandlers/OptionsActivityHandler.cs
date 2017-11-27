using System.Net;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api.MessageHandlers {
    internal class OptionsActivityHandler : IActivityHandler
    {
        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            response.Content = null;
            response.HttpStatusCode = HttpStatusCode.OK;
        }
    }
}