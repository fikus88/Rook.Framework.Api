using System;
using System.Net;
using Microlise.Microservice.Core.Api;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api
{
	public interface IRequestStore
	{
		Func<Guid> CreateUniqueId { get; set; }
		bool FindResponse(MessageWrapper messageWrapper);
		void PublishAndWaitForResponse(dynamic message, HttpStatusCode successResponseCode, HttpResponse response);
		void Start();
	}
}
