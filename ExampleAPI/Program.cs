using System.Threading;
using Rook.Framework.Api;
using Rook.Framework.Core.StructureMap;

namespace Rook.Example.ExampleAPI
{
    internal static class Program
    {
        private static void Main()
        {
            // ---------------------------------
            //  STRUCTURE MAP CONFIG // If you're using Structuremap - i.e. UseStructureMap=true in config.json
            // ---------------------------------
            var container = Bootstrapper.Init();

            //It's also possible to use these utility methods so we can see what the container has scanned/registered
            var whatDidIScan = container.WhatDidIScan();
            var whatDoIHave = container.WhatDoIHave();

            var apiService = container.GetInstance<IApiService>();
            // ---------------------------------

            // ---------------------------------
            //  LEGACY CONFIG // If you're not using Structuremap - you probably should - but you need to use this
            // ---------------------------------
            // Container.Map<IContainerFacade>(new ContainerFacade(null, Container.GetInstance<IConfigurationManager>()));
            // var apiService = Container.GetInstance<IApiService>();
            // ---------------------------------

            apiService.Start();

            Thread.CurrentThread.IsBackground = true;

            while (true)
                Thread.Sleep(int.MaxValue);
        }
    }
}
