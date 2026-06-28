using Application.Constants;
using Application.Contracts.Auth.Requests;
using Application.Services.Auth;
using Application.Services.Token;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Test.Shared.Mocks;

namespace Application.Tests.Auth.Tests
{
    public class SignInServiceTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly IConfiguration _configuration;
        private readonly SignInService _signInService;

        public SignInServiceTest()
        {
            _userManagerMock = MockUserManager.Create();
            _signInManagerMock = MockSignInManager.Create(_userManagerMock);
            _tokenServiceMock = new Mock<ITokenService>();

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:RefreshTokenExpiryDays"] = "7",
                })
                .Build();

            _signInService = new SignInService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _configuration
            );
        }

        private SignInRequest CreateValidRequest()
            => new() { Email = "abc@gmail.com", Password = "123456" };

        private User SetupUserExist(SignInRequest request)
        {
            var user = new User { Id = Guid.NewGuid(), Email = request.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
            return user;
        }

        private void SetupSuccessfulSignIn(User user, SignInRequest request)
        {
            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
                .ReturnsAsync(SignInResult.Success);
            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(user))
                .ReturnsAsync("mocked_token");
            _tokenServiceMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("mocked_refresh_token");
            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);
        }

        // ── Happy Path ──

        [Fact]
        public async Task SignInAsync_ValidCredentials_ReturnsSuccessWithTokens()
        {
            // Arrange
            var request = CreateValidRequest();
            var user = SetupUserExist(request);
            SetupSuccessfulSignIn(user, request);

            // Act
            var result = await _signInService.SignInAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("mocked_token", result.Token);
            Assert.Equal("mocked_refresh_token", result.RefreshToken);
        }

        // ── User Not Found ──

        [Fact]
        public async Task SignInAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var request = CreateValidRequest();
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _signInService.SignInAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
            _signInManagerMock.Verify(
                x => x.CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false),
                Times.Never);
        }

        // ── Invalid Password ──

        [Fact]
        public async Task SignInAsync_InvalidPassword_ReturnsFailure()
        {
            // Arrange
            var request = CreateValidRequest();
            var user = SetupUserExist(request);
            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _signInService.SignInAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Auth.InvalidPassword, result.ErrorCode);
            _tokenServiceMock.Verify(x => x.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        }

        // ── Update User Failed ──

        [Fact]
        public async Task SignInAsync_UpdateUserFails_ReturnsFailure()
        {
            // Arrange
            var request = CreateValidRequest();
            var user = SetupUserExist(request);
            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
                .ReturnsAsync(SignInResult.Success);
            _tokenServiceMock.Setup(x => x.GenerateAccessToken(user)).ReturnsAsync("token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");
            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "DB error" }));

            // Act
            var result = await _signInService.SignInAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UpdateFailed, result.ErrorCode);
            Assert.Contains("DB error", result.ErrorMessage);
        }

        // ── Verify RefreshToken Saved ──

        [Fact]
        public async Task SignInAsync_Success_SetsRefreshTokenOnUser()
        {
            // Arrange
            var request = CreateValidRequest();
            var user = SetupUserExist(request);
            SetupSuccessfulSignIn(user, request);

            // Act
            await _signInService.SignInAsync(request);

            // Assert
            Assert.Equal("mocked_refresh_token", user.RefreshToken);
            Assert.True(user.IsActive);
            Assert.NotNull(user.RefreshTokenExpiryTime);
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }
    }
}
