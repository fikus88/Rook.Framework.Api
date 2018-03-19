using System.Text;
using Newtonsoft.Json;
using Microlise.MicroService.Core.Application.Message;
using System.Net;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.HttpServer;

namespace Microlise.MicroService.Core.Api
{
    /// <summary>
    /// Passes the json in the body of the HTTP request directly onto the bus with a given Method name, returning the first received Solution(s) in the body of the HTTP response.
    /// </summary>
	public abstract class PassThroughActivityHandler : Core.HttpServer.IActivityHandler
    {
		protected abstract string Method { get; }
		protected abstract HttpStatusCode SuccessStatusCode { get; }

		private readonly IRequestStore requestStore;

        protected virtual ResponseStyle ResponseStyle { get; } = ResponseStyle.WholeSolution;

		protected PassThroughActivityHandler(IRequestStore requestStore)
		{
			this.requestStore = requestStore;
		}

		public virtual void Handle(IHttpRequest request, IHttpResponse response)
		{
			byte[] buffer = request.Body;
			object need = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(buffer));

			Message<dynamic, object> message = new Message<dynamic, object>
			{
				Source = ServiceInfo.Name,
				Method = Method,
				Need = need ?? new { }
			};

			requestStore.PublishAndWaitForResponse(message, SuccessStatusCode, response, ResponseStyle,s=>s.Length>0);
		}

	    public abstract dynamic ExampleRequestDocument { get; }
	    public abstract dynamic ExampleResponseDocument { get; }
	}
}