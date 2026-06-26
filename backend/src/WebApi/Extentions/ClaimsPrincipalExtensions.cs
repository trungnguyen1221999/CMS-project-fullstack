using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Constants;

namespace WebApi.Extentions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            var claim =
                principal.FindFirst(UserClaims.Id)
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? principal.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                return Guid.Empty;
            return Guid.Parse(claim.Value);
        }
    }
}
