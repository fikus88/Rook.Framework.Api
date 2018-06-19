using System;
using System.Collections.Generic;
using System.Net;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.ResponseHandlers;
using Microlise.MicroService.Core.HttpServer;

namespace Microlise.MicroService.Core.Api
{
    public interface IRequestStore
    {
        void PublishAndWaitForResponse<TNeed, TSolution>(Message<TNeed, TSolution> message, HttpStatusCode successResponseCode, IHttpResponse response,
            ResponseStyle responseStyle = ResponseStyle.WholeSolution, Func<string, bool> solutionMatchFunction = null);

        void SetResponse(HttpStatusCode successResponseCode, IHttpResponse response, string solution, List<ResponseError> errors);
    }
}
