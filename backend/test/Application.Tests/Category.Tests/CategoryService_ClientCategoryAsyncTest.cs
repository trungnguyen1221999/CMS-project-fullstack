using System.Linq.Expressions;
using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Posts.Response;
using Domain;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using static Application.Exceptions.CustomException;

namespace Application.Tests.Category.Tests
{
    public partial class CategoryServiceTest
    {
        [Fact]
        public async Task GetAllActiveCategoriesAsync_ReturnsActiveCategories()
        {
            var categories = new List<PostCategory>
            {
                new() { Id = Guid.NewGuid(), Name = "Tech", Slug = "tech", IsActive = true },
                new() { Id = Guid.NewGuid(), Name = "Life", Slug = "life", IsActive = true },
            };

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(categories.BuildMockQueryable());

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

            var result = await _categoryService.GetAllActiveCategoriesAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllActiveCategoriesAsync_NoActive_ReturnsEmptyList()
        {
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

            var result = await _categoryService.GetAllActiveCategoriesAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetActiveCategoryByIdAsync_NotFound_ThrowsNotFound()
        {
            var categoryId = Guid.NewGuid();

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory>().BuildMockQueryable());

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _categoryService.GetActiveCategoryByIdAsync(categoryId)
            );
            Assert.Equal(ErrorMessages.Category.CategoryNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task GetActiveCategoryByIdAsync_Success_ReturnsCategory()
        {
            var categoryId = Guid.NewGuid();
            var category = new PostCategory
            {
                Id = categoryId,
                Name = "Tech",
                Slug = "tech",
                IsActive = true,
            };
            var expectedResponse = new PostCategoryResponse
            {
                Name = "Tech",
                Slug = "tech",
            };

            _mockCategoryRepository
                .Setup(x => x.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory> { category }.BuildMockQueryable());

            _mockMapper
                .Setup(x => x.Map<PostCategory, PostCategoryResponse>(category))
                .Returns(expectedResponse);

            var result = await _categoryService.GetActiveCategoryByIdAsync(categoryId);

            Assert.NotNull(result);
            Assert.Equal("Tech", result.Name);
        }

        [Fact]
        public async Task GetActiveCategoriesPagingAsync_ReturnsPagedResult()
        {
            var request = new PagingRequest { CurrentPage = 1, PageSize = 10 };
            var pageResult = new PageResult<PostCategoryResponse>
            {
                CurrentPage = 1,
                PageSize = 10,
                TotalCount = 1,
                Result = [new PostCategoryResponse { Name = "Tech" }],
            };

            _mockCategoryRepository
                .Setup(x => x.GetActiveCategoriesPagingAsync(request))
                .ReturnsAsync(pageResult);

            var result = await _categoryService.GetActiveCategoriesPagingAsync(request);

            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Result);
        }
    }
}
