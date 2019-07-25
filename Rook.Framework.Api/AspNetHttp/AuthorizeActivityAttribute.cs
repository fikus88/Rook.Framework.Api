using System;
using Microsoft.AspNetCore.Authorization;

namespace Rook.Framework.Api.AspNetHttp
{
	[AttributeUsage(AttributeTargets.Method)]
	public class AuthorizeActivityAttribute : AuthorizeAttribute
	{
		public string ActivityName { get; }
		public static readonly string AuthorizePolicyName = "Activity";

		public AuthorizeActivityAttribute(string activityName) : base(AuthorizePolicyName)
		{
			ActivityName = activityName;
		}
	}
}
