using System.IdentityModel.Tokens.Jwt;

namespace Microlise.MicroService.Core.Api.MessageHandlers {
    internal interface IActivityAuthorisationManager { bool CheckAuthorisation(JwtSecurityToken token, ActivityHandlerAttribute attribute); }
}