using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Application.Repositories;
using AutoMapper;
using Domain;
using Domain.Cores.Content;
using Microsoft.EntityFrameworkCore;

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
            if (!hasApprovePostPermission)
                query = query.Where(q => q.AuthorUserId == currentUserId);
            if (!string.IsNullOrEmpty(request.Keyword))
                query = query.Where(q => q.Name.Contains(request.Keyword));
            if (request.CategoryId.HasValue)
                query = query.Where(q => q.CategoryId == request.CategoryId.Value);

            //pagination

            var totalCount = query.Count();
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
