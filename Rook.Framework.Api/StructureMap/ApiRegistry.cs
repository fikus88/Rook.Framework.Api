using Rook.Framework.Core.Application.MessageHandlers;
using Rook.Framework.Core.Backplane;
using Rook.Framework.Core.HttpServer;
using Rook.Framework.Api.ActivityAuthorisation;
using Rook.Framework.Api.ActivityAuthorisation.MessageHandlers;
using Rook.Framework.Api.BuiltInActivityHandlers;
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
