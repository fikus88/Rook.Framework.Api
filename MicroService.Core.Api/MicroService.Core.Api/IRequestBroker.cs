using MicroService.Core.Api.HttpServer;

namespace MicroService.Core.Api
{
	public interface IRequestBroker
	{
		HttpResponse HandleRequest(HttpRequest header);
	}
}