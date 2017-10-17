using System;
using Microlise.MicroService.Core.Data;

namespace Microlise.Microservice.Core.Api {
	internal class MessageWrapper : DataEntity
	{
		public Guid Uuid;
		public object Message;
	}
}