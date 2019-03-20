using System;
using System.Threading;
using Rook.Framework.Core.Common;
using Rook.Framework.Core.Services;
using Rook.Framework.Core.Api.ActivityAuthorisation;

namespace Rook.Framework.Core.Api
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