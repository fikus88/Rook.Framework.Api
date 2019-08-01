using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Rook.Framework.Api.AspNetHttp
{
	public class ActivityAuthorizationRequirement : IAuthorizationRequirement
	{
		public ActivityAuthorizationRequirement()
		{
		}
	}
}
