using System.Threading;
using Microlise.MicroService.Core.Api.ActivityAuthorisation;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.HttpServer;
using Microlise.MicroService.Core.IoC;
using Microlise.MicroService.Core.Services;

namespace Microlise.MicroService.Core.Api
{
    public sealed class ApiService : IApiService
	{
		public void Start()
		{
            // Override the Core.HttpServer.RequestBroker to give full API functionality            
            Container.Map<IRequestBroker, RequestBroker>();

            IService instance = Container.GetInstance<IService>();
			ActivityAuthorisationManager aam = Container.GetInstance<ActivityAuthorisationManager>();

			Thread.CurrentThread.Name = $"{ServiceInfo.Name} Main Thread";

			instance.Start();
			aam.Initialise();
        }
    }
}