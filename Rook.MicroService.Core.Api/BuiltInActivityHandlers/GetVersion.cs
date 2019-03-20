using Rook.Framework.Core.Common;
using Rook.Framework.Core.HttpServer;

namespace Rook.Framework.Core.Api.BuiltInActivityHandlers
{
    [ActivityHandler("GetApiVersion", HttpVerb.Get, "version", "Gets the version of the service", SkipAuthorisation = true)]
    internal class GetVersion : IActivityHandler
    {
        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            response.SetObjectContent(new { version = ServiceInfo.Version });
        }

        public dynamic ExampleRequestDocument { get; } = null;
        public dynamic ExampleResponseDocument { get; } = null;
    }
}
