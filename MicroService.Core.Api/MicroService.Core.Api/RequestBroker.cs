using Microlise.MicroService.Core.Api;
using MicroService.Core.Api.HttpServer;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System;
using Microlise.MicroService.Core.IoC;
using System.Collections.Generic;
using System.Linq;
using Microlise.MicroService.Core.Common;

namespace MicroService.Core.Api
{
	internal class RequestBroker : IRequestBroker
	{
		private IEnumerable<KeyValuePair<Type, VerbHandlerAttribute[]>> _verbHandlers = null;
		private readonly IDateTimeProvider dateTimeProvider;

		public RequestBroker(IDateTimeProvider dateTimeProvider)
		{
			this.dateTimeProvider = dateTimeProvider;
		}

		public HttpResponse HandleRequest(HttpRequest request)
		{
			IRequestHandler handler = GetRequestHandler(request);

			if (handler == null)
				return HttpResponse.MethodNotFound;

			HttpStatusCode statusCode = handler.Handle(request, out object content);
			return new HttpResponse(dateTimeProvider)
			{
				HttpStatusCode = statusCode,
				_content = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(content))
			};
		}

		private IRequestHandler GetRequestHandler(HttpRequest request)
		{
			IEnumerable<KeyValuePair<Type, VerbHandlerAttribute[]>> verbHandlers = (_verbHandlers ?? (_verbHandlers = Container.FindAttributedTypes<VerbHandlerAttribute>())).ToArray();
			KeyValuePair<Type, VerbHandlerAttribute[]> handlerInfo = verbHandlers.FirstOrDefault(kvp => kvp.Value.Any(v => v.Verb == request.Verb && PathPatternMatches(request.Path, v.Path)));
			IRequestHandler instance = (IRequestHandler)Activator.CreateInstance(handlerInfo.Key);
			request.SetUriPattern(handlerInfo.Value.FirstOrDefault(v => v.Verb == request.Verb).Path);
			return instance;
		}

		private bool PathPatternMatches(string path, string pattern)
		{
			string[] pathParts = path.Split('?');

			string[] tokens = pattern.Split('/');
			string[] values = pathParts[0].Split('/');

			if (tokens.Length != values.Length) return false;

			for (int i = 0; i < tokens.Length; i++)
			{
				string token = tokens[i];
				if (!token.StartsWith("{") && !token.EndsWith("}"))
					if (token != values[i]) return false;
			}

			return true;
		}
	}


}