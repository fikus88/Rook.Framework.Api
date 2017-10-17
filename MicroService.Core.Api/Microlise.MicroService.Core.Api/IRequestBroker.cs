using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api
{
	public interface IRequestBroker
	{
		HttpResponse HandleRequest(HttpRequest header);
	}
}