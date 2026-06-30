using Application.Contracts.Users.Responses;
using Domain;
using Moq;

namespace Application.Tests.User.Tests
{
    public partial class UserServiceTest
    {
        //Happy case

        [Fact]
        public async Task GetAllAsync_WithData_ReturnSuccess()
        {
            //1. Arrange
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

            //2. Act
            var result = await _userService.GetAllAsync(
                testInput.KeyWord,
                testInput.CurrentPage,
                testInput.PageSize
            );

            //3 . Assert
            Assert.NotNull(result.Data);
            Assert.True(result.IsSuccess);
            Assert.Single(result.Data.Result);
        }

        //Edge case: No data found
        [Fact]
        public async Task GetAllAsync_NoData_ReturnsEmptyResult()
        {
            //1. Arrange
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

            //2. Act
            var result = await _userService.GetAllAsync(
                testInput.KeyWord,
                testInput.CurrentPage,
                testInput.PageSize
            );

            //3 . Assert
            Assert.NotNull(result.Data);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data.Result);
        }
    }
}
