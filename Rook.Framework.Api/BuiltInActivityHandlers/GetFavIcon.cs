using Rook.Framework.Core.HttpServer;
using System.Net;

namespace Rook.Framework.Api.BuiltInActivityHandlers
{
	[ActivityHandler("GetApiFavicon", HttpVerb.Get, "favicon.ico", "Sends a NotFound response", SkipAuthorisation = true)]
	internal class GetFavIcon : IActivityHandler
    {
		public void Handle(IHttpRequest request, IHttpResponse response)
		{
			response.HttpContent = new EmptyHttpContent();
			response.HttpStatusCode = HttpStatusCode.NotFound;
		}

	    public dynamic ExampleRequestDocument { get; } = null;
	    public dynamic ExampleResponseDocument { get; } = null;
	}
}