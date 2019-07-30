using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Rook.Framework.Api.AspNetHttp
{
	public static class Extensions
	{
		public static bool IsRookFrameworkCoreAction(this ControllerActionDescriptor controllerActionDescriptor)
		{
			return controllerActionDescriptor.ControllerTypeInfo.Assembly ==
			       Assembly.GetAssembly(typeof(Core.HttpServerAspNet.AspNetHttp));
		}
	}
}
