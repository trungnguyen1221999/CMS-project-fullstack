using System;
using System.Collections.Generic;
using System.Text;
using Application.Contracts.Common;
using Application.Contracts.Series.Response;
using Domain;
using Domain.Cores.Content;

namespace Application.Services.Series
{
    public interface ISeriesService
    {
        Task<PageResult<SeriesInListResponse>> GetPublishSeriesPaging(PagingRequest request);
    }
}
