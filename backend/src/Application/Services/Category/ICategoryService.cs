using Application.Contracts.Common;
using Application.Contracts.Posts.Response;
using Domain;

namespace Application.Services.Category
{
    public interface ICategoryService
    {
        Task<PostCategoryResponse> GetActiveCategoryByIdAsync(Guid categoryId);
        Task<List<PostCategoryResponse>> GetAllActiveCategoriesAsync();

        Task<PageResult<PostCategoryResponse>> GetActiveCategoriesPagingAsync(
            PagingRequest request
        );
    }
}
