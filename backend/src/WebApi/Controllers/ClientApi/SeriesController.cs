using Application.Contracts.Common;
using Application.Services.Series;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.ClientApi
{
    [Route("api/series")]
    [ApiController]
    [AllowAnonymous]
    public class SeriesController : ControllerBase
    {
        private readonly ISeriesService _seriesService;

        public SeriesController(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        //Read
        [HttpGet]
        public async Task<IActionResult> GetSeriesPaging(PagingRequest request)
        {
            var result = await _seriesService.GetPublishSeriesPaging(request);
            return Ok(result);
        }
    }
}
