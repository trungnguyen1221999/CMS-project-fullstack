using System.Linq.Expressions;
using Application.Constants;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using static Application.Exceptions.CustomException;

namespace Application.Tests.Category.Tests
{
    public partial class CategoryServiceTest
    {
        private static CreateUpdatePostCategoryRequest CreateValidCategoryRequest() =>
            new()
            {
                Name = "Tech",
                Slug = "tech",
                IsActive = true,
                SortOrder = 1,
            };

        [Fact]
        public async Task CreateCategoryAsync_NoPermission_ThrowsForbidden()
        {
            var userId = Guid.NewGuid();
            var request = CreateValidCategoryRequest();

            _mockPermissionService
                .Setup(x => x.HasCreateCategoryPermission(userId))
                .Returns(false);

            var ex = await Assert.ThrowsAsync<ForbiddenException>(
                () => _categoryService.CreateCategoryAsync(request, userId)
            );
            Assert.Equal(ErrorMessages.Category.InsufficientPermissions, ex.ErrorCode);
        }

        [Fact]
        public async Task CreateCategoryAsync_SlugExists_ThrowsBadRequest()
        {
            var userId = Guid.NewGuid();
            var request = CreateValidCategoryRequest();

            _mockPermissionService
                .Setup(x => x.HasCreateCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(
                    new List<PostCategory>
                    {
                        new() { Slug = "tech" },
                    }.BuildMockQueryable()
                );

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _categoryService.CreateCategoryAsync(request, userId)
            );
            Assert.Equal(ErrorMessages.Category.SlugAlreadyExists, ex.ErrorCode);
        }

        [Fact]
        public async Task CreateCategoryAsync_SaveFails_ThrowsBadRequest()
        {
            var userId = Guid.NewGuid();
            var request = CreateValidCategoryRequest();
            var mappedCategory = new PostCategory { Name = "Tech", Slug = "tech" };

            _mockPermissionService
                .Setup(x => x.HasCreateCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory>().BuildMockQueryable());

            _mockMapper
                .Setup(x =>
                    x.Map<CreateUpdatePostCategoryRequest, PostCategory>(request)
                )
                .Returns(mappedCategory);

            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(0);

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _categoryService.CreateCategoryAsync(request, userId)
            );
            Assert.Equal(ErrorMessages.Category.CreateFailed, ex.ErrorCode);
        }

        [Fact]
        public async Task CreateCategoryAsync_Success_ReturnsCategory()
        {
            var userId = Guid.NewGuid();
            var request = CreateValidCategoryRequest();
            var mappedCategory = new PostCategory { Name = "Tech", Slug = "tech" };
            var expectedResponse = new PostCategoryResponse { Name = "Tech", Slug = "tech" };

            _mockPermissionService
                .Setup(x => x.HasCreateCategoryPermission(userId))
                .Returns(true);

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory>().BuildMockQueryable());

            _mockMapper
                .Setup(x =>
                    x.Map<CreateUpdatePostCategoryRequest, PostCategory>(request)
                )
                .Returns(mappedCategory);

            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            _mockMapper
                .Setup(x => x.Map<PostCategory, PostCategoryResponse>(mappedCategory))
                .Returns(expectedResponse);

            var result = await _categoryService.CreateCategoryAsync(request, userId);

            Assert.NotNull(result);
            Assert.Equal("Tech", result.Name);
            _mockCategoryRepository.Verify(x => x.Add(mappedCategory), Times.Once);
        }
    }
}
