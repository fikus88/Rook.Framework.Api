using MicroService.Core.Api.HttpServer;
using System.Net;

namespace Microlise.MicroService.Core.Api
{
    public interface IRequestHandler
    {
        HttpStatusCode Handle(HttpRequest request, out object responseData);
    }
}