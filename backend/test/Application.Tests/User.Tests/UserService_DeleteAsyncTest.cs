using Application.Constants;
using Moq;
using static Application.Exceptions.CustomException;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        [Fact]
        public async Task DeleteAsync_NullIds_ReturnsInvalidIds()
        {
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _userService.DeleteAsync(null!)
            );
            Assert.Equal(ErrorMessages.User.InvalidIds, ex.ErrorCode);
            _userRepositoryMock.Verify(
                x => x.DeleteByIdsAsync(It.IsAny<IEnumerable<Guid>>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_EmptyIds_ReturnsInvalidIds()
        {
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _userService.DeleteAsync([])
            );
            Assert.Equal(ErrorMessages.User.InvalidIds, ex.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_NoRowsAffected_ReturnsUsersNotFound()
        {
            var ids = new List<Guid> { Guid.NewGuid() };
            _userRepositoryMock
                .Setup(x => x.DeleteByIdsAsync(ids))
                .ReturnsAsync(0);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _userService.DeleteAsync(ids)
            );
            Assert.Equal(ErrorMessages.User.UserNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task DeleteAsync_RowsDeleted_ReturnsSuccess()
        {
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            _userRepositoryMock
                .Setup(x => x.DeleteByIdsAsync(ids))
                .ReturnsAsync(2);

            await _userService.DeleteAsync(ids);

            _userRepositoryMock.Verify(x => x.DeleteByIdsAsync(ids), Times.Once);
        }
    }
}
