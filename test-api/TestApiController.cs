using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;
using Microsoft.AspNetCore.Authorization;
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

		[HttpGet("auth")]
		[SwaggerTag("Test")]
		[Activity("TestActivity")]
		public IActionResult TestAuthenticated()
		{
			return Ok("Test");
		}
	}
}
