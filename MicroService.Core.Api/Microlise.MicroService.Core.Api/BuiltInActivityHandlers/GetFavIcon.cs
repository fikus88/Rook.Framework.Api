using Microlise.MicroService.Core.HttpServer;
using System.Net;

namespace Microlise.MicroService.Core.Api.BuiltInActivityHandlers
{
	[ActivityHandler("GetApiFavicon", HttpVerb.Get, "favicon.ico", "Sends a NotFound response", SkipAuthorisation = true)]
	internal class GetFavIcon : Core.HttpServer.IActivityHandler
    {
		public void Handle(IHttpRequest request, IHttpResponse response)
		{
			response.Content = null;
			response.HttpStatusCode = HttpStatusCode.NotFound;
		}

	    public dynamic ExampleRequestDocument { get; } = null;
	    public dynamic ExampleResponseDocument { get; } = null;
	}
}