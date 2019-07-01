using Microsoft.AspNetCore.Mvc;
using Rook.Framework.Core.Common;

namespace Rook.Framework.Api.BuiltInControllers
{
	[Route("[controller]")]
	[ApiController]
	public class VersionController : ControllerBase
	{
		[HttpGet]
		public JsonResult Get()
		{
			return new JsonResult(new { version = ServiceInfo.Version });
		}
	}
}
