using System.Net;

namespace Rook.Framework.Api
{
    public interface IApiMetrics
    {        
        void RecordHandlerDuration(double elapsedMilliseconds, string handlerName, HttpStatusCode httpStatusCode);
    }
}
