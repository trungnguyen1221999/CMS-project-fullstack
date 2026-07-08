using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;

namespace Application.Services.Post
{
    public interface IClientPostService
    {
        Task<PageResult<PostInListResponse>> GetAllPostsAsync(
            PostPagingRequest request
        );

        Task<PostResponse> GetPostByIdAsync(Guid postId);

        Task<PageResult<PostInListResponse>> GetPostsByCategoryAsync(
            string categorySlug,
            PostPagingRequest request
        );

        Task<PageResult<PostInListResponse>> GetPostsByTagAsync(
            string tagSlug,
            PostPagingRequest request
        );
    }
}
