using Microsoft.AspNetCore.Mvc;
using Rook.Framework.Core.Common;
using Rook.Framework.Core.HttpServerAspNet;

namespace Rook.Framework.Api.BuiltInControllers
{
	[Route("[controller]")]
	[ApiController]
	public class VersionController : ControllerBase
	{
		[HttpGet]
		[SwaggerTag("System")]
		public ActionResult<string> Get()
		{
			return Ok(new { version = ServiceInfo.Version });
		}
	}
}
