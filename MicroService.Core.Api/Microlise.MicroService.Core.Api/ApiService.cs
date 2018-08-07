using System;
using System.Threading;
using Microlise.MicroService.Core.Api.ActivityAuthorisation;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.Services;

namespace Microlise.MicroService.Core.Api
{
    public sealed class ApiService : IApiService
	{
	    private readonly ILogger _logger;
	    private readonly IService _service;
	    private readonly IActivityAuthorisationManager _activityAuthorisationManager;

	    public ApiService(ILogger logger, IService service, IActivityAuthorisationManager activityAuthorisationManager)
	    {
	        _logger = logger;
	        _service = service;
	        _activityAuthorisationManager = activityAuthorisationManager;
	    }

		public void Start()
		{
		    AppDomain.CurrentDomain.UnhandledException += (s, e) => _logger.Error("Unhandled", new LogItem("Exception", e.ExceptionObject.ToString));
			Thread.CurrentThread.Name = $"{ServiceInfo.Name} Main Thread";

			_service.Start();
			_activityAuthorisationManager.Initialise();
        }
    }
}