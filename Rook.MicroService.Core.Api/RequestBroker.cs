﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using Rook.Framework.Core.Common;
using Rook.Framework.Core.HttpServer;
using Rook.Framework.Core.StructureMap;
using Rook.Framework.Core.Api.ActivityAuthorisation;
using Rook.Framework.Core.Api.BuiltInActivityHandlers;

namespace Rook.Framework.Core.Api
{
    internal class RequestBroker : IRequestBroker
    {
        private IDictionary<Type, ActivityHandlerAttribute[]> activityHandlers;
        private readonly ILogger logger;
        private readonly IActivityAuthorisationManager activityAuthorisationManager;
        private readonly IApiMetrics apiMetrics;
        private readonly IContainerFacade _container;
        private readonly IConfigurationManager _configurationManager;

        private IHttpResponse UnauthorisedResponse
        {
            get
            {
                var unauthorisedResponse = _container.GetInstance<IHttpResponse>(true);
                unauthorisedResponse.HttpStatusCode = HttpStatusCode.Unauthorized;
                unauthorisedResponse.HttpContent = new EmptyHttpContent();
                return unauthorisedResponse;
            }
        }

        public RequestBroker(ILogger logger, IActivityAuthorisationManager activityAuthorisationManager, 
            IApiMetrics apiMetrics, IContainerFacade container, IConfigurationManager configurationManager)
        {
            this.logger = logger;
            this.activityAuthorisationManager = activityAuthorisationManager;
            this.apiMetrics = apiMetrics;
            _container = container;
            _configurationManager = configurationManager;
        }
        
        public IHttpResponse HandleRequest(IHttpRequest request, TokenState tokenState)
        {
            IHttpResponse response = _container.GetInstance<IHttpResponse>(true);
            
            if (tokenState == TokenState.Invalid || tokenState == TokenState.Expired || tokenState == TokenState.NotYetValid)
            {
                return UnauthorisedResponse;
            }

            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "GetRequestHandler started"));
            Stopwatch timer = Stopwatch.StartNew();
            
            IActivityHandler handler = GetRequestHandler(request, out ActivityHandlerAttribute attribute);

            var handlerName = handler != null ? handler.GetType().Name : "null";

            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "GetRequestHandler completed"), new LogItem("DurationMilliseconds", timer.Elapsed.TotalMilliseconds), new LogItem("FoundHandler", handlerName));

            if (handler == null)
                switch (request.Verb)
                {
                    case HttpVerb.Options:
                        handler = _container.GetInstance<OptionsActivityHandler>();
                        attribute = new ActivityHandlerAttribute("", request.Verb, "") { SkipAuthorisation = true };
                        break;
                    default:
                        return null;
                }

            JwtSecurityToken token = request.SecurityToken;
            if (!activityAuthorisationManager.CheckAuthorisation(token, attribute))
            {
                var unauthorised = _container.GetInstance<IHttpResponse>(true);
                unauthorised.HttpStatusCode = HttpStatusCode.Unauthorized;
                return unauthorised;
            }

            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "Handler Handle called"));
            timer.Restart();

            try
            {
                handler.Handle(request, response);
            }
            catch(Exception exception)
            {
                logger.Exception($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", "Exception caught handling request", exception);

                response.SetStringContent(string.Empty);
                response.HttpStatusCode = HttpStatusCode.InternalServerError;
            }

            var elapsedMilliseconds = timer.Elapsed.TotalMilliseconds;
            logger.Trace($"{nameof(RequestBroker)}.{nameof(HandleRequest)}", new LogItem("Event", "Handler Handle completed"), new LogItem("DurationMilliseconds", elapsedMilliseconds));
            apiMetrics.RecordHandlerDuration(elapsedMilliseconds, handlerName, response.HttpStatusCode);
            
            return response;
        }

        public int Precedence { get; } = 1;

        private IActivityHandler GetRequestHandler(IHttpRequest request, out ActivityHandlerAttribute activityHandler)
        {
            var baseUrlParts = _configurationManager.Get("BaseUrl", "/").ToLowerInvariant().Split('/').Where(x => x != string.Empty).ToArray();

            activityHandler = null;
            bool Predicate(ActivityHandlerAttribute attr) => RequestPathMatchesAttributePath(request, attr, baseUrlParts);

            IDictionary<Type, ActivityHandlerAttribute[]> handlers = activityHandlers ??
                                                                     (activityHandlers =
                                                                         _container.GetAttributedTypes<ActivityHandlerAttribute>(
                                                                             typeof(IActivityHandler)));

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

            var path = attribute.Path;
            if (baseUrlParts.Length > 0)
                path = $"/{string.Join("/",baseUrlParts)}{path}";

            request.SetUriPattern(path);

            IActivityHandler instance = (IActivityHandler) _container.GetInstance(handlerInfo.Key);

            return instance;
        }

        private bool RequestPathMatchesAttributePath(IHttpRequest request, ActivityHandlerAttribute attribute, string[] baseUrlParts)
        {
            if (request.Verb != attribute.Verb) return false;

            string pattern = attribute.Path;
            string path = request.Path;

            if (baseUrlParts.Length > 0)
                pattern = $"/{string.Join("/", baseUrlParts)}{pattern}";

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
                bool replaceable = tokens[i].StartsWith("{") && tokens[i].EndsWith("}");
                if (!replaceable && tokens[i] != values[i])
                    return false;
            }

            return true;
        }
    }
}