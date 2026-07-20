using System;
using System.Collections.Generic;
using System.Text;
using Application.Contracts.Common;
using Application.Contracts.Series.Request;
using Application.Contracts.Series.Response;
using Domain;
using Domain.Cores.Content;

namespace Application.Services.Series
{
    public interface ISeriesService
    {
        //Client
        Task<PageResult<SeriesInListResponse>> GetPublishSeriesPaging(PagingRequest request);

        Task<Serie> GetPublishSeriesById(Guid id);
        Task<List<SeriesInListResponse>> GetAllPublishSeries();

        //Admin
        Task<PageResult<SeriesInListResponse>> GetSeriesPaging(
            PagingRequest request,
            Guid currentUserId
        );

        Task<Serie> GetSeriesById(Guid seriesId, Guid currentUserId);
        Task<List<SeriesInListResponse>> GetAllSeries(Guid currentUserId);
        Task<Serie> CreateSeries(CreateUpdateSeriesRequest request, Guid currentUserId);
        Task<Serie> UpdateSeries(
            Guid seriesId,
            CreateUpdateSeriesRequest request,
            Guid currentUserId
        );
    }
}
