using System;
using Microlise.MicroService.Core.Application.Bus;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.Services;
using Microlise.MicroService.Core.Application.Subscribe;

namespace Microlise.MicroService.Core.Api
{
    public sealed class ApiService : Service, IDisposable
    {
        internal ApiService(IQueueWrapper queueWrapper, ILogger logger, IConfigurationManager configurationManager, IMessageSubscriber messageSubscriber):base(messageSubscriber,queueWrapper,logger)
	    {
		    var serviceUri = configurationManager.AppSettings["ServiceUri"];
	    }

	    public new bool Stop()
        {
            base.Stop();
            Dispose();

            return true;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}