using Application.Contracts.Common;
using Application.Contracts.Royaltys.Request;
using Application.Services.Royalty;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
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
        [HttpGet("report-by-user-and-month")]
        public async Task<ActionResult> GetRoyaltyReportByUserAndMonth(
            [FromQuery] RoyaltyReportByUserAndMonthRequest request
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _royaltyService.GetRoyaltyReportByUserAndMonthAsync(
                request,
                currentUserId
            );
            return Ok(result);
        }

        [HttpGet("report-by-user")]
        public async Task<ActionResult> GetRoyaltyReportByUser(
            [FromQuery] RoyaltyReportByUserAndMonthRequest request
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _royaltyService.GetRoyaltyReportByUserAsync(request, currentUserId);
            return Ok(result);
        }

        [HttpGet("report-by-month")]
        public async Task<ActionResult> GetRoyaltyReportByMonth(
            [FromQuery] RoyaltyReportByUserAndMonthRequest request
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _royaltyService.GetRoyaltyReportByMonthAsync(request, currentUserId);
            return Ok(result);
        }

        [HttpGet("transaction-histories")]
        public async Task<ActionResult> GetTransactionHistory(
            [FromQuery] TransactionHistoryRequest request
        )
        {
            var currentUserId = User.GetUserId();
            var result = await _royaltyService.GetTransactionHistoryAsync(request, currentUserId);
            return Ok(result);
        }

        //Write

        [HttpPost("pay-royalty/{toUserId}")]
        [HasPermission(Permissions.Royalty.Pay)]
        public async Task<IActionResult> PayRoyalty(Guid toUserId)
        {
            var fromUserId = User.GetUserId();
            await _royaltyService.PayRoyaltyForUserAsync(fromUserId, toUserId);
            return Ok(WriteResponse.Success());
        }
    }
}
