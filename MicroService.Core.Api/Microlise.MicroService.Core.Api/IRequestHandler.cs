using Microlise.MicroService.Core.Api.HttpServer;
using System.Net;

namespace Microlise.MicroService.Core.Api
{
    public interface IVerbHandler
    {
        HttpStatusCode Handle(HttpRequest request, out object responseData);
    }
}