﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microlise.MicroService.Core.Application.Bus;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.ResponseHandlers;
using System.Net;
using Microlise.MicroService.Core.Api.HttpServer;
using Microlise.MicroService.Core.Api.Utils;
using Microlise.MicroService.Core.Data;
using Newtonsoft.Json;

namespace Microlise.MicroService.Core.Api
{
    public sealed class RequestStore : IRequestStore
    {
        private readonly int busTimeoutMilliseconds;

        private readonly IQueueWrapper queueWrapper;

        private readonly ILogger logger;
        private readonly IMongoStore mongo;
        private readonly IRequestMatcher requestMatcher;
        private readonly IDateTimeProvider dateTimeProvider;

        public RequestStore(
            IDateTimeProvider dateTimeProvider,
            IQueueWrapper queueWrapper,
            ILogger logger,
            IMongoStore mongo,
            IRequestMatcher requestMatcher,
            IConfigurationManager config)
        {
            logger.Trace($"{nameof(RequestStore)} constructor");
            this.queueWrapper = queueWrapper;
            this.logger = logger;
            this.mongo = mongo;
            this.requestMatcher = requestMatcher;
            this.dateTimeProvider = dateTimeProvider;
            if (!int.TryParse(config.AppSettings["BusTimeoutMilliseconds"],
                out busTimeoutMilliseconds))
            {
                busTimeoutMilliseconds = 5000;
            }
        }

        public Func<Guid> CreateUniqueId { get; set; } = Guid.NewGuid;
        public static List<string> Methods = new List<string>();

        public BusResponse PublishAndWaitForResponse<TNeed, TSolution>(Message<TNeed, TSolution> message, ResponseStyle responseStyle = ResponseStyle.WholeSolution, Func<string, bool> solutionMatchFunction = null)
        {
            Guid requestId = CreateUniqueId.Invoke();
            if (message.Uuid == Guid.Empty)
                message.Uuid = requestId;

            message.LastModifiedBy = ServiceInfo.Name;
            message.LastModifiedTime = dateTimeProvider.UtcNow;

            if (!Methods.Contains(message.Method))
                Methods.Add(message.Method);

            logger.Trace($"Operation=\"{nameof(RequestStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Preparing to publish message\" MessageId=\"{message.Uuid}\" MessageMethod=\"{message.Method}\"");

            BusResponse response = new BusResponse();

            using (DataWaitHandle dataWaitHandle = new DataWaitHandle(false, EventResetMode.AutoReset, solutionMatchFunction))
            {
                requestMatcher.RegisterWaitHandle(message.Uuid, dataWaitHandle, responseStyle);

                queueWrapper.PublishMessage(message);

                if (dataWaitHandle.WaitOne(busTimeoutMilliseconds))
                {
                    List<ResponseError> errors = null;
                    if (dataWaitHandle.Errors != null)
                        errors = JsonConvert.DeserializeObject<List<ResponseError>>(dataWaitHandle.Errors);

                    logger.Trace($"Operation=\"{nameof(RequestStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received response\" MessageId=\"{message.Uuid}\" MessageMethod=\"{message.Method}\"");

                    if (errors != null && errors.Any())
                        response.Errors = dataWaitHandle.Errors;

                    response.Solution = dataWaitHandle.Solution;
                }

                if (string.IsNullOrEmpty(response.Errors) && string.IsNullOrEmpty(response.Solution))
                    logger.Trace($"Operation=\"{nameof(RequestStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received no response\" MessageId=\"{message.Uuid}\" MessageMethod=\"{message.Method}\" Message=\"{message}\"");

                return response;
            }
        }

        public void PublishAndWaitForResponse<TNeed, TSolution>(Message<TNeed, TSolution> message, HttpStatusCode successResponseCode, IHttpResponse response, ResponseStyle responseStyle = ResponseStyle.WholeSolution, Func<string, bool> solutionMatchFunction = null)
        {
            BusResponse busResponse = PublishAndWaitForResponse(message, responseStyle, solutionMatchFunction);

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

        private Timer pollTimer;
        private ulong lastId;

        public void Start()
        {
            logger.Debug($"{nameof(RequestStore)}.{nameof(Start)}");
            pollTimer = new Timer(PollForResponses, null, TimeSpan.FromMilliseconds(50), TimeSpan.FromMinutes(1));
        }

        private void PollForResponses(object state)
        {
            try
            {
                foreach (MessageWrapper mw in mongo.Get<MessageWrapper>(mw => mw.Sequence > lastId))
                {
                    requestMatcher.RegisterMessageWrapper(mw.Uuid, mw);
                    lastId = mw.Sequence;
                    logger.Trace($"{nameof(RequestStore)}.{nameof(PollForResponses)}", new LogItem("LastId", lastId));
                }
            }
            catch (Exception ex)
            {
                logger.Error($"{nameof(RequestStore)}.{nameof(PollForResponses)}",
                    new LogItem("ExceptionMessage", ex.Message), new LogItem("Exception", ex.ToString));
            }
            finally
            {
                pollTimer.Change(TimeSpan.FromMilliseconds(10), TimeSpan.FromMinutes(1));
            }
        }
    }
    public class BusResponse
    {
        public string Errors { get; set; }
        public string Solution { get; set; }
    }
}