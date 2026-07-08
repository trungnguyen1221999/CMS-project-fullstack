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
        public async Task GetAllCategoriesAsync_NoPermission_ThrowsForbidden()
        {
            var userId = Guid.NewGuid();

            _mockPermissionService
                .Setup(x => x.HasViewAllCategoryPermission(userId))
                .Returns(false);

            var ex = await Assert.ThrowsAsync<ForbiddenException>(
                () => _categoryService.GetAllCategoriesAsync(userId)
            );
            Assert.Equal(ErrorMessages.Category.InsufficientPermissions, ex.ErrorCode);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_Success_ReturnsCategories()
        {
            var userId = Guid.NewGuid();
            var categories = new List<PostCategory>
            {
                new() { Id = Guid.NewGuid(), Name = "Tech", Slug = "tech" },
                new() { Id = Guid.NewGuid(), Name = "Life", Slug = "life" },
            };
            var mockQueryable = categories.BuildMockQueryable();

            _mockPermissionService
                .Setup(x => x.HasViewAllCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(mockQueryable);

            _mockMapper
                .Setup(x =>
                    x.ProjectTo<PostCategoryResponse>(
                        It.IsAny<IQueryable>(),
                        It.IsAny<object>(),
                        It.IsAny<Expression<Func<PostCategoryResponse, object>>[]>()
                    )
                )
                .Returns(
                    new List<PostCategoryResponse>
                    {
                        new() { Name = "Tech", Slug = "tech" },
                        new() { Name = "Life", Slug = "life" },
                    }.BuildMockQueryable()
                );

            var result = await _categoryService.GetAllCategoriesAsync(userId);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_Empty_ReturnsEmptyList()
        {
            var userId = Guid.NewGuid();

            _mockPermissionService
                .Setup(x => x.HasViewAllCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory>().BuildMockQueryable());

            _mockMapper
                .Setup(x =>
                    x.ProjectTo<PostCategoryResponse>(
                        It.IsAny<IQueryable>(),
                        It.IsAny<object>(),
                        It.IsAny<Expression<Func<PostCategoryResponse, object>>[]>()
                    )
                )
                .Returns(new List<PostCategoryResponse>().BuildMockQueryable());

            var result = await _categoryService.GetAllCategoriesAsync(userId);

            Assert.Empty(result);
        }
    }
}
