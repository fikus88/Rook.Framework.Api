using Microlise.MicroService.Core.Api.ActivityAuthorisation;
using Microlise.MicroService.Core.Api.ActivityAuthorisation.MessageHandlers;
using Microlise.MicroService.Core.Api.BuiltInActivityHandlers;
using Microlise.MicroService.Core.Application.MessageHandlers;
using Microlise.MicroService.Core.Backplane;
using Microlise.MicroService.Core.HttpServer;
using StructureMap;

namespace Microlise.MicroService.Core.Api.StructureMap
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

            For<IRequestBroker>().Singleton().Add<RequestBroker>();

            For<IMessageHandler2<ActivityRoleNeed, string>>().Use<AuthoriseForbidRoleMessageHandler>();
            For<IMessageHandler2<string, string>>().Use<RoleMessageHandler>();
            For<IActivityHandler>().Use<GetDescription>();
            For<IActivityHandler>().Use<GetFavIcon>();
            For<IActivityHandler>().Use<GetVersion>();
            For<IActivityHandler>().Use<OptionsActivityHandler>();

            For<IActivityAuthorisationManager>().Singleton().Use<ActivityAuthorisationManager>();
            For<IBackplaneConsumer>().Add<ActivityRoleBackplaneConsumer>();
            For<ActivityRoles>().Use<ActivityRoles>();
        }
    }
}
