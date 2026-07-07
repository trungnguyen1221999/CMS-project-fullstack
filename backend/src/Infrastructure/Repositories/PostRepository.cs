using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Application.Repositories;
using AutoMapper;
using Domain;
using Domain.Cores.Content;
using Microsoft.EntityFrameworkCore;
using AppUser = Domain.Cores.Identity.User;

namespace Infrastructure.Repositories
{
    public class PostRepository : RepositoryBase<Post, Guid>, IPostRepository
    {
        private readonly IMapper _mapper;

        public PostRepository(ApplicationDbContext context, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
        }

        public async Task<PageResult<PostInListResponse>> GetAllPostsAsync(
            GetAllPostsRequest request,
            Guid currentUserId,
            bool hasApprovePostPermission
        )
        {
            var query = _context.Posts.AsQueryable();

            if (hasApprovePostPermission)
            {
                // Editor/Admin: own posts (all statuses) + others' posts (only WaitingForApproval, Published, Rejected)
                query = query.Where(q =>
                    q.AuthorUserId == currentUserId
                    || q.Status == PostStatus.WaitingForApproval
                    || q.Status == PostStatus.Published
                    || q.Status == PostStatus.Rejected
                );
            }
            else
            {
                // Author: own posts only (all statuses)
                query = query.Where(q => q.AuthorUserId == currentUserId);
            }

            if (!string.IsNullOrEmpty(request.Keyword))
                query = query.Where(q => q.Name.Contains(request.Keyword));
            if (request.CategoryId.HasValue)
                query = query.Where(q => q.CategoryId == request.CategoryId.Value);

            return await ToPagedResultAsync(query, request);
        }

        public async Task<PageResult<PostInListResponse>> GetPostsByCategoryAsync(
            string categorySlug,
            PostPagingRequest request
        )
        {
            var query = _context.Posts.Where(q => q.Status == PostStatus.Published);
            if (!string.IsNullOrEmpty(categorySlug))
                query = query.Where(q => q.CategorySlug == categorySlug);

            return await ToPagedResultAsync(query, request);
        }

        public async Task<PageResult<PostInListResponse>> GetPostsByTagAsync(
            string tagSlug,
            PostPagingRequest request
        )
        {
            var query = _context.Posts.Where(q => q.Status == PostStatus.Published);
            if (!string.IsNullOrEmpty(tagSlug))
                query = query.Where(q => q.Tags != null && q.Tags.Contains(tagSlug));

            return await ToPagedResultAsync(query, request);
        }

        public async Task<PageResult<PostInListResponse>> GetPublishedPostsAsync(
            PostPagingRequest request
        )
        {
            var query = _context.Posts.Where(q => q.Status == PostStatus.Published);
            return await ToPagedResultAsync(query, request);
        }

        public async Task<bool> Approve(Guid postId, Guid userId, string? note) =>
            await ChangePostStatusAsync(postId, userId, PostStatus.Published, note);

        public async Task<bool> Reject(Guid postId, Guid userId, string? note) =>
            await ChangePostStatusAsync(postId, userId, PostStatus.Rejected, note);

        public async Task<bool> SubmitForApproval(Guid postId, Guid userId, string? note) =>
            await ChangePostStatusAsync(postId, userId, PostStatus.WaitingForApproval, note);

        private async Task<bool> ChangePostStatusAsync(
            Guid postId,
            Guid userId,
            PostStatus newStatus,
            string? note
        )
        {
            var post = await FindPostByIdAsync(postId);
            if (post == null)
                return false;
            var user = await FindUserByIdAsync(userId);
            if (user == null)
                return false;

            _context.PostActivityLogs.Add(
                new PostActivityLog
                {
                    FromStatus = post.Status,
                    ToStatus = newStatus,
                    UserName = user.UserName,
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Note = note,
                }
            );
            post.Status = newStatus;

            _context.Posts.Update(post);
            return true;
        }

        private async Task<Post?> FindPostByIdAsync(Guid postId)
        {
            return await _context.Posts.FindAsync(postId);
        }

        private async Task<AppUser?> FindUserByIdAsync(Guid userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        private async Task<PageResult<PostInListResponse>> ToPagedResultAsync(
            IQueryable<Post> query,
            PostPagingRequest request
        )
        {
            var totalCount = await query.CountAsync();
            query = query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((request.CurrentPage - 1) * request.PageSize)
                .Take(request.PageSize);

            return new PageResult<PostInListResponse>
            {
                Result = await _mapper.ProjectTo<PostInListResponse>(query).ToListAsync(),
                TotalCount = totalCount,
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
            };
        }
    }
}
