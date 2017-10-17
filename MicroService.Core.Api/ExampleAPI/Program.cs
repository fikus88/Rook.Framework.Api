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
			Container.Scan(Assembly.GetEntryAssembly(), typeof(IApiService).GetTypeInfo().Assembly, typeof(IService).GetTypeInfo().Assembly);

			IApiService apiService = Container.GetInstance<IApiService>();
			apiService.Start();

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
