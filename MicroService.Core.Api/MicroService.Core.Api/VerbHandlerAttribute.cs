using Microlise.MicroService.Core.Api;
using System;
using MicroService.Core.Api.HttpServer;
using System.Net;

namespace MicroService.Core.Api
{
	public sealed class VerbHandlerAttribute : Attribute
	{
		public VerbHandlerAttribute(HttpVerb verb, string path)
		{
			Verb = verb;
			Path = path;
		}
				
		public HttpVerb Verb { get; }
		public string Path { get; }
		public bool AsSingleton { get; set; }
	}
}