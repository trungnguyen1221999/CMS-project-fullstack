using Application.Constants;
using Application.Contracts.Auth.Requests;
using Application.Services.Auth;
using Application.Services.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Test.Shared.Mocks;
using static Application.Exceptions.CustomException;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Auth.Tests
{
    public class SignInServiceTest
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<SignInManager<AppUser>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly IConfiguration _configuration;
        private readonly SignInService _signInService;

        public SignInServiceTest()
        {
            _userManagerMock = MockUserManager.Create();
            _signInManagerMock = MockSignInManager.Create(_userManagerMock);
            _tokenServiceMock = new Mock<ITokenService>();

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?> { ["JwtSettings:RefreshTokenExpiryDays"] = "7" }
                )
                .Build();

            _signInService = new SignInService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _configuration
            );
        }

        private SignInRequest CreateValidRequest() =>
            new() { Email = "abc@gmail.com", Password = "123456" };

        private AppUser SetupUserExist(SignInRequest request)
        {
            var user = new AppUser { Id = Guid.NewGuid(), Email = request.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
            return user;
        }

        private void SetupSuccessfulSignIn(AppUser user, SignInRequest request)
        {
            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
                .ReturnsAsync(SignInResult.Success);
            _tokenServiceMock.Setup(x => x.GenerateAccessToken(user)).ReturnsAsync("mocked_token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("mocked_refresh_token");
            _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        }

        [Fact]
        public async Task SignInAsync_ValidCredentials_ReturnsSuccessWithTokens()
        {
            var request = CreateValidRequest();
            var user = SetupUserExist(request);
            SetupSuccessfulSignIn(user, request);

            var result = await _signInService.SignInAsync(request);

            Assert.Equal("mocked_token", result.Token);
            Assert.Equal("mocked_refresh_token", result.RefreshToken);
        }

        [Fact]
        public async Task SignInAsync_UserNotFound_ReturnsFailure()
        {
            var request = CreateValidRequest();
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _signInService.SignInAsync(request)
            );
            Assert.Equal(ErrorMessages.User.UserNotFound, ex.ErrorCode);
            _signInManagerMock.Verify(
                x => x.CheckPasswordSignInAsync(It.IsAny<AppUser>(), It.IsAny<string>(), false),
                Times.Never
            );
        }

        [Fact]
        public async Task SignInAsync_InvalidPassword_ReturnsFailure()
        {
            var request = CreateValidRequest();
            var user = SetupUserExist(request);
            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
                .ReturnsAsync(SignInResult.Failed);

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _signInService.SignInAsync(request)
            );
            Assert.Equal(ErrorMessages.Auth.InvalidPassword, ex.ErrorCode);
            _tokenServiceMock.Verify(x => x.GenerateAccessToken(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task SignInAsync_UpdateUserFails_ReturnsFailure()
        {
            var request = CreateValidRequest();
            var user = SetupUserExist(request);
            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false))
                .ReturnsAsync(SignInResult.Success);
            _tokenServiceMock.Setup(x => x.GenerateAccessToken(user)).ReturnsAsync("token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");
            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(
                    IdentityResult.Failed(new IdentityError { Description = "DB error" })
                );

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _signInService.SignInAsync(request)
            );
            Assert.Equal(ErrorMessages.User.UpdateFailed, ex.ErrorCode);
        }

        [Fact]
        public async Task SignInAsync_Success_SetsRefreshTokenOnUser()
        {
            var request = CreateValidRequest();
            var user = SetupUserExist(request);
            SetupSuccessfulSignIn(user, request);

            await _signInService.SignInAsync(request);

            Assert.Equal("mocked_refresh_token", user.RefreshToken);
            Assert.True(user.IsActive);
            Assert.NotNull(user.RefreshTokenExpiryTime);
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }
    }
}
