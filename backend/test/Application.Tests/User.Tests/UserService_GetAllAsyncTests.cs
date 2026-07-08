using Application.Contracts.Common;
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
            var request = new PagingRequest
            {
                Keyword = "test",
                CurrentPage = 1,
                PageSize = 10,
            };

            var pageResult = new PageResult<UserListItemResponse>
            {
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                TotalCount = 1,
                Result = new List<UserListItemResponse>
                {
                    new UserListItemResponse { Id = Guid.NewGuid(), Email = "test@gmail.com" },
                },
            };

            _userRepositoryMock
                .Setup(x => x.GetAllWithRolesAsync(request))
                .ReturnsAsync(pageResult);

            var result = await _userService.GetAllAsync(request);

            Assert.NotNull(result);
            Assert.Single(result.Result);
        }

        [Fact]
        public async Task GetAllAsync_NoData_ReturnsEmptyResult()
        {
            var request = new PagingRequest
            {
                Keyword = "test",
                CurrentPage = 1,
                PageSize = 10,
            };

            var pageResult = new PageResult<UserListItemResponse>
            {
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                TotalCount = 0,
                Result = new List<UserListItemResponse>(),
            };

            _userRepositoryMock
                .Setup(x => x.GetAllWithRolesAsync(request))
                .ReturnsAsync(pageResult);

            var result = await _userService.GetAllAsync(request);

            Assert.NotNull(result);
            Assert.Empty(result.Result);
        }
    }
}
