using Microlise.MicroService.Core.HttpServer;

namespace Microlise.MicroService.Core.Api.BuiltInActivityHandlers
{
    [ActivityHandler("GetApiHealth", HttpVerb.Get, "health", "Docker HealthCheck endpoint", SkipAuthorisation = true)]
    public class GetApiHealth : HealthActivityHandler
    {
        public GetApiHealth():base()            
        {
        }
    }
}
