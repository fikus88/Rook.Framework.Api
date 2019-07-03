using Microsoft.AspNetCore.Mvc;

namespace testapi
{
	[Route("[controller]")]
	[ApiController]
    public class TestingController : ControllerBase
	{
		/// <summary>
		/// Summary of the Testing Get Method
		/// </summary>
		/// <returns>String of Testing 12345</returns>
		[HttpGet]
		public ActionResult<string> Get()
		{
			return Ok("Testing 12345");
		}
	}
}
