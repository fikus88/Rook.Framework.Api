using System.Net;

namespace Rook.Framework.Core.Api
{
    public interface IApiMetrics
    {        
        void RecordHandlerDuration(double elapsedMilliseconds, string handlerName, HttpStatusCode httpStatusCode);
    }
}
