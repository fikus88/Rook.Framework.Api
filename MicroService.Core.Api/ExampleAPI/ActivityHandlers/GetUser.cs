using Microlise.MicroService.Core.Api;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.Example.ExampleAPI.ActivityHandlers
{
    [ActivityHandler("GetWidgetUser", HttpVerb.Get, "/user/{userId}")]
    internal class GetUser : IActivityHandler
    {
        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            response.SetObjectContent(new { userId = int.Parse(request.Parameters["userId"]), userName = request.Parameters["name"] });
        }
    }
}
    