using MicroService.Core.Api.HttpServer;

namespace MicroService.Core.Api
{
	internal interface IRequestBroker
	{
		HttpResponse HandleRequest(HttpRequest header);
	}
}