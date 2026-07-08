using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Posts.Response;
using Domain;
using Moq;
using static Application.Exceptions.CustomException;

namespace Application.Tests.Category.Tests
{
    public partial class CategoryServiceTest
    {
        [Fact]
        public async Task GetCategoriesPagingAsync_NoPermission_ThrowsForbidden()
        {
            var userId = Guid.NewGuid();
            var request = new PagingRequest { CurrentPage = 1, PageSize = 10 };

            _mockPermissionService
                .Setup(x => x.HasViewAllCategoryPermission(userId))
                .Returns(false);

            var ex = await Assert.ThrowsAsync<ForbiddenException>(
                () => _categoryService.GetCategoriesPagingAsync(request, userId)
            );
            Assert.Equal(ErrorMessages.Category.InsufficientPermissions, ex.ErrorCode);
        }

        [Fact]
        public async Task GetCategoriesPagingAsync_Success_ReturnsPagedResult()
        {
            var userId = Guid.NewGuid();
            var request = new PagingRequest { CurrentPage = 1, PageSize = 10 };
            var pageResult = new PageResult<PostCategoryResponse>
            {
                CurrentPage = 1,
                PageSize = 10,
                TotalCount = 2,
                Result =
                [
                    new PostCategoryResponse { Name = "Tech" },
                    new PostCategoryResponse { Name = "Life" },
                ],
            };

            _mockPermissionService
                .Setup(x => x.HasViewAllCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.GetCategoriesPagingAsync(request))
                .ReturnsAsync(pageResult);

            var result = await _categoryService.GetCategoriesPagingAsync(request, userId);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Result.Count);
        }
    }
}
