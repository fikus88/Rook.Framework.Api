using System;
using System.Net;
using Microlise.MicroService.Core.Api.HttpServer;
using Microlise.MicroService.Core.Application.Message;

namespace Microlise.MicroService.Core.Api
{
    public interface IRequestStore
    {
        Func<Guid> CreateUniqueId { get; set; }
        //bool FindResponse(MessageWrapper messageWrapper);
        BusResponse PublishAndWaitForResponse<TNeed, TSolution>(Message<TNeed, TSolution> message, ResponseStyle responseStyle = ResponseStyle.WholeSolution, Func<string, bool> solutionMatchFunction = null);
        void PublishAndWaitForResponse<TNeed, TSolution>(Message<TNeed, TSolution> message, HttpStatusCode successResponseCode, IHttpResponse response, ResponseStyle responseStyle = ResponseStyle.WholeSolution, Func<string, bool> solutionMatchFunction = null);
        void Start();
    }
}
