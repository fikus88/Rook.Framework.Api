﻿using Microlise.MicroService.Core.Services;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api.BuiltInVerbHandlers
{
    [ActivityHandler("GetApiVersion", HttpVerb.Get, "version", "Gets the version of the service")]
    internal class GetVersion : IActivityHandler
    {
        public void Handle(HttpRequest request, HttpResponse response)
        {
            response.SetObjectContent(new { version = Service.GetServiceVersion() });
        }
    }
}
