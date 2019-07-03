using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Rook.Framework.Core.Common;

namespace Rook.Framework.Api
{
	public class ActionDurationActionFilter : IActionFilter
	{
		private readonly IApiMetrics _apiMetrics;
		private readonly ILogger _logger;
		private Stopwatch _stopwatch;

		public ActionDurationActionFilter(IApiMetrics apiMetrics, ILogger logger)
		{
			_apiMetrics = apiMetrics;
			_logger = logger;
		}

		public void OnActionExecuting(ActionExecutingContext context)
		{
            _logger.Trace(context.ActionDescriptor.DisplayName, new LogItem("Event", "Controller Action Executing"));
			_stopwatch = Stopwatch.StartNew();
		}

		public void OnActionExecuted(ActionExecutedContext context)
		{
			_stopwatch.Stop();

			double elapsedMilliseconds = _stopwatch.Elapsed.TotalMilliseconds;

			_logger.Trace(context.ActionDescriptor.DisplayName, new LogItem("Event", "Controller Action Finished Executing"), new LogItem("DurationMilliseconds", elapsedMilliseconds));
			_apiMetrics.RecordHandlerDuration(elapsedMilliseconds, context.ActionDescriptor.DisplayName, (HttpStatusCode)context.HttpContext.Response.StatusCode);
		}
	}
}