using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Rook.Framework.Core.HttpServerAspNet;

namespace Rook.Framework.Api
{
	public class ApiStartupConfiguration : IAspNetStartupConfiguration
	{
		public void AddMiddleware(IApplicationBuilder applicationBuilder)
		{
		}


		public IEnumerable<Type> ActionFilterTypes => new[] { typeof(ActionDurationActionFilter) };
		public IEnumerable<IActionFilter> ActionFilters { get; }
	}
}
