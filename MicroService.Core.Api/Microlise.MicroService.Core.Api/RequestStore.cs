using System;
using System.Collections.Generic;
using System.Linq;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.ResponseHandlers;
using System.Net;
using Microlise.MicroService.Core.Api.HttpServer;
using Newtonsoft.Json;

namespace Microlise.MicroService.Core.Api
{
    public sealed class RequestStore : IRequestStore
    {
        private readonly ILogger logger;
        private readonly Core.IRequestStore coreRequestStore;

        public RequestStore(
            ILogger logger,
            Core.IRequestStore coreRequestStore)
        {
            logger.Trace($"{nameof(RequestStore)} constructor");
            this.logger = logger;
            this.coreRequestStore = coreRequestStore;
        }
        
        public void PublishAndWaitForResponse<TNeed, TSolution>(Message<TNeed, TSolution> message, HttpStatusCode successResponseCode, IHttpResponse response, ResponseStyle responseStyle = ResponseStyle.WholeSolution, Func<string, bool> solutionMatchFunction = null)
        {
            BusResponse busResponse = coreRequestStore.PublishAndWaitForResponse(message, responseStyle, solutionMatchFunction);

            if (string.IsNullOrWhiteSpace(busResponse.Solution) && string.IsNullOrWhiteSpace(busResponse.Errors))
            {
                response.SetStringContent("Failed to get a response from the bus");
                logger.Debug(
                    $"Operation=\"{nameof(RequestStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received no response\" MessageId=\"{message.Uuid}\" MessageMethod=\"{message.Method}\" Message=\"{message}\"");
                response.HttpStatusCode = HttpStatusCode.RequestTimeout;
                return;
            }

            List<ResponseError> errors = null;
            if (!string.IsNullOrWhiteSpace(busResponse.Errors))
                errors = JsonConvert.DeserializeObject<List<ResponseError>>(busResponse.Errors);

            logger.Debug(
                $"Operation=\"{nameof(RequestStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received response\" MessageId=\"{message.Uuid}\" MessageMethod=\"{message.Method}\"");

            if (errors != null && errors.Any())
            {
                response.SetStringContent(busResponse.Errors);

                if (errors.Any(e => e.Type == ResponseError.ErrorType.Server))
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;
                else
                    response.HttpStatusCode = HttpStatusCode.BadRequest;
                return;
            }

            response.SetStringContent(busResponse.Solution);
            response.HttpStatusCode = successResponseCode;
        }
    }
}