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
    }
}
