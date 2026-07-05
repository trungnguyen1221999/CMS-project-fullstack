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
    }
}
