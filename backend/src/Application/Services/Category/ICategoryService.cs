using Application.Contracts.Posts.Response;

namespace Application.Services.Category
{
    public interface ICategoryService
    {
        Task<PostCategoryResponse> GetCategoryByIdAsync(
            Guid categoryId,
            Guid currentUserId
        );
    }
}
