using Application.Contracts.Royaltys.Request;
using Application.Services.Royalty;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using WebApi.Extensions;

namespace WebApi.Controllers.AdminApi
{
    [Route("api/admin/royalty")]
    [ApiController]
    [Authorize]
    public class AdminRoyaltyController : ControllerBase
    {
        private readonly IRoyaltyService _royaltyService;

        public AdminRoyaltyController(IRoyaltyService royaltyService)
        {
            _royaltyService = royaltyService;
        }

        //Read
        [HttpGet("report-by-user")]
        public async Task<ActionResult> GetRoyaltyReportByUser(
            [FromQuery] RoyaltyReportByUserRequest request
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _royaltyService.GetRoyaltyReportByUserAsync(request, currentUserId);
            return Ok(result);
        }

        //Write
    }
}
