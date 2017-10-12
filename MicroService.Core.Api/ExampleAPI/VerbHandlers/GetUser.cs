using System.Net;
using Microlise.MicroService.Core.Api;
using MicroService.Core.Api;
using MicroService.Core.Api.HttpServer;

namespace ExampleAPI.VerbHandlers
{
	[VerbHandler(HttpVerb.Get, "/user/{userId}")]
	internal class GetUser : IVerbHandler
	{
		public HttpStatusCode Handle(HttpRequest request, out object responseData)
		{
			responseData = new { userId = int.Parse(request.Parameters["userId"]), userName = request.Parameters["name"] };
			return HttpStatusCode.OK;
		}
	}
}
