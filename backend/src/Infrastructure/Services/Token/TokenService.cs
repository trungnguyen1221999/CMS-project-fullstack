using System.Security.Cryptography;
using Application.Services.Token;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken()
        {
            throw new NotImplementedException();
        }

        public string GenerateRefreshToken()
        {
            var expiryDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpiryDays");
            if (expiryDays <= 0)
            {
                throw new InvalidOperationException(
                    "RefreshTokenExpiryDays must be greater than zero."
                );
            }
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
