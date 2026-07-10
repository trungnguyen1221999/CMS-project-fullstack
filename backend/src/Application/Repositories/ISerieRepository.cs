using System;
using System.Collections.Generic;
using System.Text;
using Application.Contracts.Common;
using Application.Contracts.Series.Response;
using Domain;
using Domain.Cores.Content;

namespace Application.Repositories
{
    public interface ISerieRepository : IRepository<Serie, Guid>
    {
        Task<PageResult<SeriesInListResponse>> GetPublishSeriesPaging(PagingRequest request);
        Task<PageResult<SeriesInListResponse>> GetSeriesPaging(PagingRequest request);
    }
}
