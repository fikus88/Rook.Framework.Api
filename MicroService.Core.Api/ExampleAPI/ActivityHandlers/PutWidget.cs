using System.Net;
using Microlise.MicroService.Core.Api;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.Example.ExampleAPI.VerbHandlers
{
    [ActivityHandler("StoreWidget", HttpVerb.Post, "/widget/{widgetId}", "Puts a Widget in the Widget Store")]
    internal class PutWidget : PassThroughActivityHandler
    {
        public PutWidget(IRequestStore requestStore) : base(requestStore) { }
        protected override string Method => "StoreWidget";
        protected override HttpStatusCode SuccessStatusCode => HttpStatusCode.OK;
    }
}