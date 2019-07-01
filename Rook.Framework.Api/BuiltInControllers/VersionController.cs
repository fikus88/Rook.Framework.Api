using Microsoft.AspNetCore.Mvc;
using Rook.Framework.Core.Common;

namespace Rook.Framework.Api.BuiltInControllers
{
	[Route("[controller]")]
	[ApiController]
	public class VersionController : ControllerBase
	{
		[HttpGet]
		public ActionResult<string> Get()
		{
			return Ok(new { version = ServiceInfo.Version });
		}
	}
}
