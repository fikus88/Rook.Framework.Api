using System.Linq;
using Microlise.MicroService.Core.Application.MessageHandlers;
using Microlise.MicroService.Core.Attributes;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.Data;
using MongoDB.Driver;

namespace Microlise.Microservice.Core.Api.MessageHandlers
{
	[Handler("*")]
	public class RequestResponseMessageHandler<TNeed, TSolution> : IMessageHandler<TNeed, TSolution>
	{
		private readonly ILogger _logger;
		private readonly IMongoStore mongo;

		public RequestResponseMessageHandler(ILogger logger, IMongoStore mongo)
		{
			_logger = logger;
			this.mongo = mongo;
		}

		public void Handle(Message<TNeed, TSolution> message)
		{
			if (message.Solution == null && !message.Errors.Any()) return;
			MessageWrapper mw = mongo.Get<MessageWrapper>(wrapper => wrapper.Uuid == message.Uuid).FirstOrDefault();
			if (mw != null)
			{
				mw.Message = message;
				mongo.Put(mw);
			}
		}
	}
}

