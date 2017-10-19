using System.Net;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api.BuiltInVerbHandlers
{
	[VerbHandler(HttpVerb.Get, "favicon.ico", "Sends a NoContent response")]
	internal class GetFavIcon : IVerbHandler
	{
		public void Handle(HttpRequest request, HttpResponse response)
		{
			response.Content = null;
			response.HttpStatusCode = HttpStatusCode.NoContent;
		}
	}
}