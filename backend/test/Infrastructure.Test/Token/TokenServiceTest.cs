using Domain.Constants;
using Domain.Cores.Identity;
using Infrastructure.Services.Token;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Test.Shared.Mocks;

namespace Infrastructure.Test.Token
{
    public class TokenServiceTest
    {
        private readonly TokenService _tokenService;
        private readonly Mock<Microsoft.AspNetCore.Identity.UserManager<User>> _userManagerMock;

        private static IConfiguration CreateConfig(Dictionary<string, string?>? overrides = null)
        {
            var defaults = new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "Kai_Secret32131236687dsabdkjasbduiy87y87sgdhjkbakdqi387178",
                ["JwtSettings:Issuer"] = "Kai_Issuer",
                ["JwtSettings:Audience"] = "Kai_Audience",
                ["JwtSettings:RefreshTokenExpiryDays"] = "7",
            };

            if (overrides != null)
            {
                foreach (var kv in overrides)
                    defaults[kv.Key] = kv.Value;
            }

            return new ConfigurationBuilder().AddInMemoryCollection(defaults).Build();
        }

        private static User CreateTestUser(string role = Roles.User) => new()
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Avatar = "avatar.png",
            Balance = 1000,
        };

        public TokenServiceTest()
        {
            _userManagerMock = MockUserManager.Create();
            _tokenService = new TokenService(CreateConfig(), _userManagerMock.Object);
        }

        // ── GenerateAccessToken ──

        [Fact]
        public async Task GenerateAccessToken_WithValidUser_ReturnsValidJwt()
        {
            // Arrange
            var user = CreateTestUser();
            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.User });

            // Act
            var token = await _tokenService.GenerateAccessToken(user);

            // Assert
            Assert.NotEmpty(token);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.Equal("Kai_Issuer", jwt.Issuer);
            Assert.Contains("Kai_Audience", jwt.Audiences);
            Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == UserClaims.Email).Value);
        }

        [Fact]
        public async Task GenerateAccessToken_AdminRole_ContainsAllPermissions()
        {
            // Arrange
            var user = CreateTestUser();
            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Admin });

            // Act
            var token = await _tokenService.GenerateAccessToken(user);

            // Assert
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var permissions = jwt.Claims.First(c => c.Type == UserClaims.Permissions).Value;
            foreach (var p in typeof(Permissions).GetAllPermissionValues())
            {
                Assert.Contains(p, permissions);
            }
        }

        [Fact]
        public async Task GenerateAccessToken_UserRole_ContainsPublicReaderPermissions()
        {
            // Arrange
            var user = CreateTestUser();
            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.User });

            // Act
            var token = await _tokenService.GenerateAccessToken(user);

            // Assert
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var permissions = jwt.Claims.First(c => c.Type == UserClaims.Permissions).Value;
            foreach (var p in RolePermissions.PublicReader)
            {
                Assert.Contains(p, permissions);
            }
        }

        [Fact]
        public async Task GenerateAccessToken_MissingSecret_ThrowsInvalidOperation()
        {
            // Arrange
            var config = CreateConfig(new Dictionary<string, string?> { ["JwtSettings:Secret"] = null });
            var service = new TokenService(config, _userManagerMock.Object);
            var user = CreateTestUser();
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateAccessToken(user));
        }

        // ── GenerateRefreshToken ──

        [Fact]
        public void GenerateRefreshToken_ReturnsBase64StringOf64Bytes()
        {
            // Act
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Assert
            Assert.NotEmpty(refreshToken);
            Assert.Equal(64, Convert.FromBase64String(refreshToken).Length);
        }

        [Fact]
        public void GenerateRefreshToken_ReturnsDifferentTokensEachCall()
        {
            // Act
            var token1 = _tokenService.GenerateRefreshToken();
            var token2 = _tokenService.GenerateRefreshToken();

            // Assert
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GenerateRefreshToken_ExpiryDaysZero_ThrowsInvalidOperation()
        {
            // Arrange
            var config = CreateConfig(new Dictionary<string, string?> { ["JwtSettings:RefreshTokenExpiryDays"] = "0" });
            var service = new TokenService(config, _userManagerMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.GenerateRefreshToken());
        }
    }
}
