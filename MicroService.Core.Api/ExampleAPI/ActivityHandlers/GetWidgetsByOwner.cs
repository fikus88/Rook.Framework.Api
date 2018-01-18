using System;
using Microlise.MicroService.Core.Api;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.Example.ExampleAPI.ActivityHandlers {
    [ActivityHandler("GetWidgetsByOwner", HttpVerb.Get, "/widget/owner/{ownerId}")]
    internal class GetWidgetsByOwner : IActivityHandler
    {
        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            response.SetObjectContent(new[]{
                new{id = Guid.NewGuid(), name = "NewWidget123", owner = request.Parameters["ownerId"], version = "1.43.2.5", width = 145.2, weight = 10},
                new{id = Guid.NewGuid(), name = "OldWidget2", owner = request.Parameters["ownerId"], version ="1.0.0.2", width = 15.7, weight = 3}
            });
        }

        public dynamic ExampleRequestDocument { get; } = null;

        public dynamic ExampleResponseDocument { get; } = new[]{
            new{id = Guid.NewGuid(), name = "NewWidget123", owner = Guid.Parse("7ea1ceba-2608-4caa-8fec-906f51d49968"), version = "1.43.2.5", width = 145.2, weight = 10},
            new{id = Guid.NewGuid(), name = "OldWidget2", owner = Guid.Parse("7ea1ceba-2608-4caa-8fec-906f51d49968"), version ="1.0.0.2", width = 15.7, weight = 3}
        };
    }
}