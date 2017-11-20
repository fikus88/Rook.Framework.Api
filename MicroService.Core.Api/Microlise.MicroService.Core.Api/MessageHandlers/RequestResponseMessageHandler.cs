using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.MessageHandlers;
using Microlise.MicroService.Core.Attributes;
using Microlise.MicroService.Core.Data;
using Microlise.MicroService.Core.IoC;

namespace Microlise.MicroService.Core.Api.MessageHandlers
{
	[Handler("*")]
	public class RequestResponseMessageHandler<TNeed, TSolution> : IMessageHandler<TNeed, TSolution>
	{
		private readonly IMongoStore mongo;
        
        public RequestResponseMessageHandler(IMongoStore mongo)
		{
			this.mongo = mongo;
		}

		public void Handle(Message<TNeed, TSolution> message)
		{
			if (message.Solution == null && !message.Errors.Any()) return;
		    if (RequestStore.Methods.Contains(message.Method))
		    {
		        MessageWrapper mw = new MessageWrapper {
		            Uuid = message.Uuid,
		            Message = message
		        };
		        mongo.Put(mw);
		    }
		}
	}
}

