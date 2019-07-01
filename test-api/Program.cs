using System.Threading;
using Rook.Framework.Api;
using Rook.Framework.Core.StructureMap;

namespace test_api
{
    class Program
    {
		private static void Main()
		{
			var container = Bootstrapper.Init();

			var apiService = container.GetInstance<IApiService>();

			apiService.Start();

			Thread.CurrentThread.IsBackground = true;

			while (true)
				Thread.Sleep(int.MaxValue);
		}
	}
}
