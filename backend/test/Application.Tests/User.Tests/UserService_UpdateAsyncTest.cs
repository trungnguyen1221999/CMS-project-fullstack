using Application.Constants;
using Application.Contracts.Users.Requests;
using Microsoft.AspNetCore.Identity;
using Moq;
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
            // Arrange
            var request = UpdateUserRequest();
            var userId = Guid.NewGuid();
            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);
            // Act
            var result = await _userService.UpdateAsync(userId, request);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);

            //Verify
            _mapperMock.Verify(x => x.Map(request, It.IsAny<AppUser>()), Times.Never);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_UserFoundButUpdateFails_ReturnsFailure()
        {
            // Arrange
            var request = UpdateUserRequest();
            var userId = Guid.NewGuid();
            var existingUser = new AppUser();
            UserFound(userId, existingUser);
            _mapperMock.Setup(x => x.Map(request, existingUser)).Returns(existingUser);
            _userManagerMock
                .Setup(x => x.UpdateAsync(existingUser))
                .ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _userService.UpdateAsync(userId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UpdateFailed, result.ErrorCode);

            // Verify
            _mapperMock.Verify(x => x.Map(request, existingUser), Times.Once);
            _userManagerMock.Verify(x => x.UpdateAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UserFoundAndUpdateSuccess_ReturnsSuccess()
        {
            // Arrange
            var request = UpdateUserRequest();
            var userId = Guid.NewGuid();
            var existingUser = new AppUser();
            UserFound(userId, existingUser);
            _mapperMock.Setup(x => x.Map(request, existingUser)).Returns(existingUser);
            _userManagerMock
                .Setup(x => x.UpdateAsync(existingUser))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.UpdateAsync(userId, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorCode);

            // Verify
            _mapperMock.Verify(x => x.Map(request, existingUser), Times.Once);
            _userManagerMock.Verify(x => x.UpdateAsync(existingUser), Times.Once);
        }
    }
}
