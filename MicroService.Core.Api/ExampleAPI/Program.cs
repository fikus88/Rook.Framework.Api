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
            IApiService apiService = Container.GetInstance<IApiService>();
			apiService.Start();

		    Thread.CurrentThread.IsBackground = true;

            while (true)
				Thread.Sleep(int.MaxValue);
		}
    }
}
