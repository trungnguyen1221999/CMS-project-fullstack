using System.Linq.Expressions;
using Application.Constants;
using Application.Contracts.Posts.Response;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using static Application.Exceptions.CustomException;

namespace Application.Tests.Category.Tests
{
    public partial class CategoryServiceTest
    {
        [Fact]
        public async Task GetCategoryByIdAsync_NoPermission_ThrowsForbidden()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockPermissionService
                .Setup(x => x.HasViewAllCategoryPermission(userId))
                .Returns(false);

            var ex = await Assert.ThrowsAsync<ForbiddenException>(
                () => _categoryService.GetCategoryByIdAsync(categoryId, userId)
            );
            Assert.Equal(ErrorMessages.Category.InsufficientPermissions, ex.ErrorCode);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_CategoryNotFound_ThrowsNotFound()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockPermissionService
                .Setup(x => x.HasViewAllCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory>().BuildMockQueryable());

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _categoryService.GetCategoryByIdAsync(categoryId, userId)
            );
            Assert.Equal(ErrorMessages.Category.CategoryNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_Success_ReturnsCategory()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var category = new PostCategory
            {
                Id = categoryId,
                Name = "Tech",
                Slug = "tech",
            };
            var expectedResponse = new PostCategoryResponse
            {
                Name = "Tech",
                Slug = "tech",
            };

            _mockPermissionService
                .Setup(x => x.HasViewAllCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory> { category }.BuildMockQueryable());

            _mockMapper
                .Setup(x => x.Map<PostCategory, PostCategoryResponse>(category))
                .Returns(expectedResponse);

            var result = await _categoryService.GetCategoryByIdAsync(categoryId, userId);

            Assert.NotNull(result);
            Assert.Equal("Tech", result.Name);
        }
    }
}
