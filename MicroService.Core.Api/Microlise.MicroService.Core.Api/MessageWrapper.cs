using System;
using Microlise.MicroService.Core.Data;

namespace Microlise.MicroService.Core.Api {
    public sealed class MessageWrapper : DataObjectIncrementalId
	{
		public Guid Uuid;
		public string SolutionJson;
	    public string FirstOrDefaultJson;
		public string ErrorsJson;

	    public MessageWrapper()
		{
			ExpiresAt = DateTime.UtcNow.AddMinutes(2);
		}
	}
}