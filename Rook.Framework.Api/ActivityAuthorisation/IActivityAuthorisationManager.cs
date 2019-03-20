using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Rook.Framework.Core.Common;

namespace Rook.Framework.Api.ActivityAuthorisation
{
    public interface IActivityAuthorisationManager
    {
        bool CheckAuthorisation(JwtSecurityToken token, ActivityHandlerAttribute attribute);
        AutoDictionary<string, IEnumerable<string>> ActivityRoles { get; }
        void Initialise();
    }
}