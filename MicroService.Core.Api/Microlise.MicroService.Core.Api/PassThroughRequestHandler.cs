using System.Text;
using Newtonsoft.Json;
using Microlise.MicroService.Core.Application.Message;
using System.Net;
using Microlise.MicroService.Core.Api.HttpServer;
using Microlise.MicroService.Core.Common;

namespace Microlise.MicroService.Core.Api
{
    /// <summary>
    /// Passes the json in the body of the HTTP request directly onto the bus with a given Method name, returning the first received Solution(s) in the body of the HTTP response.
    /// </summary>
	public abstract class PassThroughActivityHandler : IActivityHandler
	{
		protected abstract string Method { get; }
		protected abstract HttpStatusCode SuccessStatusCode { get; }

		private readonly IRequestStore requestStore;

		protected PassThroughActivityHandler(IRequestStore requestStore)
		{
			this.requestStore = requestStore;
		}

		public virtual void Handle(HttpRequest request, HttpResponse response)
		{
			byte[] buffer = request.Body;
			object need = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(buffer));

			Message<dynamic, object> message = new Message<dynamic, object>
			{
				Source = ServiceInfo.Name,
				Method = Method,
				Need = need ?? new { }
			};

			requestStore.PublishAndWaitForResponse(message, SuccessStatusCode, response);
		}
	}
}