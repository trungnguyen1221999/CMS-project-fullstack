using Application.Constants;
using Application.Contracts.Users.Requests;
using Microsoft.AspNetCore.Identity;
using Moq;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        private static SetPasswordRequest CreateSetPasswordRequest(
            string password = "NewPassword123!"
        ) => new() { NewPassword = password };

        private static AppUser CreateUserForSetPassword(Guid? id = null) =>
            new() { Id = id ?? Guid.NewGuid(), Email = "test@test.com" };

        private void SetupFindUser(Guid userId, AppUser? user) =>
            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        private void SetupCheckPassword(AppUser user, string password, bool isSame) =>
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password)).ReturnsAsync(isSame);

        private void SetupResetPassword(AppUser user, string token, string newPassword, IdentityResult result) =>
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, token, newPassword)).ReturnsAsync(result);

        private void SetupGenerateResetToken(AppUser user, string token) =>
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync(token);

        [Fact]
        public async Task SetPasswordAsync_UserNotFound_ReturnFailure()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var request = CreateSetPasswordRequest();
            SetupFindUser(userId, null);

            //Act
            var result = await _userService.SetPasswordAsync(userId, request);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
            _userManagerMock.Verify(
                x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
                Times.Never
            );
            _userManagerMock.Verify(
                x => x.ResetPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SetPasswordAsync_NewPasswordSameAsCurrent_ReturnFailure()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var user = CreateUserForSetPassword(userId);
            var request = CreateSetPasswordRequest();
            SetupFindUser(userId, user);
            SetupCheckPassword(user, request.NewPassword, true);

            //Act
            var result = await _userService.SetPasswordAsync(userId, request);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(
                ErrorMessages.User.ChangePassword.NewPasswordSameAsCurrent,
                result.ErrorCode
            );
            _userManagerMock.Verify(
                x => x.ResetPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SetPasswordAsync_ResetFailed_ReturnSetPasswordFailed()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var user = CreateUserForSetPassword(userId);
            var request = CreateSetPasswordRequest();
            var token = "reset-token";
            SetupFindUser(userId, user);
            SetupCheckPassword(user, request.NewPassword, false);
            SetupGenerateResetToken(user, token);
            SetupResetPassword(user, token, request.NewPassword,
                IdentityResult.Failed(new IdentityError { Description = "Reset error" })
            );

            //Act
            var result = await _userService.SetPasswordAsync(userId, request);

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.SetPasswordFailed, result.ErrorCode);
            Assert.Contains("Reset error", result.ErrorMessage);
        }

        [Fact]
        public async Task SetPasswordAsync_ValidRequest_ReturnSuccess()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var user = CreateUserForSetPassword(userId);
            var request = CreateSetPasswordRequest();
            var token = "reset-token";
            SetupFindUser(userId, user);
            SetupCheckPassword(user, request.NewPassword, false);
            SetupGenerateResetToken(user, token);
            SetupResetPassword(user, token, request.NewPassword, IdentityResult.Success);

            //Act
            var result = await _userService.SetPasswordAsync(userId, request);

            //Assert
            Assert.True(result.IsSuccess);
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, token, request.NewPassword), Times.Once);
        }
    }
}
