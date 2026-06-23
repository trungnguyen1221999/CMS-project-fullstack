using Domain.Cores.Identity;

namespace Application.Services.Token
{
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
