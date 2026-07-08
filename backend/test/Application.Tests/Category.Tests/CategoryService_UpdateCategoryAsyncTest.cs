using System.Linq.Expressions;
using Application.Constants;
using Application.Contracts.Posts.Request;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using static Application.Exceptions.CustomException;

namespace Application.Tests.Category.Tests
{
    public partial class CategoryServiceTest
    {
        [Fact]
        public async Task UpdateCategoryAsync_NoPermission_ThrowsForbidden()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = CreateValidCategoryRequest();

            _mockPermissionService
                .Setup(x => x.HasEditCategoryPermission(userId))
                .Returns(false);

            var ex = await Assert.ThrowsAsync<ForbiddenException>(
                () => _categoryService.UpdateCategoryAsync(categoryId, request, userId)
            );
            Assert.Equal(ErrorMessages.Category.InsufficientPermissions, ex.ErrorCode);
        }

        [Fact]
        public async Task UpdateCategoryAsync_CategoryNotFound_ThrowsNotFound()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = CreateValidCategoryRequest();

            _mockPermissionService
                .Setup(x => x.HasEditCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory>().BuildMockQueryable());

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _categoryService.UpdateCategoryAsync(categoryId, request, userId)
            );
            Assert.Equal(ErrorMessages.Category.CategoryNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task UpdateCategoryAsync_SlugExists_ThrowsBadRequest()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = CreateValidCategoryRequest();
            var existingCategory = new PostCategory
            {
                Id = categoryId,
                Name = "Old",
                Slug = "old",
            };
            var otherCategory = new PostCategory
            {
                Id = Guid.NewGuid(),
                Slug = "tech",
            };

            var callCount = 0;
            _mockPermissionService
                .Setup(x => x.HasEditCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        return new List<PostCategory> { existingCategory }.BuildMockQueryable();
                    return new List<PostCategory> { otherCategory }.BuildMockQueryable();
                });

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _categoryService.UpdateCategoryAsync(categoryId, request, userId)
            );
            Assert.Equal(ErrorMessages.Category.SlugAlreadyExists, ex.ErrorCode);
        }

        [Fact]
        public async Task UpdateCategoryAsync_SaveFails_ThrowsBadRequest()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = CreateValidCategoryRequest();
            var existingCategory = new PostCategory
            {
                Id = categoryId,
                Name = "Old",
                Slug = "old",
            };

            var callCount = 0;
            _mockPermissionService
                .Setup(x => x.HasEditCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        return new List<PostCategory> { existingCategory }.BuildMockQueryable();
                    return new List<PostCategory>().BuildMockQueryable();
                });

            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(0);

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _categoryService.UpdateCategoryAsync(categoryId, request, userId)
            );
            Assert.Equal(ErrorMessages.Category.UpdateFailed, ex.ErrorCode);
        }

        [Fact]
        public async Task UpdateCategoryAsync_Success()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = CreateValidCategoryRequest();
            var existingCategory = new PostCategory
            {
                Id = categoryId,
                Name = "Old",
                Slug = "old",
            };

            var callCount = 0;
            _mockPermissionService
                .Setup(x => x.HasEditCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        return new List<PostCategory> { existingCategory }.BuildMockQueryable();
                    return new List<PostCategory>().BuildMockQueryable();
                });

            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            await _categoryService.UpdateCategoryAsync(categoryId, request, userId);

            _mockMapper.Verify(
                x => x.Map(request, existingCategory),
                Times.Once
            );
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
        }
    }
}
