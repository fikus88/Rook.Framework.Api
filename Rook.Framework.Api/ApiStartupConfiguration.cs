using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Rook.Framework.Core.HttpServerAspNet;

namespace Rook.Framework.Api
{
	public class ApiStartupConfiguration : IAspNetStartupConfiguration
	{
		public void AddMiddleware(IApplicationBuilder applicationBuilder)
		{
		}

		public IEnumerable<Type> ActionFilterTypes => new[] { typeof(ActionDurationActionFilter) };
	}
}
