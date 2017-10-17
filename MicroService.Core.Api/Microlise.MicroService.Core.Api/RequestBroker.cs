using Microlise.MicroService.Core.Api;
using Microlise.MicroService.Core.Api.HttpServer;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System;
using Microlise.MicroService.Core.IoC;
using System.Collections.Generic;
using System.Linq;
using Microlise.MicroService.Core.Common;

namespace Microlise.MicroService.Core.Api
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
			IVerbHandler handler = GetRequestHandler(request);

			if (handler == null)
				return HttpResponse.MethodNotFound;

			HttpStatusCode statusCode = handler.Handle(request, out object content);
			return new HttpResponse(dateTimeProvider)
			{
				HttpStatusCode = statusCode,
				_content = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(content))
			};
		}

		private readonly Dictionary<Type, IVerbHandler> singletonCache = new Dictionary<Type, IVerbHandler>();

		private IVerbHandler GetRequestHandler(HttpRequest request)
		{
			bool Predicate(VerbHandlerAttribute attr) => RequestMatchesAttribute(request, attr);

			IEnumerable<KeyValuePair<Type, VerbHandlerAttribute[]>> verbHandlers =
				(_verbHandlers ?? (_verbHandlers = Container.FindAttributedTypes<VerbHandlerAttribute>())).ToArray();

			KeyValuePair<Type, VerbHandlerAttribute[]> handlerInfo = verbHandlers.FirstOrDefault(kvp => kvp.Value.Any(Predicate));

			if (handlerInfo.Key == null) return null;

			VerbHandlerAttribute attribute = handlerInfo.Value.First(Predicate);

			request.SetUriPattern(attribute.Path);

			IVerbHandler instance;

			if (attribute.AsSingleton)
			{
				if (!singletonCache.ContainsKey(handlerInfo.Key))
					singletonCache.Add(handlerInfo.Key, (IVerbHandler)Activator.CreateInstance(handlerInfo.Key));

				instance = singletonCache[handlerInfo.Key];
			}
			else
			{
				instance = (IVerbHandler)Activator.CreateInstance(handlerInfo.Key);
			}

			return instance;
		}

		private bool RequestMatchesAttribute(HttpRequest request, VerbHandlerAttribute attribute)
		{
			if (request.Verb != attribute.Verb) return false;

			string pattern = attribute.Path;
			string path = request.Path;

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