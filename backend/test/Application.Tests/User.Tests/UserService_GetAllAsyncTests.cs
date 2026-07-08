using Application.Contracts.Users.Responses;
using Domain;
using Moq;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        [Fact]
        public async Task GetAllAsync_WithData_ReturnSuccess()
        {
            var testInput = new
            {
                KeyWord = "test",
                CurrentPage = 1,
                PageSize = 10,
            };

            var pageResult = new PageResult<UserListItemResponse>
            {
                CurrentPage = testInput.CurrentPage,
                PageSize = testInput.PageSize,
                TotalCount = 1,
                Result = new List<UserListItemResponse>
                {
                    new UserListItemResponse { Id = Guid.NewGuid(), Email = "test@gmail.com" },
                },
            };

            _userRepositoryMock
                .Setup(x =>
                    x.GetAllWithRolesAsync(
                        testInput.KeyWord,
                        testInput.CurrentPage,
                        testInput.PageSize
                    )
                )
                .ReturnsAsync(pageResult);

            var result = await _userService.GetAllAsync(
                testInput.KeyWord,
                testInput.CurrentPage,
                testInput.PageSize
            );

            Assert.NotNull(result);
            Assert.Single(result.Result);
        }

        [Fact]
        public async Task GetAllAsync_NoData_ReturnsEmptyResult()
        {
            var testInput = new
            {
                KeyWord = "test",
                CurrentPage = 1,
                PageSize = 10,
            };

            var pageResult = new PageResult<UserListItemResponse>
            {
                CurrentPage = testInput.CurrentPage,
                PageSize = testInput.PageSize,
                TotalCount = 0,
                Result = new List<UserListItemResponse>(),
            };

            _userRepositoryMock
                .Setup(x =>
                    x.GetAllWithRolesAsync(
                        testInput.KeyWord,
                        testInput.CurrentPage,
                        testInput.PageSize
                    )
                )
                .ReturnsAsync(pageResult);

            var result = await _userService.GetAllAsync(
                testInput.KeyWord,
                testInput.CurrentPage,
                testInput.PageSize
            );

            Assert.NotNull(result);
            Assert.Empty(result.Result);
        }
    }
}
