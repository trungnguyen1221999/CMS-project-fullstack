using Application.Contracts.Common;
using Application.Contracts.Posts.Response;
using Application.Repositories;
using AutoMapper;
using Domain;
using Domain.Cores.Content;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : RepositoryBase<PostCategory, Guid>, ICategoryRepository
    {
        private readonly IMapper _mapper;

        public CategoryRepository(ApplicationDbContext context, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
        }

        public async Task<PageResult<PostCategoryResponse>> GetCategoriesPagingAsync(
            PagingRequest request
        )
        {
            var query = _context.PostCategories.AsQueryable();
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.Name.Contains(request.Keyword));
            }

            return await ToPagedResultAsync(query, request);
        }

        public async Task<PageResult<PostCategoryResponse>> GetActiveCategoriesPagingAsync(
            PagingRequest request
        )
        {
            var query = _context.PostCategories.AsQueryable();
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.Name.Contains(request.Keyword));
            }
            query = query.Where(x => x.IsActive);

            // Apply pagination
            return await ToPagedResultAsync(query, request);
        }

        private async Task<PageResult<PostCategoryResponse>> ToPagedResultAsync(
            IQueryable<PostCategory> query,
            PagingRequest request
        )
        {
            var totalCount = await query.CountAsync();
            query = query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((request.CurrentPage - 1) * request.PageSize)
                .Take(request.PageSize);

            return new PageResult<PostCategoryResponse>
            {
                Result = await _mapper.ProjectTo<PostCategoryResponse>(query).ToListAsync(),
                TotalCount = totalCount,
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
            };
        }
    }
}
