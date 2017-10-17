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

namespace Microlise.MicroService.Core.Api
{
	public sealed class RequestStore : IRequestStore
	{
	    public const string CreateCallbackMessageMethod = "CreateCallback";

        private const int MaxTimeToWaitForBusResponseInMilliseconds = 5000;

	    private readonly string _modulePath;
	    private readonly string _responsePath;

	    private readonly IQueueWrapper _queueWrapper;

	    private readonly ILogger _logger;
	    private readonly IDateTimeProvider _dateTimeProvider;

        public RequestStore(
	        IDateTimeProvider dateTimeProvider,
	        IQueueWrapper queueWrapper,
	        ILogger logger,
            IConfigurationManager configurationManager)
	    {
	        _queueWrapper = queueWrapper;
	        _logger = logger;
	        _dateTimeProvider = dateTimeProvider;

	        _modulePath = configurationManager.AppSettings["ModulePath"];
	        _responsePath = configurationManager.AppSettings["ResponsePath"];
        }

        public static readonly ConcurrentDictionary<Guid, DataWaitHandle<object>> RequestsStore = new ConcurrentDictionary<Guid, DataWaitHandle<object>>();

		public Func<Guid> CreateUniqueId { get; set; } = Guid.NewGuid;

		public HttpStatusCode PublishAndWaitForResponse(string baseAddress, dynamic message, HttpStatusCode successResponseCode, out object responseData)
		{
			var requestId = CreateUniqueId.Invoke();

			message.Uuid = requestId;
		    message.LastModifiedBy = Service.GetServiceName();
		    message.LastModifiedTime = _dateTimeProvider.UtcNow;

            var callbackUri = $"{baseAddress}/{_modulePath}/{_responsePath}/{requestId}";

		    _logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Preparing to publish message\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\" CallbackUri=\"{callbackUri}\"");

            using (var dataWaitHandle = new DataWaitHandle<object>(false, EventResetMode.AutoReset))
		    {
		        if (RequestsStore.TryAdd(requestId, dataWaitHandle))
		        {
		            Message<Callback, object> createCallbackMessage = new Message<Callback, object>
		            {
		                Uuid = Guid.NewGuid(),
		                Method = CreateCallbackMessageMethod,
		                LastModifiedBy = Service.GetServiceName(),
		                LastModifiedTime = _dateTimeProvider.UtcNow,
		                Source = Service.GetServiceName(),
		                Need = new Callback
		                {
		                    Id = requestId,
		                    Uri = callbackUri
		                }
		            };

		            _queueWrapper.PublishMessage(createCallbackMessage);
		            _queueWrapper.PublishMessage(message);

		            DataWaitHandle<object> dataWaitHandleFromRemove;

		            if (dataWaitHandle.WaitOne(MaxTimeToWaitForBusResponseInMilliseconds))
		            {
		                RequestsStore.TryRemove(requestId, out dataWaitHandleFromRemove);

		                var completedMessage = ((JObject)dataWaitHandle.Data).ToObject<Message<object, object>>();

		                _logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received response\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\" Message=\"{completedMessage}\" CallbackUri=\"{callbackUri}\"");

		                if (completedMessage.Errors.Any())
		                {
		                    responseData = completedMessage.Errors;

		                    if (completedMessage.Errors.Any(e => e.Type == ResponseError.ErrorType.Server))
		                    {
		                        return HttpStatusCode.InternalServerError;
		                    }

		                    return HttpStatusCode.BadRequest;
		                }

		                responseData = completedMessage.Solution;
		                return successResponseCode;
		            }

		            RequestsStore.TryRemove(requestId, out dataWaitHandleFromRemove);

		            responseData = "Failed to get a response from the bus";
		            _logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Published message and received no response\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\" Message=\"{message}\" CallbackUri=\"{callbackUri}\"");
		            return HttpStatusCode.RequestTimeout;
		        }
            }

            responseData = "Failed to publish request to the bus";
			_logger.Trace($"Operation=\"{nameof(RequestsStore)}.{nameof(PublishAndWaitForResponse)}\" Event=\"Failed to publish message\" MessageId=\"{requestId}\" MessageMethod=\"{message.Method}\" Message=\"{message}\" CallbackUri=\"{callbackUri}\"");
			return HttpStatusCode.InternalServerError;
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
	}


}