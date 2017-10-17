using System.Net;
using Microlise.MicroService.Core.Api;
using Microlise.MicroService.Core.Api.HttpServer;

namespace ExampleAPI.VerbHandlers {
	[VerbHandler(HttpVerb.Put, "/widget/{widgetId}")]
	internal class PutWidget : PassThroughVerbHandler
	{
		public PutWidget(IRequestStore requestStore) : base(requestStore) { }
		protected override string Method => "StoreWidget";
		protected override HttpStatusCode SuccessStatusCode => HttpStatusCode.OK;
	}
}