using Application.Constants;
using Application.Contracts.Users.Requests;
using Microsoft.AspNetCore.Identity;
using Moq;
using static Application.Exceptions.CustomException;
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

        [Fact]
        public async Task ChangeMyPasswordAsync_UserNotFound_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var request = CreateChangePasswordRequest();
            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _userService.ChangeMyPasswordAsync(userId, request)
            );
            Assert.Equal(ErrorMessages.User.UserNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task ChangeMyPasswordAsync_SameAsCurrent_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var request = CreateChangePasswordRequest();
            var user = new AppUser { Id = userId };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, request.NewPassword))
                .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _userService.ChangeMyPasswordAsync(userId, request)
            );
            Assert.Equal(
                ErrorMessages.User.ChangePassword.NewPasswordSameAsCurrent,
                ex.ErrorCode);
            _userManagerMock.Verify(
                x => x.ChangePasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangeMyPasswordAsync_CurrentPasswordWrong_ReturnsCurrentPasswordIncorrect()
        {
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

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _userService.ChangeMyPasswordAsync(userId, request)
            );
            Assert.Equal(ErrorMessages.Auth.CurrentPasswordIncorrect, ex.ErrorCode);
        }

        [Fact]
        public async Task ChangeMyPasswordAsync_OtherFailure_ReturnsChangePasswordFailed()
        {
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

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _userService.ChangeMyPasswordAsync(userId, request)
            );
            Assert.Equal(ErrorMessages.Auth.ChangePasswordFailed, ex.ErrorCode);
        }

        [Fact]
        public async Task ChangeMyPasswordAsync_ValidRequest_ReturnsSuccess()
        {
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

            await _userService.ChangeMyPasswordAsync(userId, request);

            _userManagerMock.Verify(
                x => x.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword),
                Times.Once);
        }
    }
}
