using Application.Constants;
using Application.Contracts.Users.Requests;
using Application.Services.Auth;
using Application.Services.Otp;
using Microsoft.AspNetCore.Identity;
using Moq;
using Test.Shared.Mocks;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Auth.Tests
{
    public class ForgotPasswordServiceTest
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly IForgotPasswordService _forgotPasswordService;

        public ForgotPasswordServiceTest()
        {
            _userManagerMock = MockUserManager.Create();
            _emailServiceMock = new Mock<IEmailService>();
            _forgotPasswordService = new ForgotPasswordService(
                _userManagerMock.Object,
                _emailServiceMock.Object
            );
        }

        // ===== HELPERS =====

        private static ForgotPasswordRequest CreateForgotRequest(string email = "test@example.com") =>
            new() { Email = email };

        private static ResetPasswordRequest CreateResetRequest(
            string email = "test@example.com",
            string code = "1234",
            string newPassword = "NewPass123!"
        ) =>
            new()
            {
                Email = email,
                Code = code,
                NewPassword = newPassword,
                ConfirmationPassword = newPassword
            };

        private AppUser SetupUserFound(string email = "test@example.com")
        {
            var user = new AppUser { Email = email };
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);
            return user;
        }

        private void SetupUserNotFound(string email = "test@example.com")
        {
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((AppUser?)null);
        }

        // ===== ForgotPasswordAsync TESTS =====

        [Fact]
        public async Task ForgotPasswordAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var request = CreateForgotRequest();
            SetupUserNotFound(request.Email);

            // Act
            var result = await _forgotPasswordService.ForgotPasswordAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
            _emailServiceMock.Verify(x => x.SendOtpAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPasswordAsync_UserFound_SendsOtpAndReturnsSuccess()
        {
            // Arrange
            var request = CreateForgotRequest();
            SetupUserFound(request.Email);
            _emailServiceMock.Setup(x => x.SendOtpAsync(request.Email)).Returns(Task.CompletedTask);

            // Act
            var result = await _forgotPasswordService.ForgotPasswordAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            _emailServiceMock.Verify(x => x.SendOtpAsync(request.Email), Times.Once);
        }

        // ===== ResetPasswordAsync TESTS =====

        [Fact]
        public async Task ResetPasswordAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var request = CreateResetRequest();
            SetupUserNotFound(request.Email);

            // Act
            var result = await _forgotPasswordService.ResetPasswordAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task ResetPasswordAsync_InvalidOtp_ReturnsCodeInvalid()
        {
            // Arrange
            var request = CreateResetRequest();
            SetupUserFound(request.Email);
            _emailServiceMock
                .Setup(x => x.ValidateOtpAsync(request.Email, request.Code))
                .ReturnsAsync(false);

            // Act
            var result = await _forgotPasswordService.ResetPasswordAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Auth.CodeInvalid, result.ErrorCode);
        }

        [Fact]
        public async Task ResetPasswordAsync_ResetFailed_ReturnsResetPasswordFailed()
        {
            // Arrange
            var request = CreateResetRequest();
            var user = SetupUserFound(request.Email);
            _emailServiceMock
                .Setup(x => x.ValidateOtpAsync(request.Email, request.Code))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");
            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(user, "reset-token", request.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Too weak" }));

            // Act
            var result = await _forgotPasswordService.ResetPasswordAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Auth.ResetPasswordFailed, result.ErrorCode);
            Assert.Contains("Too weak", result.ErrorMessage);
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidOtp_ReturnsSuccessAndRemovesOtp()
        {
            // Arrange
            var request = CreateResetRequest();
            var user = SetupUserFound(request.Email);
            _emailServiceMock
                .Setup(x => x.ValidateOtpAsync(request.Email, request.Code))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");
            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(user, "reset-token", request.NewPassword))
                .ReturnsAsync(IdentityResult.Success);
            _emailServiceMock
                .Setup(x => x.RemoveOtpAsync(request.Email))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _forgotPasswordService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            _emailServiceMock.Verify(x => x.RemoveOtpAsync(request.Email), Times.Once);
        }
    }
}
