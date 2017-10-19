using System;
using Microlise.MicroService.Core.Data;

namespace Microlise.MicroService.Core.Api {
	public sealed class MessageWrapper : DataEntity
	{
		public Guid Uuid;
		public object Message;

		internal MessageWrapper()
		{
			ExpiresAt = DateTime.UtcNow.AddMinutes(1);
		}
	}
}