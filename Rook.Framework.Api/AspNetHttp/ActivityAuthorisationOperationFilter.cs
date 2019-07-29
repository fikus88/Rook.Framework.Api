using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Rook.Framework.Core.Common;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Rook.Framework.Api.AspNetHttp
{
	public class ActivityAuthorisationOperationFilter : IOperationFilter
	{
		private readonly bool _requiresAuthorisation;

		public ActivityAuthorisationOperationFilter(IConfigurationManager config)
		{
			_requiresAuthorisation = config.Get("RequiresAuthorisation", true);
		}

		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var operationRequiresAuthorisation = context.MethodInfo
				.GetCustomAttributes(true)
				.OfType<ActivityAttribute>()
				.Any(x => !x.SkipAuthorisation);


			if (_requiresAuthorisation && operationRequiresAuthorisation)
			{
				operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
				operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

				var oAuthScheme = new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
				};

				operation.Security = new List<OpenApiSecurityRequirement>
				{
					new OpenApiSecurityRequirement
					{
						[ oAuthScheme ] = new List<string>()
					}
				};
			}
		}
	}
}
