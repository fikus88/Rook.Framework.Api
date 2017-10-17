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

		private readonly IRequestStore _requestStore;

		protected PassThroughVerbHandler(IRequestStore requestStore)
		{
			_requestStore = requestStore;
		}

		public virtual HttpStatusCode Handle(HttpRequest request, out object responseData)
		{
			byte[] buffer = request.Body;
			var need = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(buffer));

			var message = new Message<dynamic, object>
			{
				Source = Service.GetServiceName(),
				Method = Method,
				Need = need ?? new { }
			};

			string port = (Environment.GetEnvironmentVariable("CURRENT_PORT") ?? (request.Uri.Port).ToString());

			var callBackUrl = $"{request.Uri.Scheme}://{request.Uri.Host}:{port}";

			return _requestStore.PublishAndWaitForResponse(callBackUrl, message, SuccessStatusCode, out responseData);
		}
	}
}