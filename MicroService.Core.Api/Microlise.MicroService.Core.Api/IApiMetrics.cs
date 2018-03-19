using System.Net;

namespace Microlise.MicroService.Core.Api
{
    public interface IApiMetrics
    {        
        void RecordHandlerDuration(double elapsedMilliseconds, string handlerName, HttpStatusCode httpStatusCode);
    }
}
