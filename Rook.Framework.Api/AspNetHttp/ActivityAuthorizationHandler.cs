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

namespace Rook.Framework.Api.AspNetHttp
{
	public class ActivityAuthorizationHandler : AuthorizationHandler<ActivityAuthorizationRequirement>
	{
		private readonly IActivityAuthorisationManager _activityAuthorisationManager;

		public ActivityAuthorizationHandler(IActivityAuthorisationManager activityAuthorisationManager)
		{
			_activityAuthorisationManager = activityAuthorisationManager;
		}
		
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ActivityAuthorizationRequirement requirement)
		{
			bool hasRequiredClaim = false;
			if (context.Resource is AuthorizationFilterContext mvcContext &&
			    mvcContext.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
			{
				var authorizeActivityAttribute = controllerActionDescriptor.MethodInfo.GetCustomAttributes<AuthorizeActivityAttribute>().SingleOrDefault();
				if (authorizeActivityAttribute == null)
				{
					throw new InvalidOperationException($"{nameof(AuthorizeActivityAttribute)} not found.");
				}

				var activityName = authorizeActivityAttribute.ActivityName;
				var activityRoles = _activityAuthorisationManager.ActivityRoles[activityName] ?? Enumerable.Empty<string>();

				hasRequiredClaim = activityRoles.Any(role => context.User.IsInRole(role));
			}

			if (hasRequiredClaim)
			{
				context.Succeed(requirement);
			}

			return Task.CompletedTask;
		}
	}
}
