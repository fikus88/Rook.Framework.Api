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
        private readonly IDateTimeProvider dateTimeProvider;

        public RequestStore(
            IDateTimeProvider dateTimeProvider,
            IQueueWrapper queueWrapper,
            ILogger logger,
            IMongoStore mongo)
        {
            logger.Trace(nameof(RequestStore) + " constructor");
            this.queueWrapper = queueWrapper;
            this.logger = logger;
            this.mongo = mongo;
            this.dateTimeProvider = dateTimeProvider;
        }

        public static readonly ConcurrentDictionary<Guid, DataWaitHandle> RequestsStore = new ConcurrentDictionary<Guid, DataWaitHandle>();

        public Func<Guid> CreateUniqueId { get; set; } = Guid.NewGuid;
        public static List<string> Methods = new List<string>();

        public void PublishAndWaitForResponse<TNeed, TSolution>(Message<TNeed, TSolution> message, HttpStatusCode successResponseCode, IHttpResponse response)
        {
            Guid requestId = CreateUniqueId.Invoke();

            message.Uuid = requestId;
            message.LastModifiedBy = ServiceInfo.Name;
            message.LastModifiedTime = dateTimeProvider.UtcNow;

            if (!Methods.Contains(message.Method))
                Methods.Add(message.Method);

            logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Preparing to publish message\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\"");

            using (var dataWaitHandle = new DataWaitHandle(false, EventResetMode.AutoReset))
            {
                if (RequestsStore.TryAdd(requestId, dataWaitHandle))
                {
                    queueWrapper.PublishMessage(message);

                    if (dataWaitHandle.WaitOne(MaxTimeToWaitForBusResponseInMilliseconds))
                    {
                        RequestsStore.TryRemove(requestId, out _);

                        var completedMessage = message;
                        message.Solution = JsonConvert.DeserializeObject<TSolution[]>(dataWaitHandle.Data);

                        logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received response\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\" Message=\"{completedMessage}\"");

                        if (completedMessage.Errors.Any())
                        {
                            response.SetObjectContent(completedMessage.Errors);

                            if (completedMessage.Errors.Any(e => e.Type == ResponseError.ErrorType.Server))
                                response.HttpStatusCode = HttpStatusCode.InternalServerError;
                            else
                                response.HttpStatusCode = HttpStatusCode.BadRequest;
                            return;
                        }

                        response.SetStringContent(dataWaitHandle.Data);
                        response.HttpStatusCode = successResponseCode;
                        return;
                    }

                    RequestsStore.TryRemove(requestId, out _);

                    response.SetStringContent("Failed to get a response from the bus");
                    logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received no response\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\" Message=\"{message}\"");
                    response.HttpStatusCode = HttpStatusCode.RequestTimeout;
                    return;
                }
            }

            response.SetStringContent("Failed to publish request to the bus");
            logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Failed to publish message\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\" Message=\"{message}\"");
            response.HttpStatusCode = HttpStatusCode.InternalServerError;
        }

        public bool FindResponse(MessageWrapper messageWrapper)
        {
            logger.Trace(nameof(RequestStore) + "." + nameof(FindResponse));
            bool idFound = RequestsStore.TryGetValue(messageWrapper.Uuid, out DataWaitHandle dataWaitHandle);

            if (idFound)
            {
                logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(FindResponse)}\" Event=\"Found Id in requests store\" MessageId=\"{messageWrapper.Uuid}\" Data=\"{messageWrapper.SolutionJson}\"");
                dataWaitHandle.Set(messageWrapper.SolutionJson);
            }
            else
            {
                logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(FindResponse)}\" Event=\"Could not find Id in requests store\" MessageId=\"{messageWrapper.Uuid}\" Data=\"{messageWrapper.SolutionJson}\"");
            }

            return idFound;
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
                        logger.Trace(nameof(RequestStore) + "." + nameof(WaitLoop),
                            new LogItem("Action", "Item added to the capped collection"));
                        if (FindResponse(mw))
                        {
                            logger.Trace(nameof(RequestStore) + "." + nameof(WaitLoop),
                                new LogItem("Action", "Item removed from the capped collection"));
                            mongo.RemoveEntity(mw);
                        }
                    });
                }
            }

        }
    }
}