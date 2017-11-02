using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microlise.MicroService.Core.Application.Bus;
using Microlise.MicroService.Core.Common;
using Newtonsoft.Json.Linq;
using Microlise.MicroService.Core.Services;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.ResponseHandlers;
using System.Net;
using System.Threading.Tasks;
using Microlise.MicroService.Core.Api.HttpServer;
using Microlise.MicroService.Core.Data;
using MongoDB.Driver;

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
			this.queueWrapper = queueWrapper;
			this.logger = logger;
			this.mongo = mongo;
			this.dateTimeProvider = dateTimeProvider;
		}

		public static readonly ConcurrentDictionary<Guid, DataWaitHandle<object>> RequestsStore = new ConcurrentDictionary<Guid, DataWaitHandle<object>>();

		public Func<Guid> CreateUniqueId { get; set; } = Guid.NewGuid;

		public void PublishAndWaitForResponse(dynamic message, HttpStatusCode successResponseCode, HttpResponse response)
		{
			Guid requestId = CreateUniqueId.Invoke();

			message.Uuid = requestId;
			message.LastModifiedBy = Service.GetServiceName();
			message.LastModifiedTime = dateTimeProvider.UtcNow;

			logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Preparing to publish message\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\"");

			MessageWrapper placeholder = new MessageWrapper { Uuid = message.Uuid };
			mongo.Put(placeholder);

			using (var dataWaitHandle = new DataWaitHandle<object>(false, EventResetMode.AutoReset))
			{
				if (RequestsStore.TryAdd(requestId, dataWaitHandle))
				{
					queueWrapper.PublishMessage(message);

					DataWaitHandle<object> dataWaitHandleFromRemove;

					if (dataWaitHandle.WaitOne(MaxTimeToWaitForBusResponseInMilliseconds))
					{
						RequestsStore.TryRemove(requestId, out dataWaitHandleFromRemove);

						var completedMessage = ((JObject)dataWaitHandle.Data).ToObject<Message<object, object>>();

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

						response.SetObjectContent(completedMessage.Solution);
						response.HttpStatusCode = successResponseCode;
						return;
					}

					RequestsStore.TryRemove(requestId, out dataWaitHandleFromRemove);

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
			bool idFound = RequestsStore.TryGetValue(messageWrapper.Uuid, out DataWaitHandle<object> dataWaitHandle);

			if (idFound)
			{
				logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(FindResponse)}\" Event=\"Found Id in requests store\" MessageId=\"{messageWrapper.Uuid}\" Data=\"{messageWrapper.Message}\"");
				dataWaitHandle.Set(messageWrapper.Message);
			}
			else
			{
				logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(FindResponse)}\" Event=\"Could not find Id in requests store\" MessageId=\"{messageWrapper.Uuid}\" Data=\"{messageWrapper.Message}\"");
			}

			return idFound;
		}

		private Task waitLoopTask;

		public void Start()
		{
			waitLoopTask = new Task(WaitLoop);
			waitLoopTask.Start();
		}

		private void WaitLoop()
		{
			IMongoCollection<MessageWrapper> collection = mongo.GetCappedCollection<MessageWrapper>();
			FindOptions<MessageWrapper> options = new FindOptions<MessageWrapper> { CursorType = CursorType.TailableAwait };

			while (true)
			{
				using (IAsyncCursor<MessageWrapper> cursor = collection.FindSync(mw => mw.Message != null, options))
				{
					cursor.ForEachAsync(mw =>
					{
						if (FindResponse(mw))
							mongo.RemoveEntity(mw);
					});
				}
			}
		}
	}
}