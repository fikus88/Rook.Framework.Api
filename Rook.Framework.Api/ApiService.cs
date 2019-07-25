using System;
using System.Reflection;
using System.Threading;
using Rook.Framework.Core.Common;
using Rook.Framework.Core.Services;
using Rook.Framework.Api.ActivityAuthorisation;
using Rook.Framework.Api.AspNetHttp;

namespace Rook.Framework.Api
{
    public sealed class ApiService : IApiService
	{
	    private readonly ILogger _logger;
	    private readonly IService _service;
	    private readonly IActivityAuthorisationManager _activityAuthorisationManager;
	    private readonly IConfigurationManager _configurationManager;

	    public ApiService(ILogger logger, IService service, IActivityAuthorisationManager activityAuthorisationManager, 
		    IConfigurationManager configurationManager)
	    {
	        _logger = logger;
	        _service = service;
	        _activityAuthorisationManager = activityAuthorisationManager;
	        _configurationManager = configurationManager;
	    }

		public void Start()
		{
		    AppDomain.CurrentDomain.UnhandledException += (s, e) => _logger.Error("Unhandled", new LogItem("Exception", e.ExceptionObject.ToString));
			Thread.CurrentThread.Name = $"{ServiceInfo.Name} Main Thread";
			Core.HttpServerAspNet.AspNetHttp.Configure(options =>
			{
				options.Filters.Add(typeof(ActionDurationActionFilter));
				options.AuthorizationPolicies.Add(AuthorizeActivityAttribute.AuthorizePolicyName, builder =>
				{
					builder.Requirements.Add(new ActivityAuthorizationRequirement());
				});
				options.AuthorizationHandlers.Add(typeof(ActivityAuthorizationHandler));
				options.MvcApplicationPartAssemblies.Add(Assembly.GetExecutingAssembly());
				options.IdentityServerOptions.RequireHttps = _configurationManager.Get("IdentityServerRequireHttps", true);
			});

			_service.Start();
			_activityAuthorisationManager.Initialise();
        }
    }
}