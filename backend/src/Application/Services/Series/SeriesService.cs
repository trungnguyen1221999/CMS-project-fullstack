using System;
using System.Collections.Generic;
using System.Text;
using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Series.Request;
using Application.Contracts.Series.Response;
using Application.Services.Permission;
using Application.UnitOfWork;
using AutoMapper;
using Domain;
using Domain.Constants;
using Domain.Cores.Content;
using Microsoft.EntityFrameworkCore;
using static Application.Exceptions.CustomException;

namespace Application.Services.Series
{
    public class SeriesService : ISeriesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;

        public SeriesService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPermissionService permissionService
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _permissionService = permissionService;
        }

        //Client
        public async Task<List<SeriesInListResponse>> GetAllPublishSeries()
        {
            var series = await _unitOfWork.Series.Find(s => s.IsActive).ToListAsync();
            return _mapper.Map<List<Serie>, List<SeriesInListResponse>>(series);
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

        //Admin
        public async Task<List<SeriesInListResponse>> GetAllSeries(Guid currentUserId)
        {
            var hasPermissionToEditSeries = _permissionService.HasEditSeriesPermission(
                currentUserId
            );
            if (hasPermissionToEditSeries)
            {
                var allSeries = await _unitOfWork.Series.GetAllAsync();
                return _mapper.Map<List<Serie>, List<SeriesInListResponse>>(allSeries.ToList());
            }

            var result = await _unitOfWork
                .Series.Find(s => s.AuthorUserId == currentUserId || s.IsActive)
                .ToListAsync();
            return _mapper.Map<List<Serie>, List<SeriesInListResponse>>(result);
        }

        public async Task<Serie> GetSeriesById(Guid serieId, Guid currentUserId)
        {
            var serie =
                await _unitOfWork.Series.GetByIdAsync(serieId)
                ?? throw new NotFoundException(ErrorMessages.Series.SeriesNotFound);

            var hasPermission = _permissionService.HasEditSeriesPermission(currentUserId);
            if (!serie.IsActive && serie.AuthorUserId != currentUserId && !hasPermission)
            {
                throw new ForbiddenException(ErrorMessages.Series.InsufficientPermissions);
            }

            return serie;
        }

        public async Task<PageResult<SeriesInListResponse>> GetSeriesPaging(
            PagingRequest request,
            Guid currentUserId
        )
        {
            var hasPermission = _permissionService.HasEditSeriesPermission(currentUserId);
            if (hasPermission)
            {
                return await _unitOfWork.Series.GetSeriesPaging(request);
            }
            var series = await _unitOfWork
                .Series.Find(s => s.AuthorUserId == currentUserId || s.IsActive)
                .ToListAsync();
            var totalCount = series.Count;
            var pagedSeries = series
                .Skip((request.CurrentPage - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            return new PageResult<SeriesInListResponse>()
            {
                Result = _mapper.Map<List<Serie>, List<SeriesInListResponse>>(pagedSeries),
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                TotalCount = totalCount,
            };
        }

        public async Task<Serie> CreateSeries(CreateUpdateSeriesRequest request, Guid currentUserId)
        {
            var existingSerie = await _unitOfWork
                .Series.Find(s => s.Slug == request.Slug)
                .FirstOrDefaultAsync();
            if (existingSerie != null)
            {
                throw new BadRequestException(ErrorMessages.Series.SlugAlreadyExists);
            }
            var serie = _mapper.Map<CreateUpdateSeriesRequest, Serie>(request);
            serie.AuthorUserId = currentUserId;
            _unitOfWork.Series.Add(serie);
            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
            {
                throw new BadRequestException(ErrorMessages.Series.CreateFailed);
            }
            return serie;
        }

        public async Task<Serie> UpdateSeries(
            Guid seriesId,
            CreateUpdateSeriesRequest request,
            Guid currentUserId
        )
        {
            var existingSerie = await _unitOfWork.Series.GetByIdAsync(seriesId);
            if (existingSerie == null)
                throw new NotFoundException(ErrorMessages.Series.SeriesNotFound);

            var hasPermission = _permissionService.HasEditSeriesPermission(currentUserId);
            if (!hasPermission && existingSerie.AuthorUserId != currentUserId)
            {
                throw new ForbiddenException(ErrorMessages.Series.InsufficientPermissions);
            }

            existingSerie = _mapper.Map(request, existingSerie);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
            {
                throw new BadRequestException(ErrorMessages.Series.UpdateFailed);
            }
            return existingSerie;
        }
    }
}
