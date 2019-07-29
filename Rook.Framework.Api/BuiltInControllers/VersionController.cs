using Microsoft.AspNetCore.Mvc;
using Rook.Framework.Api.AspNetHttp;
using Rook.Framework.Core.Common;
using Rook.Framework.Core.HttpServerAspNet;

namespace Rook.Framework.Api.BuiltInControllers
{
	[ApiController]
	public class VersionController : ControllerBase
	{
		[HttpGet("version")]
		[SwaggerTag("System")]
		[Activity("GetVersion", SkipAuthorisation = true)]
		public ActionResult<string> Get()
		{
			return Ok(new { version = ServiceInfo.Version });
		}
	}
}
