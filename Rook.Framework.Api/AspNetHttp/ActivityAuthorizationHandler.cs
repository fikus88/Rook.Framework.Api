using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Rook.Framework.Api.ActivityAuthorisation;
using Rook.Framework.Core.Common;

namespace Rook.Framework.Api.AspNetHttp
{
	public class ActivityAuthorizationHandler : AuthorizationHandler<ActivityAuthorizationRequirement>
	{
		private readonly IActivityAuthorisationManager _activityAuthorisationManager;
		private readonly bool _requiresAuthorisation;

		public ActivityAuthorizationHandler(IActivityAuthorisationManager activityAuthorisationManager, IConfigurationManager config)
		{
			_activityAuthorisationManager = activityAuthorisationManager;
			_requiresAuthorisation = config.Get("RequiresAuthorisation", true);
		}
		
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ActivityAuthorizationRequirement requirement)
		{
			bool succeed = false;
			if (context.Resource is AuthorizationFilterContext mvcContext &&
			    mvcContext.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
			{
				var activityAttribute = controllerActionDescriptor.MethodInfo.GetCustomAttributes<ActivityAttribute>().SingleOrDefault();
				if (activityAttribute == null)
				{
					throw new InvalidOperationException($"{nameof(ActivityAttribute)} not found.");
				}

				var activityName = activityAttribute.ActivityName;
				var activityRoles = _activityAuthorisationManager.ActivityRoles[activityName] ?? Enumerable.Empty<string>();

				bool hasRequiredClaim = activityRoles.Any(role => context.User.IsInRole(role));

				succeed = !_requiresAuthorisation || activityAttribute.SkipAuthorisation || hasRequiredClaim;
			}

			if (succeed)
			{
				context.Succeed(requirement);
			}

			return Task.CompletedTask;
		}
	}
}
