using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Rook.Framework.Core.Common;

namespace Rook.Framework.Api
{
	public class RequestDurationMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger _logger;

		public RequestDurationMiddleware(RequestDelegate next, ILogger logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var timer = Stopwatch.StartNew();
			await _next(context);
			timer.Stop();
			_logger.Trace(context.Request.Path, new LogItem("requestDuration", timer.Elapsed.TotalMilliseconds));
		}
	}
}
