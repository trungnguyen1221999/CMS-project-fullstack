using Application.Constants;
using Application.Contracts.Users.Requests;
using Microsoft.AspNetCore.Identity;
using Moq;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        private static ChangeEmailRequest CreateChangeEmailRequest(string email = "new@test.com")
            => new() { Email = email };

        private void SetupGenerateChangeEmailToken(AppUser user, string email, string token) =>
            _userManagerMock
                .Setup(x => x.GenerateChangeEmailTokenAsync(user, email))
                .ReturnsAsync(token);

        private void SetupChangeEmail(AppUser user, string email, string token, IdentityResult result) =>
            _userManagerMock
                .Setup(x => x.ChangeEmailAsync(user, email, token))
                .ReturnsAsync(result);

        [Fact]
        public async Task ChangeEmailAsync_UserNotFound_ReturnFailure()
        {
            //Arrange
            var id = Guid.NewGuid();
            var request = CreateChangeEmailRequest();
            SetupFindUser(id, null);

            //Act
            var result = await _userService.ChangeEmailAsync(id, request);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
            _userManagerMock.Verify(
                x => x.GenerateChangeEmailTokenAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task ChangeEmailAsync_ChangeEmailFailed_ReturnFailure()
        {
            //Arrange
            var id = Guid.NewGuid();
            var user = CreateUserForSetPassword(id);
            var request = CreateChangeEmailRequest();
            var token = "email-change-token";
            SetupFindUser(id, user);
            SetupGenerateChangeEmailToken(user, request.Email, token);
            SetupChangeEmail(user, request.Email, token,
                IdentityResult.Failed(new IdentityError { Description = "Email change error" })
            );

            //Act
            var result = await _userService.ChangeEmailAsync(id, request);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.ChangeEmailFailed, result.ErrorCode);
            Assert.Contains("Email change error", result.ErrorMessage);
        }

        [Fact]
        public async Task ChangeEmailAsync_ValidRequest_ReturnSuccess()
        {
            //Arrange
            var id = Guid.NewGuid();
            var user = CreateUserForSetPassword(id);
            var request = CreateChangeEmailRequest();
            var token = "email-change-token";
            SetupFindUser(id, user);
            SetupGenerateChangeEmailToken(user, request.Email, token);
            SetupChangeEmail(user, request.Email, token, IdentityResult.Success);

            //Act
            var result = await _userService.ChangeEmailAsync(id, request);

            //Assert
            Assert.True(result.IsSuccess);
            _userManagerMock.Verify(
                x => x.ChangeEmailAsync(user, request.Email, token),
                Times.Once
            );
        }
    }
}
