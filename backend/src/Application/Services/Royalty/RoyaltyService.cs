using Application.Constants;
using Application.Contracts.Royaltys.Request;
using Application.Contracts.Royaltys.Response;
using Application.Services.Permission;
using Application.UnitOfWork;
using Domain.Cores.Content;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Application.Exceptions.CustomException;
using AppPost = Domain.Cores.Content.Post;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Services.Royalty
{
    public class RoyaltyService : IRoyaltyService
    {
        private readonly IPermissionService _permissionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public RoyaltyService(
            IPermissionService permissionService,
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager
        )
        {
            _permissionService = permissionService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<List<RoyaltyReportByUserAndMonthResponse>> GetRoyaltyReportByUserAndMonthAsync(
            RoyaltyReportByUserAndMonthRequest request,
            Guid currentUserId
        )
        {
            var query = await BuildFilteredQueryAsync(request, currentUserId);

            return await query
                .GroupBy(p => new
                {
                    p.AuthorUserId,
                    p.AuthorUserName,
                    p.CreatedAt.Month,
                    p.CreatedAt.Year,
                })
                .Select(g => new RoyaltyReportByUserAndMonthResponse
                {
                    UserId = g.Key.AuthorUserId,
                    UserName = g.Key.AuthorUserName ?? string.Empty,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    NumberOfDraftPosts = g.Count(p => p.Status == PostStatus.Draft),
                    NumberOfWaitingApprovalPosts = g.Count(p =>
                        p.Status == PostStatus.WaitingForApproval
                    ),
                    NumberOfRejectedPosts = g.Count(p => p.Status == PostStatus.Rejected),
                    NumberOfPublishPosts = g.Count(p => p.Status == PostStatus.Published),
                    NumberOfPaidPublishPosts = g.Count(p =>
                        p.Status == PostStatus.Published && p.IsPaid
                    ),
                    NumberOfUnpaidPublishPosts = g.Count(p =>
                        p.Status == PostStatus.Published && !p.IsPaid
                    ),
                })
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .ToListAsync();
        }

        public async Task<List<RoyaltyReportByUserResponse>> GetRoyaltyReportByUserAsync(
            RoyaltyReportByUserAndMonthRequest request,
            Guid currentUserId
        )
        {
            var query = await BuildFilteredQueryAsync(request, currentUserId);

            return await query
                .GroupBy(p => new { p.AuthorUserId, p.AuthorUserName })
                .Select(g => new RoyaltyReportByUserResponse
                {
                    UserId = g.Key.AuthorUserId,
                    UserName = g.Key.AuthorUserName ?? string.Empty,
                    NumberOfDraftPosts = g.Count(p => p.Status == PostStatus.Draft),
                    NumberOfWaitingApprovalPosts = g.Count(p =>
                        p.Status == PostStatus.WaitingForApproval
                    ),
                    NumberOfRejectedPosts = g.Count(p => p.Status == PostStatus.Rejected),
                    NumberOfPublishPosts = g.Count(p => p.Status == PostStatus.Published),
                    NumberOfPaidPublishPosts = g.Count(p =>
                        p.Status == PostStatus.Published && p.IsPaid
                    ),
                    NumberOfUnpaidPublishPosts = g.Count(p =>
                        p.Status == PostStatus.Published && !p.IsPaid
                    ),
                })
                .ToListAsync();
        }

        public async Task<List<RoyaltyReportByMonthResponse>> GetRoyaltyReportByMonthAsync(
            RoyaltyReportByUserAndMonthRequest request,
            Guid currentUserId
        )
        {
            var query = await BuildFilteredQueryAsync(request, currentUserId);

            return await query
                .GroupBy(p => new { p.CreatedAt.Month, p.CreatedAt.Year })
                .Select(g => new RoyaltyReportByMonthResponse
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    NumberOfDraftPosts = g.Count(p => p.Status == PostStatus.Draft),
                    NumberOfWaitingApprovalPosts = g.Count(p =>
                        p.Status == PostStatus.WaitingForApproval
                    ),
                    NumberOfRejectedPosts = g.Count(p => p.Status == PostStatus.Rejected),
                    NumberOfPublishPosts = g.Count(p => p.Status == PostStatus.Published),
                    NumberOfPaidPublishPosts = g.Count(p =>
                        p.Status == PostStatus.Published && p.IsPaid
                    ),
                    NumberOfUnpaidPublishPosts = g.Count(p =>
                        p.Status == PostStatus.Published && !p.IsPaid
                    ),
                })
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .ToListAsync();
        }

        private async Task<IQueryable<AppPost>> BuildFilteredQueryAsync(
            RoyaltyReportByUserAndMonthRequest request,
            Guid currentUserId
        )
        {
            if (
                request.FromYear > request.ToYear
                || (request.FromYear == request.ToYear && request.FromMonth > request.ToMonth)
            )
            {
                throw new BadRequestException(ErrorMessages.Royalty.InvalidDateRange);
            }

            var hasPermission = _permissionService.HasRoyaltyReportViewPermission(currentUserId);
            if (request.UserId != currentUserId && !hasPermission)
            {
                throw new ForbiddenException(ErrorMessages.Royalty.InsufficientPermissions);
            }

            var query = _unitOfWork.Posts.FilterByMonth(request);

            if (request.UserId.HasValue)
            {
                var user =
                    await _userManager.FindByIdAsync(request.UserId.Value.ToString())
                    ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

                query = query.Where(p => p.AuthorUserId == user.Id);
            }

            return query;
        }
    }
}
