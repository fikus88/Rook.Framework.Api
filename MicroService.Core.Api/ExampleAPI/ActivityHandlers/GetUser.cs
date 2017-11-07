using Microlise.MicroService.Core.Api;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.Example.ExampleAPI.VerbHandlers
{
    [ActivityHandler("GetWidgetUser", HttpVerb.Get, "/user/{userId}")]
    internal class GetUser : IActivityHandler
    {
        public void Handle(HttpRequest request, HttpResponse response)
        {
            response.SetObjectContent(new { userId = int.Parse(request.Parameters["userId"]), userName = request.Parameters["name"] });
        }
    }
}
