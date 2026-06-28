using Application.Constants;
using Application.Contracts.Auth.Requests;
using Application.Services.Auth;
using AutoMapper;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;
using Test.Shared.Mocks;

namespace Application.Tests.Auth.Tests
{
    public class SignUpServiceTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ISignUpService _signUpService;

        public SignUpServiceTest()
        {
            _userManagerMock = MockUserManager.Create();
            _mapperMock = new Mock<IMapper>();
            _signUpService = new SignUpService(_userManagerMock.Object, _mapperMock.Object);
        }

        // ===== HELPERS =====

        private static SignUpRequest CreateValidRequest() =>
            new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
            };

        /// <summary>
        /// Setup: Email does not exist, Map request to User.
        /// </summary>
        private User SetupNewUserFlow(SignUpRequest request)
        {
            var user = new User { Email = request.Email };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            _mapperMock.Setup(x => x.Map<SignUpRequest, User>(request)).Returns(user);

            return user;
        }

        // ===== TEST CASES =====

        [Fact]
        public async Task SignUpAsync_EmailAlreadyExists_ReturnsErrorUserAlreadyExists()
        {
            // Arrange
            var request = CreateValidRequest();
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });

            // Act
            var result = await _signUpService.SignUpAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserAlreadyExists, result.ErrorCode);
            _userManagerMock.Verify(
                x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SignUpAsync_CreateUserFailed_ReturnsErrorCreateFailed()
        {
            // Arrange
            var request = CreateValidRequest();
            SetupNewUserFlow(request);

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(
                    IdentityResult.Failed(new IdentityError { Description = "Password too weak" })
                );

            // Act
            var result = await _signUpService.SignUpAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.CreateFailed, result.ErrorCode);
            Assert.Contains("Password too weak", result.ErrorMessage);
            _userManagerMock.Verify(
                x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SignUpAsync_AddToRoleFailed_ReturnsErrorFailedToAssignRole()
        {
            // Arrange
            var request = CreateValidRequest();
            var user = SetupNewUserFlow(request);

            _userManagerMock
                .Setup(x => x.CreateAsync(user, request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(user, "User"))
                .ReturnsAsync(
                    IdentityResult.Failed(new IdentityError { Description = "Role does not exist" })
                );

            // Act
            var result = await _signUpService.SignUpAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Auth.FailedToAssignRole, result.ErrorCode);
            Assert.Contains("Role does not exist", result.ErrorMessage);
        }

        [Fact]
        public async Task SignUpAsync_Success_ReturnsIsSuccessTrue()
        {
            // Arrange
            var request = CreateValidRequest();
            var user = SetupNewUserFlow(request);

            _userManagerMock
                .Setup(x => x.CreateAsync(user, request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(user, "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _signUpService.SignUpAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorCode);
            Assert.Null(result.ErrorMessage);
        }
    }
}
