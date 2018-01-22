using System.Linq;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.MessageHandlers;
using Microlise.MicroService.Core.Attributes;
using Microlise.MicroService.Core.Data;
using Newtonsoft.Json;

namespace Microlise.MicroService.Core.Api.MessageHandlers
{
	[Handler("*")]
	public class RequestResponseMessageHandler<TNeed, TSolution> : IMessageHandler2<TNeed, TSolution>
	{
		private readonly IMongoStore mongo;
        
        public RequestResponseMessageHandler(IMongoStore mongo)
		{
			this.mongo = mongo;
		}

		public CompletionAction Handle(Message<TNeed, TSolution> message)
		{
		    // If Solution == null and there are no errors, we have nothing to store.
		    if (message.Solution != null || message.Errors.Any())
		        if (RequestStore.Methods.Contains(message.Method))
		        {
		            MessageWrapper mw = new MessageWrapper {
		                Uuid = message.Uuid,
		                SolutionJson = JsonConvert.SerializeObject(message.Solution),
		                ErrorsJson = JsonConvert.SerializeObject(message.Errors)
		            };

		            if (message.Solution != null)
		                mw.FirstOrDefaultJson = JsonConvert.SerializeObject(message.Solution.FirstOrDefault());

		            mongo.Put(mw);
		        }
            return CompletionAction.DoNothing;
		}
	}
}

