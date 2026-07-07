using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;
using Domain.Cores.Content;

namespace Application.Repositories
{
    public interface IPostRepository : IRepository<Post, Guid>
    {
        Task<PageResult<PostInListResponse>> GetAllPostsAsync(
            GetAllPostsRequest request,
            Guid currentUserId,
            bool hasApprovePostPermission
        );

        Task<PageResult<PostInListResponse>> GetPostsByCategoryAsync(
            string categorySlug,
            PostPagingRequest request
        );

        Task<PageResult<PostInListResponse>> GetPostsByTagAsync(
            string tagSlug,
            PostPagingRequest request
        );

        Task<PageResult<PostInListResponse>> GetPublishedPostsAsync(PostPagingRequest request);

        Task<bool> Approve(Guid postId, Guid userId, string? note);
        Task<bool> Reject(Guid postId, Guid userId, string? note);

        Task<bool> SubmitForApproval(Guid postId, Guid userId, string? note);
    }
}
