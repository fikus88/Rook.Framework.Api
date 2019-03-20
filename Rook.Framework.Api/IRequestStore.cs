using System;
using System.Collections.Generic;
using System.Net;
using Rook.Framework.Core;
using Rook.Framework.Core.Application.Message;
using Rook.Framework.Core.Application.ResponseHandlers;
using Rook.Framework.Core.HttpServer;

namespace Rook.Framework.Api
{
    public interface IRequestStore
    {
        void PublishAndWaitForResponse<TNeed, TSolution>(Message<TNeed, TSolution> message, HttpStatusCode successResponseCode, IHttpResponse response,
            ResponseStyle responseStyle = ResponseStyle.WholeSolution, Func<string, bool> solutionMatchFunction = null);

        void SetResponse(HttpStatusCode successResponseCode, IHttpResponse response, string solution, List<ResponseError> errors);
    }
}
