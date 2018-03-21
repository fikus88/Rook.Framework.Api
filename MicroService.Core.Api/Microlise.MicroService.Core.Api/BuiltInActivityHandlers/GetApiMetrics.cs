using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.HttpServer;
using Microlise.MicroService.Core.Monitoring;
using Prometheus.Advanced;

namespace Microlise.MicroService.Core.Api.BuiltInActivityHandlers
{
    [ActivityHandler("GetApiMetrics", HttpVerb.Get, "metrics", "Prometheus metrics endpoint", SkipAuthorisation = true)]
    public class GetApiMetrics : MetricsActivityHandler
    {
        public GetApiMetrics(ICollectorRegistry registry, ILogger logger) 
            : base(registry, logger)
        {
        }
    }
}
