using System;
using System.Collections.Generic;
using System.Text;
using Application.Contracts.Common;
using Application.Contracts.Series.Response;
using Application.Repositories;
using AutoMapper;
using Domain;
using Domain.Cores.Content;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SerieRepository : RepositoryBase<Serie, Guid>, ISerieRepository
    {
        private readonly IMapper _mapper;

        public SerieRepository(ApplicationDbContext context, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
        }

        public async Task<PageResult<SeriesInListResponse>> GetPublishSeriesPaging(
            PagingRequest request
        )
        {
            var query = _context.Series.AsQueryable();
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.Name.Contains(request.Keyword));
            }
            query = query.Where(x => x.IsActive);
            var totalCount = await query.CountAsync();
            query = query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.CurrentPage - 1) * request.PageSize)
                .Take(request.PageSize);
            return new PageResult<SeriesInListResponse>
            {
                Result = await _mapper.ProjectTo<SeriesInListResponse>(query).ToListAsync(),
                TotalCount = totalCount,
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
            };
        }
    }
}
