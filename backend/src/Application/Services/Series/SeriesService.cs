using System;
using System.Collections.Generic;
using System.Text;
using Application.Contracts.Common;
using Application.Contracts.Series.Response;
using Application.UnitOfWork;
using Domain;
using Domain.Cores.Content;

namespace Application.Services.Series
{
    public class SeriesService : ISeriesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SeriesService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PageResult<SeriesInListResponse>> GetPublishSeriesPaging(
            PagingRequest request
        )
        {
            var series = await _unitOfWork.Series.GetPublishSeriesPaging(request);
            return series;
        }
    }
}
