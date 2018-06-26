using System;
using Microlise.MicroService.Core.IoC;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using Microlise.MicroService.Core.Api.ActivityAuthorisation;
using Microlise.MicroService.Core.Api.BuiltInActivityHandlers;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.HttpServer;

namespace Microlise.MicroService.Core.Api
{
    internal class RequestBroker : IRequestBroker
    {
        private IEnumerable<KeyValuePair<Type, ActivityHandlerAttribute[]>> activityHandlers;
        private readonly ILogger logger;
        private readonly IActivityAuthorisationManager activityAuthorisationManager;
        private readonly IApiMetrics apiMetrics;

        public RequestBroker(ILogger logger, IActivityAuthorisationManager activityAuthorisationManager, IApiMetrics apiMetrics)
        {
            this.logger = logger;
            this.activityAuthorisationManager = activityAuthorisationManager;
            this.apiMetrics = apiMetrics;
        }
        
        public IHttpResponse HandleRequest(IHttpRequest request, TokenState tokenState)
        {
            IHttpResponse response = Container.GetNewInstance<IHttpResponse>();

            if (tokenState == TokenState.Invalid || tokenState == TokenState.Expired || tokenState == TokenState.NotYetValid)
            {
                response = Container.GetNewInstance<IHttpResponse>();
                response.HttpStatusCode = HttpStatusCode.Unauthorized;
                return response;
            }

            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "GetRequestHandler started"));
            Stopwatch timer = Stopwatch.StartNew();
            
            Core.HttpServer.IActivityHandler handler = GetRequestHandler(request, out ActivityHandlerAttribute attribute);

            var handlerName = handler != null ? handler.GetType().Name : "null";

            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "GetRequestHandler completed"), new LogItem("DurationMilliseconds", timer.Elapsed.TotalMilliseconds), new LogItem("FoundHandler", handlerName));

            if (handler == null)
                switch (request.Verb)
                {
                    case HttpVerb.Options:
                        handler = Container.GetInstance<OptionsActivityHandler>();
                        attribute = new ActivityHandlerAttribute("", request.Verb, "") { SkipAuthorisation = true };
                        break;
                    default:
                        return HttpResponse.MethodNotFound;
                }

            JwtSecurityToken token = request.SecurityToken;
            if (!activityAuthorisationManager.CheckAuthorisation(token, attribute))
            {
                IHttpResponse unauthorisedResponse = Container.GetNewInstance<IHttpResponse>();
                unauthorisedResponse.HttpStatusCode = HttpStatusCode.Unauthorized;
                return unauthorisedResponse;
            }

            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "Handler Handle called"));
            timer.Restart();
            handler.Handle(request, response);
            
            var elapsedMilliseconds = timer.Elapsed.TotalMilliseconds;
            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "Handler Handle completed"), new LogItem("DurationMilliseconds", elapsedMilliseconds));
            apiMetrics.RecordHandlerDuration(elapsedMilliseconds, handlerName, response.HttpStatusCode);
            
            return response;
        }

        private Core.HttpServer.IActivityHandler GetRequestHandler(IHttpRequest request, out ActivityHandlerAttribute activityHandler)
        {
            activityHandler = null;
            bool Predicate(ActivityHandlerAttribute attr) => RequestPathMatchesAttributePath(request, attr);

            IEnumerable<KeyValuePair<Type, ActivityHandlerAttribute[]>> handlers =
                (activityHandlers ?? (activityHandlers = Container.FindAttributedTypes<ActivityHandlerAttribute>()))
                .ToArray();

            logger.Debug(nameof(RequestBroker) + "." + nameof(GetRequestHandler),
                new LogItem("Event", "Got handlers"),
                new LogItem("Handlers", string.Join(",", handlers.Select(kvp => kvp.Key.Name))));

            KeyValuePair<Type, ActivityHandlerAttribute[]> handlerInfo =
                handlers.FirstOrDefault(kvp => kvp.Value.Any(Predicate));

            logger.Debug(nameof(RequestBroker) + "." + nameof(GetRequestHandler),
                new LogItem("HandlerInfo", handlerInfo.Key?.Name));

            if (handlerInfo.Key == null) return null;

            ActivityHandlerAttribute attribute = handlerInfo.Value.First(Predicate);
            activityHandler = attribute;
            request.SetUriPattern(attribute.Path);

            Core.HttpServer.IActivityHandler instance = (Core.HttpServer.IActivityHandler) Container.GetInstance(handlerInfo.Key);

            return instance;
        }

        private static bool RequestPathMatchesAttributePath(IHttpRequest request, ActivityHandlerAttribute attribute)
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
            {
                bool replacable = tokens[i].StartsWith("{") && tokens[i].EndsWith("}");
                if (!replacable && tokens[i] != values[i])
                    return false;
            }

            return true;
        }
    }
}