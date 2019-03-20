using System;
using System.Collections.Generic;
using System.Linq;
using Rook.Framework.Core.HttpServer;
using Rook.Framework.Core.StructureMap;

namespace Rook.Framework.Api.BuiltInActivityHandlers
{
    [ActivityHandler("GetApiDescription", HttpVerb.Get, "description", "Describes the API based on descriptions provided in the ActivityHandlerAttribute constructor for each VerbHandler class", SkipAuthorisation = true)]
    internal class GetDescription : IActivityHandler
    {
        private readonly IContainerFacade _container;

        public GetDescription(IContainerFacade container)
        {
            _container = container;
        }

        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            Dictionary<Type, ActivityHandlerAttribute[]> handlers = _container.GetAttributedTypes<ActivityHandlerAttribute>(typeof(IActivityHandler));
            List<object> handlerInfoList = new List<object>();

            foreach (KeyValuePair<Type, ActivityHandlerAttribute[]> keyValuePair in handlers)
            {
                foreach (ActivityHandlerAttribute attribute in keyValuePair.Value)
                {
                    var instance = (IActivityHandler)_container.GetInstance(keyValuePair.Key);
                    handlerInfoList.Add(new
                    {
                        path = attribute.Path,
                        verb = attribute.Verb.ToString(),
                        description = attribute.Description,
                        expectedParameters = attribute.ExpectedParameters,
                        exampleCall =
                        $"{attribute.Path}{(attribute.ExpectedParameters.Any() ? "?" : "")}{string.Join("&", attribute.ExpectedParameters.Select(a => a + "=<value>"))}",
                        exampleRequestDocument = instance.ExampleRequestDocument,
                        exampleResponseDocument = instance.ExampleResponseDocument,
                    });
                }
            }
            response.SetObjectContent(handlerInfoList);
        }

        public dynamic ExampleRequestDocument { get; } = null;
        public dynamic ExampleResponseDocument { get; } = new dynamic[] {
            new{path="apath/{someValue}",verb="Eat",description="The Description",expectedParameters=new string[]{"hello","world"},exampleCall="apath/{someValue}?hello=<value>&world=<value>",exampleRequestDocument=new{},exampleResponseDocument=new{}},
            new{path="{someOtherValue}/something",verb="Drink",description="Another Description",expectedParameters=new string[]{},exampleCall="{someOtherValue}/something",exampleRequestDocument=new{fish="Haddock",banana=true},exampleResponseDocument=new{id=Guid.NewGuid(),fish="Haddock",banana=true,createdAt=DateTime.Now.ToString("s")}}
        };
    }
}