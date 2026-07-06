using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;

namespace Application.Services.Post
{
    public interface IPostService
    {
        Task<ReadResponse<PageResult<PostInListResponse>>> GetAllPostsAsync(
            GetAllPostsRequest request,
            Guid currentUserId
        );

        Task<ReadResponse<PageResult<PostInListResponse>>> GetPostsByCategoryAsync(
            string categorySlug,
            PostPagingRequest request
        );

        Task<ReadResponse<PageResult<PostInListResponse>>> GetPostsByTagAsync(
            string tagSlug,
            PostPagingRequest request
        );

        Task<ReadResponse<PageResult<PostInListResponse>>> GetPublishedPostsAsync(
            PostPagingRequest request
        );

        Task<WriteResponse> CreatePostAsync(CreateUpdatePostRequest request, Guid userId);
    }
}
