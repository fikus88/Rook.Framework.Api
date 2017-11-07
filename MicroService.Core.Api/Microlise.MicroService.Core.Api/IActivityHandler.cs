using Microlise.MicroService.Core.Api.HttpServer;
using System;

namespace Microlise.MicroService.Core.Api
{
    [Obsolete("Use IActivityHandler", true)]
    public interface IVerbHandler : IActivityHandler { }
    public interface IActivityHandler
    {
        void Handle(HttpRequest request, HttpResponse response);
    }
}