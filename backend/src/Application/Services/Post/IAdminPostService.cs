using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;
using AppPost = Domain.Cores.Content.Post;

namespace Application.Services.Post
{
    public interface IAdminPostService
    {
        Task<ReadResponse<PageResult<PostInListResponse>>> GetAllPostsAsync(
            GetAllPostsRequest request,
            Guid currentUserId
        );

        Task<ReadResponse<AppPost>> GetPostByIdAsync(Guid postId, Guid currentUserId);

        Task<WriteResponse> CreatePostAsync(CreateUpdatePostRequest request, Guid userId);

        Task<WriteResponse> UpdatePostAsync(
            CreateUpdatePostRequest request,
            Guid postId,
            Guid userId
        );

        Task<WriteResponse> DeletePostAsync(Guid[] ids, Guid userId);

        Task<WriteResponse> ApprovePostAsync(Guid postId, Guid userId, string? note);
        Task<WriteResponse> RejectPostAsync(Guid postId, Guid userId, string? note);

        Task<WriteResponse> SubmitPostForApprovalAsync(Guid postId, Guid userId, string? note);

        Task<ReadResponse<string>> GetRejectReasonAsync(Guid postId, Guid userId);
    }
}
