using Application.Constants;
using Application.Contracts.Users.Responses;
using Moq;
using static Application.Exceptions.CustomException;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        [Fact]
        public async Task GetByIdAsync_WithData_ReturnSuccess()
        {
            var testInput = Guid.NewGuid();
            var user = new UserResponse { Id = testInput, Email = "test@gmail.com" };

            _userRepositoryMock.Setup(x => x.GetByIdWithRolesAsync(testInput)).ReturnsAsync(user);

            var result = await _userService.GetByIdAsync(testInput);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound()
        {
            var testInput = Guid.NewGuid();

            _userRepositoryMock
                .Setup(x => x.GetByIdWithRolesAsync(testInput))
                .ReturnsAsync((UserResponse)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _userService.GetByIdAsync(testInput)
            );
            Assert.Equal(ErrorMessages.User.UserNotFound, ex.ErrorCode);
        }
    }
}
