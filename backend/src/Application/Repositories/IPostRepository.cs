using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Application.Contracts.Royaltys.Request;
using Domain;
using Domain.Cores.Content;
using Domain.Cores.Identity;

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
            PagingRequest request
        );

        Task<PageResult<PostInListResponse>> GetPostsByTagAsync(
            string tagSlug,
            PagingRequest request
        );

        Task<PageResult<PostInListResponse>> GetPublishedPostsAsync(PagingRequest request);

        Task<bool> Approve(Post post, User user, string? note);
        Task<bool> Reject(Post post, User user, string? note);

        Task<bool> SubmitForApproval(Post post, User user, string? note);

        IQueryable<Post> FilterByUser(User user);
        IQueryable<Post> FilterByMonth(RoyaltyReportByUserRequest request);
    }
}
