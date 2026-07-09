using Application.Contracts.Royaltys.Request;
using Application.Contracts.Royaltys.Response;

namespace Application.Services.Royalty
{
    public interface IRoyaltyService
    {
        Task<List<RoyaltyReportByUserResponse>> GetRoyaltyReportByUserAsync(
            RoyaltyReportByUserRequest request,
            Guid currentUserId
        );
    }
}
