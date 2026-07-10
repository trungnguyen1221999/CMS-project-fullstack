using System;
using System.Collections.Generic;
using System.Text;
using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Series.Response;
using Application.UnitOfWork;
using Domain;
using Domain.Cores.Content;
using Microsoft.EntityFrameworkCore;
using static Application.Exceptions.CustomException;

namespace Application.Services.Series
{
    public class SeriesService : ISeriesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SeriesService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Serie> GetPublishSeriesById(Guid id)
        {
            var series = await _unitOfWork
                .Series.Find(s => s.Id == id && s.IsActive)
                .FirstOrDefaultAsync();
            if (series == null)
                throw new NotFoundException(ErrorMessages.Series.SeriesNotFound);
            return series;
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
