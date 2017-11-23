using System.Net;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api.BuiltInVerbHandlers
{
	[ActivityHandler("GetApiFavicon", HttpVerb.Get, "favicon.ico", "Sends a NoContent response")]
	internal class GetFavIcon : IActivityHandler
	{
		public void Handle(IHttpRequest request, IHttpResponse response)
		{
			response.Content = null;
			response.HttpStatusCode = HttpStatusCode.NoContent;
		}
	}
}