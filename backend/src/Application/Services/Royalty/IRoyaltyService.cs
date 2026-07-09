using Application.Contracts.Royaltys.Request;
using Application.Contracts.Royaltys.Response;

namespace Application.Services.Royalty
{
    public interface IRoyaltyService
    {
        Task<List<RoyaltyReportByUserAndMonthResponse>> GetRoyaltyReportByUserAndMonthAsync(
            RoyaltyReportByUserAndMonthRequest request,
            Guid currentUserId
        );
        Task<List<RoyaltyReportByUserResponse>> GetRoyaltyReportByUserAsync(
            RoyaltyReportByUserAndMonthRequest request,
            Guid currentUserId
        );
        Task<List<RoyaltyReportByMonthResponse>> GetRoyaltyReportByMonthAsync(
            RoyaltyReportByUserAndMonthRequest request,
            Guid currentUserId
        );
    }
}
