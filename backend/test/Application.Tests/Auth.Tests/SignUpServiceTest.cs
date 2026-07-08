using Application.Constants;
using Application.Contracts.Auth.Requests;
using Application.Services.Auth;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using Test.Shared.Mocks;
using static Application.Exceptions.CustomException;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Auth.Tests
{
    public class SignUpServiceTest
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ISignUpService _signUpService;

        public SignUpServiceTest()
        {
            _userManagerMock = MockUserManager.Create();
            _mapperMock = new Mock<IMapper>();
            _signUpService = new SignUpService(_userManagerMock.Object, _mapperMock.Object);
        }

        private static SignUpRequest CreateValidRequest() =>
            new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
            };

        private AppUser SetupNewUserFlow(SignUpRequest request)
        {
            var user = new AppUser { Email = request.Email };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((AppUser?)null);

            _mapperMock.Setup(x => x.Map<SignUpRequest, AppUser>(request)).Returns(user);

            return user;
        }

        [Fact]
        public async Task SignUpAsync_EmailAlreadyExists_ReturnsErrorUserAlreadyExists()
        {
            var request = CreateValidRequest();
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new AppUser { Email = request.Email });

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _signUpService.SignUpAsync(request)
            );
            Assert.Equal(ErrorMessages.User.UserAlreadyExists, ex.ErrorCode);
            _userManagerMock.Verify(
                x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SignUpAsync_CreateUserFailed_ReturnsErrorCreateFailed()
        {
            var request = CreateValidRequest();
            SetupNewUserFlow(request);

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), request.Password))
                .ReturnsAsync(
                    IdentityResult.Failed(new IdentityError { Description = "Password too weak" })
                );

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _signUpService.SignUpAsync(request)
            );
            Assert.Equal(ErrorMessages.User.CreateFailed, ex.ErrorCode);
            _userManagerMock.Verify(
                x => x.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SignUpAsync_AddToRoleFailed_ReturnsErrorFailedToAssignRole()
        {
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

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _signUpService.SignUpAsync(request)
            );
            Assert.Equal(ErrorMessages.Auth.FailedToAssignRole, ex.ErrorCode);
        }

        [Fact]
        public async Task SignUpAsync_Success_ReturnsIsSuccessTrue()
        {
            var request = CreateValidRequest();
            var user = SetupNewUserFlow(request);

            _userManagerMock
                .Setup(x => x.CreateAsync(user, request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(user, "User"))
                .ReturnsAsync(IdentityResult.Success);

            await _signUpService.SignUpAsync(request);

            _userManagerMock.Verify(x => x.CreateAsync(user, request.Password), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(user, "User"), Times.Once);
        }
    }
}
