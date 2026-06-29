using Application.Constants;
using Application.Contracts.Users.Requests;
using Microsoft.AspNetCore.Identity;
using Moq;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        private static ChangeMyPasswordRequest CreateChangePasswordRequest() =>
            new()
            {
                CurrentPassword = "OldPass123",
                NewPassword = "NewPass456",
                ConfirmNewPassword = "NewPass456",
            };

        // ── ChangeMyPasswordAsync: user not found ──

        [Fact]
        public async Task ChangeMyPasswordAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = CreateChangePasswordRequest();
            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _userService.ChangeMyPasswordAsync(userId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
        }

        // ── ChangeMyPasswordAsync: new password same as current ──

        [Fact]
        public async Task ChangeMyPasswordAsync_SameAsCurrent_ReturnsFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = CreateChangePasswordRequest();
            var user = new AppUser { Id = userId };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, request.NewPassword))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.ChangeMyPasswordAsync(userId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(
                ErrorMessages.User.ChangePassword.NewPasswordSameAsCurrent,
                result.ErrorCode);
            _userManagerMock.Verify(
                x => x.ChangePasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        // ── ChangeMyPasswordAsync: ChangePasswordAsync fails with PasswordMismatch ──

        [Fact]
        public async Task ChangeMyPasswordAsync_CurrentPasswordWrong_ReturnsCurrentPasswordIncorrect()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = CreateChangePasswordRequest();
            var user = new AppUser { Id = userId };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, request.NewPassword))
                .ReturnsAsync(false);
            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password." }));

            // Act
            var result = await _userService.ChangeMyPasswordAsync(userId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Auth.CurrentPasswordIncorrect, result.ErrorCode);
            Assert.Contains("Incorrect password.", result.ErrorMessage);
        }

        // ── ChangeMyPasswordAsync: ChangePasswordAsync fails with other error ──

        [Fact]
        public async Task ChangeMyPasswordAsync_OtherFailure_ReturnsChangePasswordFailed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = CreateChangePasswordRequest();
            var user = new AppUser { Id = userId };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, request.NewPassword))
                .ReturnsAsync(false);
            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Code = "PasswordTooShort", Description = "Too short" }));

            // Act
            var result = await _userService.ChangeMyPasswordAsync(userId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Auth.ChangePasswordFailed, result.ErrorCode);
        }

        // ── ChangeMyPasswordAsync: success ──

        [Fact]
        public async Task ChangeMyPasswordAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = CreateChangePasswordRequest();
            var user = new AppUser { Id = userId };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, request.NewPassword))
                .ReturnsAsync(false);
            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ChangeMyPasswordAsync(userId, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorCode);
            _userManagerMock.Verify(
                x => x.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword),
                Times.Once);
        }
    }
}
