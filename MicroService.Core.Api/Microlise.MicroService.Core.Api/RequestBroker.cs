using Microlise.MicroService.Core.Api.HttpServer;
using System;
using Microlise.MicroService.Core.IoC;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microlise.MicroService.Core.Common;

namespace Microlise.MicroService.Core.Api
{
	internal class RequestBroker : IRequestBroker
	{
		private IEnumerable<KeyValuePair<Type, VerbHandlerAttribute[]>> verbHandlers;
		private readonly IDateTimeProvider dateTimeProvider;
		private readonly ILogger logger;

		public RequestBroker(IDateTimeProvider dateTimeProvider, ILogger logger)
		{
			this.dateTimeProvider = dateTimeProvider;
			this.logger = logger;
		}

		public HttpResponse HandleRequest(HttpRequest request)
		{
			logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "GetRequestHandler started"));
			Stopwatch timer = Stopwatch.StartNew();
			IVerbHandler handler = GetRequestHandler(request);
			logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "GetRequestHandler completed"), new LogItem("DurationMilliseconds", timer.Elapsed.TotalMilliseconds), new LogItem("FoundHandler", handler != null ? handler.GetType().Name : "null"));

			if (handler == null)
				return HttpResponse.MethodNotFound;

		    JwtSecurityToken token = request.SecurityToken;
            // Check role here

		    HttpResponse response = new HttpResponse(dateTimeProvider);

            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "Handler Handle called"));
			timer.Restart();
			handler.Handle(request, response);
			logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "Handler Handle completed"), new LogItem("DurationMilliseconds",timer.Elapsed.TotalMilliseconds));
			return response;
		}

		private readonly Dictionary<Type, IVerbHandler> singletonCache = new Dictionary<Type, IVerbHandler>();

		private IVerbHandler GetRequestHandler(HttpRequest request)
		{
			bool Predicate(VerbHandlerAttribute attr) => RequestMatchesAttribute(request, attr);

			IEnumerable<KeyValuePair<Type, VerbHandlerAttribute[]>> handlers =
				(verbHandlers ?? (verbHandlers = Container.FindAttributedTypes<VerbHandlerAttribute>())).ToArray();

			KeyValuePair<Type, VerbHandlerAttribute[]> handlerInfo = handlers.FirstOrDefault(kvp => kvp.Value.Any(Predicate));

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

			string[] pathParts = path.Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries);

			string[] tokens = pattern.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			string[] values = pathParts[0].Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			if (tokens.Length != values.Length) return false;

            /*
             * if any non-tokenised path field doesn't match, bomb out
             * These should match
             * /link/{orgId}/xyz/{userId}
             * /link/anything/xyz/blah?hello=world&etc=etc
             * 
             * These should not
		     * /link/{orgId}/xyz/{userId}
		     * /link/{orgId}/abc/{userId}
             */

            for (int i = 0; i < tokens.Length; i++)
		        if (!tokens[i].StartsWith("{") && !tokens[i].EndsWith("}") && tokens[i] != values[i])
		            return false;

		    return true;
		}
	}


}