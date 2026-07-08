using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;
using Domain.Cores.Content;
using AppPost = Domain.Cores.Content.Post;

namespace Application.Services.Post
{
    public interface IAdminPostService
    {
        //Read
        Task<PageResult<PostInListResponse>> GetAllPostsAsync(
            GetAllPostsRequest request,
            Guid currentUserId
        );

        Task<AppPost> GetPostByIdAsync(Guid postId, Guid currentUserId);
        Task<string> GetRejectReasonAsync(Guid postId, Guid userId);
        Task<List<PostActivityLog>> GetActivityLogsAsync(Guid postId, Guid userId);

        //Write
        Task CreatePostAsync(CreateUpdatePostRequest request, Guid userId);

        Task UpdatePostAsync(
            CreateUpdatePostRequest request,
            Guid postId,
            Guid userId
        );
        Task DeletePostAsync(Guid[] ids, Guid userId);

        Task ApprovePostAsync(Guid postId, Guid userId, string? note);
        Task RejectPostAsync(Guid postId, Guid userId, string? note);

        Task SubmitPostForApprovalAsync(Guid postId, Guid userId, string? note);
    }
}
