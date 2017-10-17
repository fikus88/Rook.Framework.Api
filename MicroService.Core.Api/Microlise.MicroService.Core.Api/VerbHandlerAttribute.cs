using System;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api
{
	public sealed class VerbHandlerAttribute : Attribute
	{
		public VerbHandlerAttribute(HttpVerb verb, string path, string description = null)
		{
			Verb = verb;
			Path = path;
			Description = description ?? $"{verb} handler for {path}";
		}

		public string Description { get; }
				
		public HttpVerb Verb { get; }
		public string Path { get; }
		public bool AsSingleton { get; set; }
	}
}