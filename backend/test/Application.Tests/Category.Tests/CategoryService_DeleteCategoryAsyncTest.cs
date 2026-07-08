using System.Linq.Expressions;
using Application.Constants;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using static Application.Exceptions.CustomException;

namespace Application.Tests.Category.Tests
{
    public partial class CategoryServiceTest
    {
        [Fact]
        public async Task DeleteCategoryAsync_NoPermission_ThrowsForbidden()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockPermissionService
                .Setup(x => x.HasDeleteCategoryPermission(userId))
                .Returns(false);

            var ex = await Assert.ThrowsAsync<ForbiddenException>(
                () => _categoryService.DeleteCategoryAsync(categoryId, userId)
            );
            Assert.Equal(ErrorMessages.Category.InsufficientPermissions, ex.ErrorCode);
        }

        [Fact]
        public async Task DeleteCategoryAsync_CategoryNotFound_ThrowsNotFound()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockPermissionService
                .Setup(x => x.HasDeleteCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory>().BuildMockQueryable());

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _categoryService.DeleteCategoryAsync(categoryId, userId)
            );
            Assert.Equal(ErrorMessages.Category.CategoryNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task DeleteCategoryAsync_SaveFails_ThrowsBadRequest()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var category = new PostCategory
            {
                Id = categoryId,
                Name = "Tech",
                Slug = "tech",
            };

            _mockPermissionService
                .Setup(x => x.HasDeleteCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory> { category }.BuildMockQueryable());

            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(0);

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _categoryService.DeleteCategoryAsync(categoryId, userId)
            );
            Assert.Equal(ErrorMessages.Category.DeleteFailed, ex.ErrorCode);
        }

        [Fact]
        public async Task DeleteCategoryAsync_Success()
        {
            var categoryId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var category = new PostCategory
            {
                Id = categoryId,
                Name = "Tech",
                Slug = "tech",
            };

            _mockPermissionService
                .Setup(x => x.HasDeleteCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory> { category }.BuildMockQueryable());

            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            await _categoryService.DeleteCategoryAsync(categoryId, userId);

            _mockCategoryRepository.Verify(x => x.Remove(category), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
        }
    }
}
