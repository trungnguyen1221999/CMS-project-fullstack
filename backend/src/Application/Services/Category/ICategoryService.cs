using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;

namespace Application.Services.Category
{
    public interface ICategoryService
    {
        // Admin
        Task<PostCategoryResponse> GetCategoryByIdAsync(Guid categoryId, Guid userId);
        Task<List<PostCategoryResponse>> GetAllCategoriesAsync(Guid userId);
        Task<PageResult<PostCategoryResponse>> GetCategoriesPagingAsync(
            PagingRequest request,
            Guid userId
        );
        Task<PostCategoryResponse> CreateCategoryAsync(
            CreateUpdatePostCategoryRequest request,
            Guid userId
        );
        Task UpdateCategoryAsync(
            Guid categoryId,
            CreateUpdatePostCategoryRequest request,
            Guid userId
        );
        Task DeleteCategoryAsync(Guid categoryId, Guid userId);

        // Client
        Task<PostCategoryResponse> GetActiveCategoryByIdAsync(Guid categoryId);
        Task<List<PostCategoryResponse>> GetAllActiveCategoriesAsync();

        Task<PageResult<PostCategoryResponse>> GetActiveCategoriesPagingAsync(
            PagingRequest request
        );
    }
}
