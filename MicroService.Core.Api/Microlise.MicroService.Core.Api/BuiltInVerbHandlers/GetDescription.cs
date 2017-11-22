using System;
using System.Collections.Generic;
using System.Net;
using Microlise.MicroService.Core.IoC;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api.BuiltInVerbHandlers
{
    [ActivityHandler("GetApiDescription", HttpVerb.Get, "description", "Describes the API based on descriptions provided in the ActivityHandlerAttribute constructor for each VerbHandler class")]
    internal class GetDescription : IActivityHandler
    {
        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            Dictionary<Type, ActivityHandlerAttribute[]> handlers = Container.FindAttributedTypes<ActivityHandlerAttribute>();
            List<object> handlerInfoList = new List<object>();
            
            foreach (KeyValuePair<Type, ActivityHandlerAttribute[]> keyValuePair in handlers)
            {
                foreach (ActivityHandlerAttribute attribute in keyValuePair.Value)
                {
                    handlerInfoList.Add(new
                    {
                        path = attribute.Path,
                        verb = attribute.Verb.ToString(),
                        description = attribute.Description
                    });
                }
            }
            response.SetObjectContent(handlerInfoList);
        }
    }
}