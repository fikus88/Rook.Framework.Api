using System.Text;
using Newtonsoft.Json;
using Microlise.MicroService.Core.Services;
using Microlise.MicroService.Core.Application.Message;
using System;
using System.Net;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api
{
	public abstract class PassThroughVerbHandler : IVerbHandler
	{
		protected abstract string Method { get; }
		protected abstract HttpStatusCode SuccessStatusCode { get; }

		private readonly IRequestStore requestStore;

		protected PassThroughVerbHandler(IRequestStore requestStore)
		{
			this.requestStore = requestStore;
		}

		public virtual HttpStatusCode Handle(HttpRequest request, out object responseData)
		{
			byte[] buffer = request.Body;
			object need = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(buffer));

			Message<dynamic, object> message = new Message<dynamic, object>
			{
				Source = Service.GetServiceName(),
				Method = Method,
				Need = need ?? new { }
			};

			return requestStore.PublishAndWaitForResponse(message, SuccessStatusCode, out responseData);
		}
	}
}