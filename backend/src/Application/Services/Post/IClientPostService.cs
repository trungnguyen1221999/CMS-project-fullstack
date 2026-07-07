using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;

namespace Application.Services.Post
{
    public interface IClientPostService
    {
        Task<ReadResponse<PageResult<PostInListResponse>>> GetAllPostsAsync(
            PostPagingRequest request
        );

        Task<ReadResponse<PostResponse>> GetPostByIdAsync(Guid postId);

        Task<ReadResponse<PageResult<PostInListResponse>>> GetPostsByCategoryAsync(
            string categorySlug,
            PostPagingRequest request
        );

        Task<ReadResponse<PageResult<PostInListResponse>>> GetPostsByTagAsync(
            string tagSlug,
            PostPagingRequest request
        );
    }
}
