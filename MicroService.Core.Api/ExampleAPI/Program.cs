using System.Reflection;
using System.Threading;
using Microlise.MicroService.Core.IoC;
using Microlise.MicroService.Core.Services;
using Microlise.MicroService.Core.Api;
using Microlise.MicroService.Core.Api.HttpServer;

namespace ExampleAPI
{
    public static class Program
    {
		static void Main()
		{
			Container.Scan(Assembly.GetEntryAssembly(), typeof(IService).GetTypeInfo().Assembly);

			IService instance = Container.GetInstance<IService>();
			INanoHttp httpServer = Container.GetInstance<INanoHttp>();

			Thread.CurrentThread.Name = $"{instance.ServiceName} Main Thread";

			instance.Start();
			httpServer.Start();

#if NETCOREAPP2_0
            AppDomain.CurrentDomain.ProcessExit += (s, e) => instance.Stop();
#else

#endif
			Thread.CurrentThread.IsBackground = true;

			while (true)
				Thread.Sleep(int.MaxValue);
		}
    }
}
