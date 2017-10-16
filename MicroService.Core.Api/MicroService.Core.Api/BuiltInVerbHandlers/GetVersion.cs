using System.Collections;
using System.Net;
using Microlise.MicroService.Core.Api;
using Microlise.MicroService.Core.Services;
using MicroService.Core.Api.HttpServer;

namespace MicroService.Core.Api.BuiltInVerbHandlers
{
	[VerbHandler(HttpVerb.Get, "version", "Gets the version of the service")]
	internal class GetVersion : IVerbHandler
	{
		public HttpStatusCode Handle(HttpRequest request, out object responseData)
		{
			responseData = new { version = Service.GetServiceVersion() };
			return HttpStatusCode.OK;
		}
	}
}
