using Application.Constants;
using Moq;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        // ── DeleteAsync: null ids ──

        [Fact]
        public async Task DeleteAsync_NullIds_ReturnsInvalidIds()
        {
            // Act
            var result = await _userService.DeleteAsync(null!);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.InvalidIds, result.ErrorCode);
            _userRepositoryMock.Verify(
                x => x.DeleteByIdsAsync(It.IsAny<IEnumerable<Guid>>()),
                Times.Never);
        }

        // ── DeleteAsync: empty list ──

        [Fact]
        public async Task DeleteAsync_EmptyIds_ReturnsInvalidIds()
        {
            // Act
            var result = await _userService.DeleteAsync([]);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.InvalidIds, result.ErrorCode);
        }

        // ── DeleteAsync: no rows affected ──

        [Fact]
        public async Task DeleteAsync_NoRowsAffected_ReturnsUsersNotFound()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid() };
            _userRepositoryMock
                .Setup(x => x.DeleteByIdsAsync(ids))
                .ReturnsAsync(0);

            // Act
            var result = await _userService.DeleteAsync(ids);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
        }

        // ── DeleteAsync: success ──

        [Fact]
        public async Task DeleteAsync_RowsDeleted_ReturnsSuccess()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            _userRepositoryMock
                .Setup(x => x.DeleteByIdsAsync(ids))
                .ReturnsAsync(2);

            // Act
            var result = await _userService.DeleteAsync(ids);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.ErrorCode);
            _userRepositoryMock.Verify(x => x.DeleteByIdsAsync(ids), Times.Once);
        }
    }
}
