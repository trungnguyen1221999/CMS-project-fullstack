using Application.Contracts.Common;
using Application.Contracts.Posts.Response;
using Domain;

namespace Application.Services.Post
{
    public interface IClientPostService
    {
        Task<PageResult<PostInListResponse>> GetAllPostsAsync(PagingRequest request);

        Task<PostResponse> GetPostByIdAsync(Guid postId);

        Task<PageResult<PostInListResponse>> GetPostsByCategoryAsync(
            string categorySlug,
            PagingRequest request
        );

        Task<PageResult<PostInListResponse>> GetPostsByTagAsync(
            string tagSlug,
            PagingRequest request
        );
    }
}
