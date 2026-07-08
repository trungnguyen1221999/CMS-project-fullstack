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
        private static UpdateUserRequest UpdateUserRequest()
        {
            return new UpdateUserRequest { FirstName = "John", LastName = "Doe" };
        }

        private void UserFound(Guid userId, AppUser existingUser)
        {
            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(existingUser);
        }

        [Fact]
        public async Task UpdateAsync_UserNotFound_ReturnsFailure()
        {
            var request = UpdateUserRequest();
            var userId = Guid.NewGuid();
            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _userService.UpdateAsync(userId, request)
            );
            Assert.Equal(ErrorMessages.User.UserNotFound, ex.ErrorCode);

            _mapperMock.Verify(x => x.Map(request, It.IsAny<AppUser>()), Times.Never);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_UserFoundButUpdateFails_ReturnsFailure()
        {
            var request = UpdateUserRequest();
            var userId = Guid.NewGuid();
            var existingUser = new AppUser();
            UserFound(userId, existingUser);
            _mapperMock.Setup(x => x.Map(request, existingUser)).Returns(existingUser);
            _userManagerMock
                .Setup(x => x.UpdateAsync(existingUser))
                .ReturnsAsync(IdentityResult.Failed());

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _userService.UpdateAsync(userId, request)
            );
            Assert.Equal(ErrorMessages.User.UpdateFailed, ex.ErrorCode);

            _mapperMock.Verify(x => x.Map(request, existingUser), Times.Once);
            _userManagerMock.Verify(x => x.UpdateAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UserFoundAndUpdateSuccess_ReturnsSuccess()
        {
            var request = UpdateUserRequest();
            var userId = Guid.NewGuid();
            var existingUser = new AppUser();
            UserFound(userId, existingUser);
            _mapperMock.Setup(x => x.Map(request, existingUser)).Returns(existingUser);
            _userManagerMock
                .Setup(x => x.UpdateAsync(existingUser))
                .ReturnsAsync(IdentityResult.Success);

            await _userService.UpdateAsync(userId, request);

            _mapperMock.Verify(x => x.Map(request, existingUser), Times.Once);
            _userManagerMock.Verify(x => x.UpdateAsync(existingUser), Times.Once);
        }
    }
}
