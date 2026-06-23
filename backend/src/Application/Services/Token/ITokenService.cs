namespace Application.Services.Token
{
    public interface ITokenService
    {
        string GenerateAccessToken();
        string GenerateRefreshToken();
    }
}
