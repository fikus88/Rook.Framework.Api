using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using Rook.Framework.Api.AspNetHttp;
using Rook.Framework.Core.Application.Bus;
using Rook.Framework.Core.Application.Message;
using Rook.Framework.Core.Common;
using Rook.Framework.Core.HttpServer;
using Rook.Framework.Core.StructureMap;

namespace Rook.Framework.Api.ActivityAuthorisation
{
    internal class AspNetActivityAuthorisationManager : IActivityAuthorisationManager
    {
        private readonly IQueueWrapper queue;
        public AutoDictionary<string, IEnumerable<string>> ActivityRoles { get; } = new AutoDictionary<string, IEnumerable<string>>();

        public AspNetActivityAuthorisationManager(IQueueWrapper queueWrapper)
        {
            queue = queueWrapper;
        }

        public void Initialise()
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly(), Assembly.GetEntryAssembly() };

			var authorizeActivityAttributes = assemblies
				.SelectMany(x => x.GetTypes())
				.SelectMany(x => x.GetMethods())
				.SelectMany(x => x.GetCustomAttributes<AuthorizeActivityAttribute>(true))
				.ToArray();
			
			foreach (var attribute in authorizeActivityAttributes)
            {
	            queue.PublishMessage(new Message<string, string>
	            {
		            Method = "RegisterActivityForAuthorisation",
		            Need = attribute.ActivityName
	            });
			}
        }

        public bool CheckAuthorisation(JwtSecurityToken token, ActivityHandlerAttribute attribute)
        {
            throw new InvalidOperationException("Authorisation checking is performed by ASP.NET Core.");
        }
    }
}