using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microlise.MicroService.Core.Common;

namespace Microlise.MicroService.Core.Api.ActivityAuthorisation {
    internal interface IActivityAuthorisationManager
    {
        bool CheckAuthorisation(JwtSecurityToken token, ActivityHandlerAttribute attribute);
        AutoDictionary<string, IEnumerable<string>> ActivityRoles { get; }
    }
}