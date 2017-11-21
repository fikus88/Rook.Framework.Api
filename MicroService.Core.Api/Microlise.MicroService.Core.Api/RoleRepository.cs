using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.IoC;

namespace Microlise.MicroService.Core.Api
{
    internal class RoleRepository:IRoleRepository
    {
        internal readonly AutoDictionary<string, IEnumerable<string>> ActivityRoles = new AutoDictionary<string, IEnumerable<string>>();

        private readonly bool requiresAuthorisation;

        public RoleRepository(IConfigurationManager config)
        {
            requiresAuthorisation = string.Equals(config.AppSettings["RequiresAuthorisation"], "true", StringComparison.OrdinalIgnoreCase);
        }

        public bool CheckAuthorisation(JwtSecurityToken token, ActivityHandlerAttribute attribute)
        {
            if (!requiresAuthorisation) return true;
            string[] usersRoles = token.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToArray();
            return ActivityRoles.ContainsKey(attribute.ActivityName) &&
                   ActivityRoles[attribute.ActivityName].Any(usersRoles.Contains);
        }

        public void Add(string activity, string[] roles)
        {
            ActivityRoles.Add(activity,roles);
        }
    }
}