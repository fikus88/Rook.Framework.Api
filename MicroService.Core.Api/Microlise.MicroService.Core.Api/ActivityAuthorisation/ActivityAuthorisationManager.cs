﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microlise.MicroService.Core.Application.Bus;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.IoC;

namespace Microlise.MicroService.Core.Api.ActivityAuthorisation
{
    internal class ActivityAuthorisationManager : IActivityAuthorisationManager
    {
        private readonly IQueueWrapper queue;
        public AutoDictionary<string, IEnumerable<string>> ActivityRoles { get; } = new AutoDictionary<string, IEnumerable<string>>();

        private readonly bool requiresAuthorisation;

        public ActivityAuthorisationManager(IQueueWrapper queueWrapper, IConfigurationManager config)
        {
            queue = queueWrapper;
            requiresAuthorisation = string.Equals(config.AppSettings["RequiresAuthorisation"], "true", StringComparison.OrdinalIgnoreCase);
        }

        public void Initialise()
        {
            // Get all the Activities
            Dictionary<Type, ActivityHandlerAttribute[]> attributes = Container.FindAttributedTypes<ActivityHandlerAttribute>();

            // Send a "Register" message on the bus for each activity
            foreach (KeyValuePair<Type, ActivityHandlerAttribute[]> keyValuePair in attributes)
                foreach (ActivityHandlerAttribute attribute in keyValuePair.Value.Where(a => a.ActivityName != null && a.SkipAuthorisation == false))
                    queue.PublishMessage(new Message<string, string>
                    {
                        Method = "RegisterActivityForAuthorisation",
                        Need = attribute.ActivityName
                    });
        }

        public bool CheckAuthorisation(JwtSecurityToken token, ActivityHandlerAttribute attribute)
        {
            if (!requiresAuthorisation || attribute.SkipAuthorisation) return true;
            if (token == null) return false;
            string[] usersRoles = token.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToArray();
            return ActivityRoles.ContainsKey(attribute.ActivityName) &&
                   ActivityRoles[attribute.ActivityName].Any(usersRoles.Contains);
        }
    }
}