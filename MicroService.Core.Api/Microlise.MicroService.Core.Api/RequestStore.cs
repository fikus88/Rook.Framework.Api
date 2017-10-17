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
using Microlise.MicroService.Core.Data;
using Microlise.Microservice.Core.Api.MessageHandlers;
using MongoDB.Driver;

namespace Microlise.MicroService.Core.Api
{
	public sealed class RequestStore : IRequestStore
	{
		private const int MaxTimeToWaitForBusResponseInMilliseconds = 5000;

		private readonly IQueueWrapper _queueWrapper;

		private readonly ILogger _logger;
		private readonly IMongoStore mongo;
		private readonly IDateTimeProvider _dateTimeProvider;

		public RequestStore(
			IDateTimeProvider dateTimeProvider,
			IQueueWrapper queueWrapper,
			ILogger logger,
			IMongoStore mongo)
		{
			_queueWrapper = queueWrapper;
			_logger = logger;
			this.mongo = mongo;
			_dateTimeProvider = dateTimeProvider;
		}

		public static readonly ConcurrentDictionary<Guid, DataWaitHandle<object>> RequestsStore = new ConcurrentDictionary<Guid, DataWaitHandle<object>>();

		public Func<Guid> CreateUniqueId { get; set; } = Guid.NewGuid;

		public HttpStatusCode PublishAndWaitForResponse(dynamic message, HttpStatusCode successResponseCode, out object responseData)
		{
			var requestId = CreateUniqueId.Invoke();

			message.Uuid = requestId;
			message.LastModifiedBy = Service.GetServiceName();
			message.LastModifiedTime = _dateTimeProvider.UtcNow;

			_logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Preparing to publish message\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\"");

			MessageWrapper placeholder = new MessageWrapper { Uuid = message.Uuid };
			mongo.Put(placeholder);

			using (var dataWaitHandle = new DataWaitHandle<object>(false, EventResetMode.AutoReset))
			{
				if (RequestsStore.TryAdd(requestId, dataWaitHandle))
				{
					_queueWrapper.PublishMessage(message);

					DataWaitHandle<object> dataWaitHandleFromRemove;

					if (dataWaitHandle.WaitOne(MaxTimeToWaitForBusResponseInMilliseconds))
					{
						RequestsStore.TryRemove(requestId, out dataWaitHandleFromRemove);

						var completedMessage = ((JObject)dataWaitHandle.Data).ToObject<Message<object, object>>();

						_logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received response\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\" Message=\"{completedMessage}\"");

						if (completedMessage.Errors.Any())
						{
							responseData = completedMessage.Errors;

							if (completedMessage.Errors.Any(e => e.Type == ResponseError.ErrorType.Server))
								return HttpStatusCode.InternalServerError;

							return HttpStatusCode.BadRequest;
						}

						responseData = completedMessage.Solution;
						return successResponseCode;
					}

					RequestsStore.TryRemove(requestId, out dataWaitHandleFromRemove);

					responseData = "Failed to get a response from the bus";
					_logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received no response\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\" Message=\"{message}\"");
					return HttpStatusCode.RequestTimeout;
				}
			}

			responseData = "Failed to publish request to the bus";
			_logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Failed to publish message\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\" Message=\"{message}\"");
			return HttpStatusCode.InternalServerError;
		}

		private Task waitLoopTask;

		public void Start()
		{
			waitLoopTask = new Task(WaitLoop);
			waitLoopTask.Start();
		}

		public bool FindResponse(Guid requestId, object data)
		{
			bool idFound = RequestsStore.TryGetValue(requestId, out DataWaitHandle<object> dataWaitHandle);

			if (idFound)
			{
				_logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(FindResponse)}\" Event=\"Found Id in requests store\" MessageId=\"{requestId}\" Data=\"{data}\"");
				dataWaitHandle.Set(data);
			}
			else
			{
				_logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(FindResponse)}\" Event=\"Could not find Id in requests store\" MessageId=\"{requestId}\" Data=\"{data}\"");
			}

			return idFound;
		}

		public void WaitLoop()
		{
			IMongoCollection<MessageWrapper> collection = mongo.GetCollection<MessageWrapper>();
			FindOptions<MessageWrapper> options = new FindOptions<MessageWrapper> { CursorType = CursorType.TailableAwait };

			while (true)
			{
				using (IAsyncCursor<MessageWrapper> cursor = collection.FindSync(mw => mw.Message != null, options))
				{
					cursor.ForEachAsync(mw =>
					{
						Guid uuid = mw.Uuid;
						if (FindResponse(uuid, mw.Message))
							mongo.Remove(mw);
					});
				}
			}
		}

	}
}