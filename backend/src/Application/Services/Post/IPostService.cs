using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;
using AppPost = Domain.Cores.Content.Post;

namespace Application.Services.Post
{
    public interface IPostService
    {
        // Admin
        Task<ReadResponse<PageResult<PostInListResponse>>> AdminGetAllPostsAsync(
            GetAllPostsRequest request,
            Guid currentUserId
        );

        Task<ReadResponse<AppPost>> AdminGetPostByIdAsync(Guid postId, Guid currentUserId);

        Task<WriteResponse> AdminCreatePostAsync(CreateUpdatePostRequest request, Guid userId);

        Task<WriteResponse> AdminUpdatePostAsync(
            CreateUpdatePostRequest request,
            Guid postId,
            Guid userId
        );

        // Client
        Task<ReadResponse<PageResult<PostInListResponse>>> ClientGetAllPostsAsync(
            PostPagingRequest request
        );

        Task<ReadResponse<PostResponse>> ClientGetPostByIdAsync(Guid postId);

        Task<ReadResponse<PageResult<PostInListResponse>>> ClientGetPostsByCategoryAsync(
            string categorySlug,
            PostPagingRequest request
        );

        Task<ReadResponse<PageResult<PostInListResponse>>> ClientGetPostsByTagAsync(
            string tagSlug,
            PostPagingRequest request
        );

        Task<WriteResponse> AdminDeletePostAsync(Guid[] ids, Guid userId);
    }
}
