using System.IdentityModel.Tokens.Jwt;

namespace Microlise.MicroService.Core.Api
{
    internal interface IActivityAuthorisationManager
    {
        bool CheckAuthorisation(JwtSecurityToken token, ActivityHandlerAttribute attribute);
    }
}