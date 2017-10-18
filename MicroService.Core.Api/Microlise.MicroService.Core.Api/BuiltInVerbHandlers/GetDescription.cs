using System;
using System.Collections.Generic;
using System.Net;
using Microlise.MicroService.Core.IoC;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api.BuiltInVerbHandlers
{
	[VerbHandler(HttpVerb.Get, "description", "Describes the API based on descriptions provided in the VerbHandlerAttribute constructor for each VerbHandler class")]
	internal class GetDescription : IVerbHandler
	{
		public HttpStatusCode Handle(HttpRequest request, out object responseData)
		{
			Dictionary<Type, VerbHandlerAttribute[]> handlers = Container.FindAttributedTypes<VerbHandlerAttribute>();
			List<object> handlerInfoList = new List<object>();
			foreach (KeyValuePair<Type, VerbHandlerAttribute[]> keyValuePair in handlers)
			{
				foreach (VerbHandlerAttribute attribute in keyValuePair.Value)
				{
					handlerInfoList.Add(new
					{
						path = attribute.Path,
						verb = attribute.Verb.ToString(),
						description = attribute.Description
					});
				}
			}
			responseData = handlerInfoList;
			return HttpStatusCode.OK;
		}
	}
}