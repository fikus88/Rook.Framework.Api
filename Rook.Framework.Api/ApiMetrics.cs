using Prometheus;
using System.Net;

namespace Rook.Framework.Api
{
    public class ApiMetrics : IApiMetrics
    {
        private const string MetricsPrefix = "rook_api";

        // Prometheus standard unit of time is seconds: https://prometheus.io/docs/practices/naming/#base-units
        private readonly Histogram _handlerDurationHistogram = Metrics.CreateHistogram(
               $"{MetricsPrefix}_activity_handler_duration_seconds",
               "Time taken to process the IActivityHandler.Handle method",
               labelNames: new[] { "handler", "status_code" }); 

        public void RecordHandlerDuration(double elapsedMilliseconds, string handlerName, HttpStatusCode httpStatusCode)
        {
            _handlerDurationHistogram.Labels(handlerName, ((int)httpStatusCode).ToString()).Observe(elapsedMilliseconds / 1000);
        }
    }
}
