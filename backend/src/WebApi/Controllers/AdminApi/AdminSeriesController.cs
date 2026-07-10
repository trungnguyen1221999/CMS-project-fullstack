using Application.Contracts.Common;
using Application.Contracts.Series.Request;
using Application.Services.Series;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers.AdminApi
{
    [Route("api/admin/series")]
    [ApiController]
    [Authorize]
    public class AdminSeriesController : ControllerBase
    {
        private readonly ISeriesService _seriesService;

        public AdminSeriesController(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        //Read
        [HttpGet]
        [HasPermission(Permissions.Series.View)]
        public async Task<IActionResult> GetSeriesPaging([FromQuery] PagingRequest request)
        {
            var currentUserId = User.GetUserId();
            var result = await _seriesService.GetSeriesPaging(request, currentUserId);
            return Ok(result);
        }

        [HttpGet("{seriesId}")]
        [HasPermission(Permissions.Series.View)]
        public async Task<IActionResult> GetSeriesById([FromRoute] Guid seriesId)
        {
            var currentUserId = User.GetUserId();
            var result = await _seriesService.GetSeriesById(seriesId, currentUserId);
            return Ok(result);
        }

        [HttpGet("all")]
        [HasPermission(Permissions.Series.View)]
        public async Task<IActionResult> GetAllSeries()
        {
            var currentUserId = User.GetUserId();
            var result = await _seriesService.GetAllSeries(currentUserId);
            return Ok(result);
        }

        //Write
        [HttpPost]
        [HasPermission(Permissions.Series.Create)]
        public async Task<IActionResult> CreateSeries([FromBody] CreateUpdateSeriesRequest request)
        {
            var userId = User.GetUserId();
            await _seriesService.CreateSeries(request, userId);
            return Ok(WriteResponse.Success());
        }
    }
}
