using System.Net;
using Microlise.MicroService.Core.Services;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api.BuiltInVerbHandlers
{
	[VerbHandler(HttpVerb.Get, "version", "Gets the version of the service")]
	internal class GetVersion : IVerbHandler
	{
		public void Handle(HttpRequest request, HttpResponse response)
		{
			response.SetObjectContent(new { version = Service.GetServiceVersion() });			
		}
	}
}
