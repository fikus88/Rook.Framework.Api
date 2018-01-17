using System.Reflection;
using System.Threading;
using Microlise.MicroService.Core.Api;
using Microlise.MicroService.Core.IoC;
using Microlise.MicroService.Core.Services;

namespace Microlise.Example.ExampleAPI
{
    internal static class Program
    {
	    private static void Main()
		{
#if NETCOREAPP2_0
            Container.Scan(Assembly.GetEntryAssembly(), typeof(IApiService).Assembly, typeof(IService).Assembly);
#else
			Container.Scan(Assembly.GetEntryAssembly(), typeof(IApiService).GetTypeInfo().Assembly, typeof(IService).GetTypeInfo().Assembly);
#endif            
            IApiService apiService = Container.GetInstance<IApiService>();
			apiService.Start();

		    Thread.CurrentThread.IsBackground = true;

            while (true)
				Thread.Sleep(int.MaxValue);
		}
    }
}
