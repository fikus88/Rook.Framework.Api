using System.IdentityModel.Tokens.Jwt;

namespace Microlise.MicroService.Core.Api
{
    internal interface IRoleRepository
    {
        bool CheckAuthorisation(JwtSecurityToken token, ActivityHandlerAttribute attribute);
        void Add(string activity, string[] roles);
    }
}