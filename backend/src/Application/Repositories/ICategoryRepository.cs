using Application.Contracts.Common;
using Application.Contracts.Posts.Response;
using Domain;
using Domain.Cores.Content;

namespace Application.Repositories
{
    public interface ICategoryRepository : IRepository<PostCategory, Guid>
    {
        Task<PageResult<PostCategoryResponse>> GetCategoriesPagingAsync(PagingRequest request);

        Task<PageResult<PostCategoryResponse>> GetActiveCategoriesPagingAsync(
            PagingRequest request
        );
    }
}
