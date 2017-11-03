using System.Threading;
using Microlise.MicroService.Core.Api.HttpServer;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.IoC;
using Microlise.MicroService.Core.Services;

namespace Microlise.MicroService.Core.Api {
	public sealed class ApiService : IApiService
	{
		public void Start()
		{
			IService instance = Container.GetInstance<IService>();
			INanoHttp httpServer = Container.GetInstance<INanoHttp>();
			IRequestStore requestStore = Container.GetInstance<IRequestStore>();

			Thread.CurrentThread.Name = $"{ServiceInfo.Name} Main Thread";

			instance.Start();
			httpServer.Start();
			requestStore.Start();
		}
	}
}