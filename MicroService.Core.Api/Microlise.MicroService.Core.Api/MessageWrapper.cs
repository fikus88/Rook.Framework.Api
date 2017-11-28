using System;
using Microlise.MicroService.Core.Data;

namespace Microlise.MicroService.Core.Api {
    [CappedCollection]
	public sealed class MessageWrapper : DataEntity
	{
		public Guid Uuid;
		public string SolutionJson;
		public string ErrorsJson;

	    public MessageWrapper()
		{
			ExpiresAt = DateTime.UtcNow.AddMinutes(1);
		}
	}
}