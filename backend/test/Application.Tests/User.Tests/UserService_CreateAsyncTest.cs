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
        private static CreateUserRequest Request()
        {
            return new CreateUserRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "test@gmail.com",
                Password = "123456",
            };
        }

        private static AppUser ExistingUser(CreateUserRequest request)
        {
            return new AppUser
            {
                Id = Guid.NewGuid(),
                FirstName = "Existing",
                LastName = "User",
                Email = request.Email,
                UserName = request.Email,
            };
        }

        private void UserAlreadyExist(CreateUserRequest request)
        {
            var existingUser = ExistingUser(request);
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(existingUser);
        }

        private void UserNotExist(CreateUserRequest request)
        {
            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((AppUser?)null);
        }

        [Fact]
        public async Task CreateAsync_UserAlreadyExists_ReturnsFailure()
        {
            var request = Request();
            UserAlreadyExist(request);

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _userService.CreateAsync(request)
            );
            Assert.Equal(ErrorMessages.User.UserAlreadyExists, ex.ErrorCode);

            _mapperMock.Verify(
                x => x.Map<CreateUserRequest, AppUser>(It.IsAny<CreateUserRequest>()),
                Times.Never
            );
            _userManagerMock.Verify(
                x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task CreateAsync_UserDoesNotExistButCreateFail_ReturnsFailure()
        {
            var request = Request();
            UserNotExist(request);
            var newUser = new AppUser { Email = request.Email };
            _mapperMock.Setup(x => x.Map<CreateUserRequest, AppUser>(request)).Returns(newUser);
            _userManagerMock
                .Setup(x => x.CreateAsync(newUser, request.Password))
                .ReturnsAsync(IdentityResult.Failed());

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _userService.CreateAsync(request)
            );
            Assert.Equal(ErrorMessages.User.CreateFailed, ex.ErrorCode);
        }

        [Fact]
        public async Task CreateAsync_UserDoesNotExistAndCreateSucceeds_ReturnsSuccess()
        {
            var request = Request();
            UserNotExist(request);
            var newUser = new AppUser { Email = request.Email };
            _mapperMock.Setup(x => x.Map<CreateUserRequest, AppUser>(request)).Returns(newUser);
            _userManagerMock
                .Setup(x => x.CreateAsync(newUser, request.Password))
                .ReturnsAsync(IdentityResult.Success);

            await _userService.CreateAsync(request);

            _userManagerMock.Verify(
                x => x.CreateAsync(newUser, request.Password),
                Times.Once
            );
        }
    }
}
