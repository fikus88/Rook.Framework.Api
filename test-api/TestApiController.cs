using Microsoft.AspNetCore.Mvc;
using Rook.Framework.Api.AspNetHttp;
using Rook.Framework.Core.HttpServerAspNet;

namespace test_api
{
	[ApiController]
	[Route("[controller]")]
	public class TestApiController : ControllerBase
	{
		[HttpGet]
		[SwaggerTag("Test")]
		public IActionResult Test()
		{
			return Ok("Test");
		}

		[HttpGet("authenticatedactivity")]
		[SwaggerTag("Test")]
		[Activity("AuthenticatedActivity")]
		public IActionResult TestAuthenticated()
		{
			return Ok("Authenticated activity was successful");
		}
	}
}
