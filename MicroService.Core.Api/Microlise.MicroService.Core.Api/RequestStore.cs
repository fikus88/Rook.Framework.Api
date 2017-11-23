using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microlise.MicroService.Core.Application.Bus;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.ResponseHandlers;
using System.Net;
using System.Threading.Tasks;
using Microlise.MicroService.Core.Api.HttpServer;
using Microlise.MicroService.Core.Api.Utils;
using Microlise.MicroService.Core.Data;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Microlise.MicroService.Core.Api
{
    public sealed class RequestStore : IRequestStore
    {
        private const int MaxTimeToWaitForBusResponseInMilliseconds = 5000;

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
            IRequestMatcher requestMatcher)
        {
            logger.Trace(nameof(RequestStore) + " constructor");
            this.queueWrapper = queueWrapper;
            this.logger = logger;
            this.mongo = mongo;
            this.requestMatcher = requestMatcher;
            this.dateTimeProvider = dateTimeProvider;
        }

        public Func<Guid> CreateUniqueId { get; set; } = Guid.NewGuid;
        public static List<string> Methods = new List<string>();

        public void PublishAndWaitForResponse<TNeed, TSolution>(Message<TNeed, TSolution> message, HttpStatusCode successResponseCode, IHttpResponse response)
        {
            Guid requestId = CreateUniqueId.Invoke();
            if (message.Uuid == Guid.Empty)
                message.Uuid = requestId;

            message.LastModifiedBy = ServiceInfo.Name;
            message.LastModifiedTime = dateTimeProvider.UtcNow;

            if (!Methods.Contains(message.Method))
                Methods.Add(message.Method);

            logger.Trace($"Operation=\"{nameof(RequestStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Preparing to publish message\" MessageId=\"{message.Uuid}\" MessageMethod=\"{message.Method}\"");

            using (DataWaitHandle dataWaitHandle = new DataWaitHandle(false, EventResetMode.AutoReset))
            {
                requestMatcher.RegisterWaitHandle(message.Uuid, dataWaitHandle);

                queueWrapper.PublishMessage(message);

                if (dataWaitHandle.WaitOne(MaxTimeToWaitForBusResponseInMilliseconds))
                {
                    List<ResponseError> errors = null;
                    if (dataWaitHandle.Errors != null)
                        errors = JsonConvert.DeserializeObject<List<ResponseError>>(dataWaitHandle.Errors);

                    logger.Trace($"Operation=\"{nameof(RequestStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received response\" MessageId=\"{message.Uuid}\" MessageMethod=\"{message.Method}\"");

                    if (errors != null && errors.Any())
                    {
                        response.SetStringContent(dataWaitHandle.Errors);

                        if (errors.Any(e => e.Type == ResponseError.ErrorType.Server))
                            response.HttpStatusCode = HttpStatusCode.InternalServerError;
                        else
                            response.HttpStatusCode = HttpStatusCode.BadRequest;
                        return;
                    }

                    response.SetStringContent(dataWaitHandle.Solution);
                    response.HttpStatusCode = successResponseCode;
                    return;
                }

                response.SetStringContent("Failed to get a response from the bus");
                logger.Trace($"Operation=\"{nameof(RequestStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received no response\" MessageId=\"{message.Uuid}\" MessageMethod=\"{message.Method}\" Message=\"{message}\"");
                response.HttpStatusCode = HttpStatusCode.RequestTimeout;
            }
        }

        private Task waitLoopTask;

        public void Start()
        {
            logger.Trace(nameof(RequestStore) + "." + nameof(Start));
            waitLoopTask = new Task(WaitLoop);
            waitLoopTask.Start();
        }

        private void WaitLoop()
        {
            logger.Trace(nameof(RequestStore) + "." + nameof(WaitLoop));
            logger.Trace(nameof(RequestStore) + "." + nameof(WaitLoop), new LogItem("Doing", "GetCappedCollection"));
            IMongoCollection<MessageWrapper> collection = mongo.GetCappedCollection<MessageWrapper>();
            logger.Trace(nameof(RequestStore) + "." + nameof(WaitLoop), new LogItem("Doing", "Create FindOptions"));
            FindOptions<MessageWrapper> options =
                new FindOptions<MessageWrapper> { CursorType = CursorType.TailableAwait };
            logger.Trace(nameof(RequestStore) + "." + nameof(WaitLoop), new LogItem("Doing", "Enter Loop"));

            while (true)
            {
                using (IAsyncCursor<MessageWrapper> cursor = collection.FindSync(mw => true, options))
                {
                    cursor.ForEachAsync(mw =>
                    {
                        requestMatcher.RegisterMessageWrapper(mw.Uuid, mw);
                    });
                }
            }

        }
    }

    
}