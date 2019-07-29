using System;

namespace Rook.Framework.Api.AspNetHttp
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ActivityAttribute : Attribute
	{
		public string ActivityName { get; }
		public bool SkipAuthorisation { get; set; }

		public ActivityAttribute(string activityName)
		{
			ActivityName = activityName;
		}
	}
}
