using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Application.Contracts.Royaltys.Request;
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
            PagingRequest request
        )
        {
            var query = _context.Posts.Where(q => q.Status == PostStatus.Published);
            if (!string.IsNullOrEmpty(categorySlug))
                query = query.Where(q => q.CategorySlug == categorySlug);
            if (!string.IsNullOrEmpty(request.Keyword))
                query = query.Where(q => q.Name.Contains(request.Keyword));
            return await ToPagedResultAsync(query, request);
        }

        public async Task<PageResult<PostInListResponse>> GetPostsByTagAsync(
            string tagSlug,
            PagingRequest request
        )
        {
            var query = _context.Posts.Where(q => q.Status == PostStatus.Published);
            if (!string.IsNullOrEmpty(tagSlug))
                query = query.Where(q => q.Tags != null && q.Tags.Contains(tagSlug));
            if (!string.IsNullOrEmpty(request.Keyword))
                query = query.Where(q => q.Name.Contains(request.Keyword));
            return await ToPagedResultAsync(query, request);
        }

        public async Task<PageResult<PostInListResponse>> GetPublishedPostsAsync(
            PagingRequest request
        )
        {
            var query = _context.Posts.Where(q => q.Status == PostStatus.Published);
            if (!string.IsNullOrEmpty(request.Keyword))
                query = query.Where(q => q.Name.Contains(request.Keyword));
            return await ToPagedResultAsync(query, request);
        }

        public Task<bool> Approve(Post post, AppUser user, string? note) =>
            ChangePostStatusAsync(post, user, PostStatus.Published, note);

        public Task<bool> Reject(Post post, AppUser user, string? note) =>
            ChangePostStatusAsync(post, user, PostStatus.Rejected, note);

        public Task<bool> SubmitForApproval(Post post, AppUser user, string? note) =>
            ChangePostStatusAsync(post, user, PostStatus.WaitingForApproval, note);

        private Task<bool> ChangePostStatusAsync(
            Post post,
            AppUser user,
            PostStatus newStatus,
            string? note
        )
        {
            _context.PostActivityLogs.Add(
                new PostActivityLog
                {
                    FromStatus = post.Status,
                    ToStatus = newStatus,
                    UserName = user.UserName,
                    PostId = post.Id,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    Note = note,
                }
            );
            post.Status = newStatus;

            _context.Posts.Update(post);
            return Task.FromResult(true);
        }

        private async Task<PageResult<PostInListResponse>> ToPagedResultAsync(
            IQueryable<Post> query,
            PagingRequest request
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

        public IQueryable<Post> FilterByUser(AppUser user)
        {
            var query = _context.Posts.AsQueryable();
            query = query.Where(q => q.AuthorUserId == user.Id);

            return query;
        }

        public IQueryable<Post> FilterByMonth(RoyaltyReportByUserAndMonthRequest request)
        {
            var fromDate = new DateTime(request.FromYear, request.FromMonth, 1);
            var toDate = new DateTime(request.ToYear, request.ToMonth, 1).AddMonths(1);

            return _context.Posts.Where(q => q.CreatedAt >= fromDate && q.CreatedAt < toDate);
        }

        public async Task<List<Post>> GetListUnpaidPublishPosts(Guid userId)
        {
            return await _context.Posts
                .Where(p => p.AuthorUserId == userId
                    && p.Status == PostStatus.Published
                    && !p.IsPaid)
                .ToListAsync();
        }
    }
}
