using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Rook.Framework.Core.Application.Bus;
using Rook.Framework.Core.Application.Message;
using Rook.Framework.Core.Common;
using Rook.Framework.Core.HttpServer;
using Rook.Framework.Core.StructureMap;

namespace Rook.Framework.Core.Api.ActivityAuthorisation
{
    internal class ActivityAuthorisationManager : IActivityAuthorisationManager
    {
        private readonly IQueueWrapper queue;
        private readonly IContainerFacade _container;
        public AutoDictionary<string, IEnumerable<string>> ActivityRoles { get; } = new AutoDictionary<string, IEnumerable<string>>();

        private readonly bool _requiresAuthorisation;

        public ActivityAuthorisationManager(IQueueWrapper queueWrapper, IConfigurationManager config, IContainerFacade container)
        {
            queue = queueWrapper;
            _container = container;
            _requiresAuthorisation = config.Get("RequiresAuthorisation", true);
        }

        public void Initialise()
        {
            Dictionary<Type, ActivityHandlerAttribute[]> attributes = _container.GetAttributedTypes<ActivityHandlerAttribute>(typeof(IActivityHandler));

            // Send a "Register" message on the bus for each activity
            foreach (KeyValuePair<Type, ActivityHandlerAttribute[]> keyValuePair in attributes)
                foreach (ActivityHandlerAttribute attribute in keyValuePair.Value.Where(a => a.ActivityName != null && !a.SkipAuthorisation))
                    queue.PublishMessage(new Message<string, string>
                    {
                        Method = "RegisterActivityForAuthorisation",
                        Need = attribute.ActivityName
                    });
        }

        public bool CheckAuthorisation(JwtSecurityToken token, ActivityHandlerAttribute attribute)
        {
            if (!_requiresAuthorisation || attribute.SkipAuthorisation) return true;
            if (token == null) return false;
            string[] usersRoles = token.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToArray();
            return ActivityRoles.ContainsKey(attribute.ActivityName) &&
                   ActivityRoles[attribute.ActivityName].Any(usersRoles.Contains);
        }
    }
}