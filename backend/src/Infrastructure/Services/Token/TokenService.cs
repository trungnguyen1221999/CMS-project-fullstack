using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Services.Token;
using Domain.Constants;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;

        public TokenService(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<string> GenerateAccessToken(User user)
        {
            var token = new JwtSecurityToken(
                issuer: GetRequiredJwtValue("Issuer"),
                audience: GetRequiredJwtValue("Audience"),
                claims: await CreateClaimsAsync(user),
                expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                signingCredentials: CreateSigningCredentials()
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private SigningCredentials CreateSigningCredentials()
        {
            var keyBytes = Encoding.UTF8.GetBytes(GetRequiredJwtValue("Secret"));
            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:Secret must be at least 32 bytes for HS256."
                );
            }

            return new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256
            );
        }

        private string GetRequiredJwtValue(string key)
        {
            return _configuration[$"JwtSettings:{key}"]
                ?? throw new InvalidOperationException($"Missing config: JwtSettings:{key}");
        }

        private int GetAccessTokenExpiryMinutes()
        {
            var expiryMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiryMinutes");
            return expiryMinutes > 0 ? expiryMinutes : 15;
        }

        private async Task<List<Claim>> CreateClaimsAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var rolePermissionsMapping = new Dictionary<string, HashSet<string>>
            {
                [Roles.Author] = new HashSet<string>(RolePermissions.PublicReader)
                    .Union(RolePermissions.Writing)
                    .ToHashSet(),
                [Roles.Editor] = new HashSet<string>(RolePermissions.PublicReader)
                    .Union(RolePermissions.Editing)
                    .ToHashSet(),
                [Roles.Admin] = typeof(Permissions).GetAllPermissionValues().ToHashSet(),
                [Roles.User] = new HashSet<string>(RolePermissions.PublicReader),
            };

            var aggregatedPermissions = new HashSet<string>();

            foreach (var role in roles)
            {
                if (rolePermissionsMapping.TryGetValue(role, out var allowedPermissions))
                {
                    aggregatedPermissions.UnionWith(allowedPermissions);
                }
            }

            return new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(UserClaims.Roles, string.Join(",", roles)),
                new Claim(UserClaims.Id, user.Id.ToString()),
                new Claim(UserClaims.FirstName, user.FirstName ?? string.Empty),
                new Claim(UserClaims.LastName, user.LastName ?? string.Empty),
                new Claim(UserClaims.Email, user.Email ?? string.Empty),
                new Claim(UserClaims.Avatar, user.Avatar ?? string.Empty),
                new Claim(UserClaims.Balance, user.Balance.ToString()),
                new Claim(UserClaims.Permissions, string.Join(",", aggregatedPermissions)),
            };
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
