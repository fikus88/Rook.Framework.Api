using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Rook.Framework.Core.Application.MessageHandlers;
using Rook.Framework.Core.Backplane;
using Rook.Framework.Core.HttpServer;
using Rook.Framework.Api.ActivityAuthorisation;
using Rook.Framework.Api.ActivityAuthorisation.MessageHandlers;
using Rook.Framework.Api.AspNetHttp;
using Rook.Framework.Api.BuiltInActivityHandlers;
using Rook.Framework.Core.Common;
using Rook.Framework.Core.HttpServerAspNet;
using Rook.Framework.Core.Services;
using Rook.Framework.Core.StructureMap.Registries;
using StructureMap;

namespace Rook.Framework.Api.StructureMap
{
    public class ApiRegistry : Registry
    {
        public ApiRegistry()
        {
            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });

            var configurationContainer = new Container(new ConfigurationRegistry());
            var configurationManager = configurationContainer.GetInstance<IConfigurationManager>();

            if (!Enum.TryParse(configurationManager.Get<string>("HttpServerType", "AspNetHttp"), out HttpServerType httpServerType))
            {
	            throw new ArgumentException("Invalid value provided for HttpServerType in configuration");
            }

			For<IRequestBroker>().Singleton().Add<RequestBroker>();
			For<IMessageHandler2<ActivityRoleNeed, string>>().Use<AuthoriseForbidRoleMessageHandler>();
            For<IMessageHandler2<string, string>>().Use<RoleMessageHandler>();
            For<IActivityHandler>().Use<GetDescription>();
            For<IActivityHandler>().Use<GetFavIcon>();
            For<IActivityHandler>().Use<GetVersion>();
            For<IActivityHandler>().Use<OptionsActivityHandler>();

            switch (httpServerType)
            {
	            case HttpServerType.NanoHttp:
					For<IActivityAuthorisationManager>().Singleton().Use<ActivityAuthorisationManager>();
		            break;
	            default:
		            For<IActivityAuthorisationManager>().Singleton().Use<AspNetActivityAuthorisationManager>();
					break;
            }

            For<IBackplaneConsumer>().Add<ActivityRoleBackplaneConsumer>();
            For<ActivityRoles>().Use<ActivityRoles>();
        }
    }
}
