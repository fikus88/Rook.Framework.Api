using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Framework.Core.Common;
using Rook.Framework.Core.Application.Message;
using Rook.Framework.Core.Application.ResponseHandlers;
using System.Net;
using Rook.Framework.Core;
using Newtonsoft.Json;
using Rook.Framework.Core.HttpServer;

namespace Rook.Framework.Api
{
    public sealed class RequestStore : IRequestStore
    {
        private readonly ILogger logger;
        private readonly Rook.Framework.Core.IRequestStore coreRequestStore;

        public RequestStore(
            ILogger logger,
            Rook.Framework.Core.IRequestStore coreRequestStore)
        {
            logger.Trace($"{nameof(RequestStore)} constructor");
            this.logger = logger;
            this.coreRequestStore = coreRequestStore;
        }
        
        public void PublishAndWaitForResponse<TNeed, TSolution>(Message<TNeed, TSolution> message, HttpStatusCode successResponseCode, IHttpResponse response, ResponseStyle responseStyle = ResponseStyle.WholeSolution, Func<string, bool> solutionMatchFunction = null)
        {
            var busResponse = coreRequestStore.PublishAndWaitForResponse(message, responseStyle, solutionMatchFunction);
            if (string.IsNullOrWhiteSpace(busResponse.Solution) && string.IsNullOrWhiteSpace(busResponse.Errors))
            {
                response.SetStringContent("Failed to get a response from the bus");
                logger.Debug(
                    $"Operation=\"{nameof(RequestStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received no response\" MessageId=\"{message.Uuid}\" MessageMethod=\"{message.Method}\" Message=\"{message}\"");
                response.HttpStatusCode = HttpStatusCode.RequestTimeout;
                return;
            }

            logger.Debug(
                $"Operation=\"{nameof(RequestStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received response\" MessageId=\"{message.Uuid}\" MessageMethod=\"{message.Method}\"");

            List<ResponseError> errors = null;
            if (!string.IsNullOrWhiteSpace(busResponse.Errors))
                errors = JsonConvert.DeserializeObject<List<ResponseError>>(busResponse.Errors);

            SetResponse(successResponseCode, response, busResponse.Solution, errors);
        }

        public void SetResponse(HttpStatusCode successResponseCode, IHttpResponse response, string solution, List<ResponseError> errors)
        {
            if (errors != null && errors.Any())
            {
                response.SetStringContent(JsonConvert.SerializeObject(errors));

                response.HttpStatusCode = errors.Any(e => e.Type == ResponseError.ErrorType.Server)
                    ? HttpStatusCode.InternalServerError
                    : HttpStatusCode.BadRequest;

                return;
            }

            if (successResponseCode == HttpStatusCode.NoContent) //the request has been successfully processed and the response is intentionally blank.
            {
                solution = string.Empty;
            }

            response.SetStringContent(solution);
            response.HttpStatusCode = successResponseCode;
        }
    }
}