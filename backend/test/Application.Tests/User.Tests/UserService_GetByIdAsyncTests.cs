using Application.Constants;
using Application.Contracts.Users.Responses;
using Moq;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        [Fact]
        public async Task GetByIdAsync_WithData_ReturnSuccess()
        {
            //1. Arrange
            var testInput = Guid.NewGuid();
            var user = new UserResponse { Id = testInput, Email = "test@gmail.com" };

            _userRepositoryMock.Setup(x => x.GetByIdWithRolesAsync(testInput)).ReturnsAsync(user);

            //2. Act
            var result = await _userService.GetByIdAsync(testInput);

            //3. Assert
            Assert.NotNull(result.Data);
            Assert.True(result.IsSuccess);
            Assert.Equal(user.Id, result.Data.Id);
            Assert.Equal(user.Email, result.Data.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound()
        {
            //1. Arrange
            var testInput = Guid.NewGuid();

            _userRepositoryMock
                .Setup(x => x.GetByIdWithRolesAsync(testInput))
                .ReturnsAsync((UserResponse)null);

            //2. Act
            var result = await _userService.GetByIdAsync(testInput);

            //3. Assert
            Assert.Null(result.Data);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
        }
    }
}
