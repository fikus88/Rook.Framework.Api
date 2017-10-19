using Microlise.MicroService.Core.Api.HttpServer;
using System.Net;

namespace Microlise.MicroService.Core.Api
{
    public interface IVerbHandler
    {
        void Handle(HttpRequest request, HttpResponse response);
    }
}