﻿using Microlise.MicroService.Core.Api.HttpServer;
using Microlise.MicroService.Core.Common;

namespace Microlise.MicroService.Core.Api.BuiltInActivityHandlers
{
    [ActivityHandler("GetApiVersion", HttpVerb.Get, "version", "Gets the version of the service", SkipAuthorisation = true)]
    internal class GetVersion : IActivityHandler
    {
        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            response.SetObjectContent(new { version = ServiceInfo.Version });
        }

        public dynamic ExampleRequestDocument { get; } = null;
        public dynamic ExampleResponseDocument { get; } = null;
    }
}
