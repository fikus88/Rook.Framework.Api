using Microlise.MicroService.Core.Api.HttpServer;
using System;
using Microlise.MicroService.Core.IoC;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using Microlise.MicroService.Core.Api.BuiltInActivityHandlers;
using Microlise.MicroService.Core.Api.MessageHandlers;
using Microlise.MicroService.Core.Common;

namespace Microlise.MicroService.Core.Api
{
    internal class RequestBroker : IRequestBroker
    {
        private IEnumerable<KeyValuePair<Type, ActivityHandlerAttribute[]>> activityHandlers;
        private readonly ILogger logger;
        private readonly IActivityAuthorisationManager activityAuthorisationManager;

        public RequestBroker(ILogger logger, IActivityAuthorisationManager activityAuthorisationManager)
        {
            this.logger = logger;
            this.activityAuthorisationManager = activityAuthorisationManager;
        }

        public IHttpResponse HandleRequest(IHttpRequest request)
        {
            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "GetRequestHandler started"));
            Stopwatch timer = Stopwatch.StartNew();
            IActivityHandler handler = GetRequestHandler(request, out ActivityHandlerAttribute attribute);
            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "GetRequestHandler completed"), new LogItem("DurationMilliseconds", timer.Elapsed.TotalMilliseconds), new LogItem("FoundHandler", handler != null ? handler.GetType().Name : "null"));

            if (handler == null)
                if (request.Verb == HttpVerb.Options)
                    handler = Container.GetInstance<OptionsActivityHandler>();
                else
                    return HttpResponse.MethodNotFound;

            JwtSecurityToken token = request.SecurityToken;
            if (!activityAuthorisationManager.CheckAuthorisation(token, attribute))
            {
                IHttpResponse unauthorisedResponse = Container.GetNewInstance<IHttpResponse>();
                unauthorisedResponse.HttpStatusCode = HttpStatusCode.Unauthorized;
                return unauthorisedResponse;
            }

            IHttpResponse response = Container.GetNewInstance<IHttpResponse>();

            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "Handler Handle called"));
            timer.Restart();
            handler.Handle(request, response);
            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "Handler Handle completed"), new LogItem("DurationMilliseconds", timer.Elapsed.TotalMilliseconds));
            return response;
        }

        private readonly Dictionary<Type, IActivityHandler> singletonCache = new Dictionary<Type, IActivityHandler>();

        private IActivityHandler GetRequestHandler(IHttpRequest request, out ActivityHandlerAttribute activityHandler)
        {
            activityHandler = null;
            bool Predicate(ActivityHandlerAttribute attr) => RequestMatchesAttribute(request, attr);

            IEnumerable<KeyValuePair<Type, ActivityHandlerAttribute[]>> handlers =
                (activityHandlers ?? (activityHandlers = Container.FindAttributedTypes<ActivityHandlerAttribute>())).ToArray();

            KeyValuePair<Type, ActivityHandlerAttribute[]> handlerInfo = handlers.FirstOrDefault(kvp => kvp.Value.Any(Predicate));

            if (handlerInfo.Key == null) return null;

            ActivityHandlerAttribute attribute = handlerInfo.Value.First(Predicate);
            activityHandler = attribute;
            request.SetUriPattern(attribute.Path);

            IActivityHandler instance;

            if (attribute.AsSingleton)
            {
                if (!singletonCache.ContainsKey(handlerInfo.Key))
                    singletonCache.Add(handlerInfo.Key, (IActivityHandler)Container.GetInstance(handlerInfo.Key));

                instance = singletonCache[handlerInfo.Key];
            }
            else
            {
                instance = (IActivityHandler)Container.GetInstance(handlerInfo.Key);
            }

            return instance;
        }

        private bool RequestMatchesAttribute(IHttpRequest request, ActivityHandlerAttribute attribute)
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
             * These two should match
             * /link/{orgId}/xyz/{userId}
             * /link/anything/xyz/blah?hello=world&etc=etc
             * 
             * These two should not
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