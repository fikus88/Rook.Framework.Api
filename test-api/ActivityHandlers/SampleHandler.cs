using System.Net;
using Rook.Framework.Api;
using Rook.Framework.Core.Application.Message;
using Rook.Framework.Core.Common;
using Rook.Framework.Core.HttpServer;
using IRequestStore = Rook.Framework.Core.IRequestStore;

namespace test_api.ActivityHandlers
{
	[ActivityHandler("SampleHandler", HttpVerb.Get, "/sample")]
	public class SampleHandler : IActivityHandler
	{
		private readonly IRequestStore _requestStore;
		private readonly ILogger _logger;

		public SampleHandler(IRequestStore requestStore, ILogger logger)
		{
			_requestStore = requestStore;
			_logger = logger;
		}
		public void Handle(IHttpRequest request, IHttpResponse response)
		{
			var responseMessage = _requestStore.PublishAndWaitForTypedResponse(new Message<object, object>
			{
				Method = "SampleMethod",
				Need = new { }
			});

			response.HttpStatusCode = HttpStatusCode.OK;
		}

		public dynamic ExampleRequestDocument => "";
		public dynamic ExampleResponseDocument => "";
	}
}