using AppUser = Domain.Cores.Identity.User;

namespace Application.Services.Token
{
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(AppUser user);
        string GenerateRefreshToken();
    }
}
