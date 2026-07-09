using Application.Constants;
using Application.Contracts.Royaltys.Request;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using static Application.Exceptions.CustomException;
using AppPost = Domain.Cores.Content.Post;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Royalty.Tests
{
    public partial class RoyaltyServiceTest
    {
        private static RoyaltyReportByUserAndMonthRequest CreateValidRequest(
            Guid? userId = null
        ) =>
            new()
            {
                UserId = userId,
                FromMonth = 1,
                FromYear = 2025,
                ToMonth = 3,
                ToYear = 2025,
            };

        private static List<AppPost> CreateFakePosts(Guid authorId)
        {
            return
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    AuthorUserId = authorId,
                    AuthorUserName = "trung",
                    Status = PostStatus.Draft,
                    Name = "Draft 1",
                    Slug = "draft-1",
                    CategoryId = Guid.NewGuid(),
                    CategorySlug = "tech",
                    CategoryName = "Tech",
                    CreatedAt = new DateTime(2025, 1, 10),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    AuthorUserId = authorId,
                    AuthorUserName = "trung",
                    Status = PostStatus.Published,
                    IsPaid = true,
                    Name = "Paid Post",
                    Slug = "paid-post",
                    CategoryId = Guid.NewGuid(),
                    CategorySlug = "tech",
                    CategoryName = "Tech",
                    CreatedAt = new DateTime(2025, 1, 15),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    AuthorUserId = authorId,
                    AuthorUserName = "trung",
                    Status = PostStatus.Published,
                    IsPaid = false,
                    Name = "Unpaid Post",
                    Slug = "unpaid-post",
                    CategoryId = Guid.NewGuid(),
                    CategorySlug = "tech",
                    CategoryName = "Tech",
                    CreatedAt = new DateTime(2025, 2, 5),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    AuthorUserId = authorId,
                    AuthorUserName = "trung",
                    Status = PostStatus.Rejected,
                    Name = "Rejected Post",
                    Slug = "rejected-post",
                    CategoryId = Guid.NewGuid(),
                    CategorySlug = "tech",
                    CategoryName = "Tech",
                    CreatedAt = new DateTime(2025, 2, 20),
                },
            ];
        }

        // --- BuildFilteredQueryAsync (shared validation) ---

        [Fact]
        public async Task InvalidDateRange_ThrowsBadRequest()
        {
            var request = new RoyaltyReportByUserAndMonthRequest
            {
                FromMonth = 6,
                FromYear = 2025,
                ToMonth = 1,
                ToYear = 2025,
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () =>
                    _royaltyService.GetRoyaltyReportByUserAndMonthAsync(request, Guid.NewGuid())
            );
            Assert.Equal(ErrorMessages.Royalty.InvalidDateRange, ex.ErrorCode);
        }

        [Fact]
        public async Task SameYearFromMonthGreaterThanToMonth_ThrowsBadRequest()
        {
            var request = new RoyaltyReportByUserAndMonthRequest
            {
                FromMonth = 5,
                FromYear = 2025,
                ToMonth = 3,
                ToYear = 2025,
            };

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _royaltyService.GetRoyaltyReportByUserAsync(request, Guid.NewGuid())
            );
            Assert.Equal(ErrorMessages.Royalty.InvalidDateRange, ex.ErrorCode);
        }

        [Fact]
        public async Task ViewOtherUserReport_NoPermission_ThrowsForbidden()
        {
            var currentUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = CreateValidRequest(otherUserId);

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(currentUserId))
                .Returns(false);

            var ex = await Assert.ThrowsAsync<ForbiddenException>(
                () =>
                    _royaltyService.GetRoyaltyReportByMonthAsync(request, currentUserId)
            );
            Assert.Equal(ErrorMessages.Royalty.InsufficientPermissions, ex.ErrorCode);
        }

        [Fact]
        public async Task ViewOtherUserReport_WithPermission_Success()
        {
            var currentUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = CreateValidRequest(otherUserId);
            var otherUser = new AppUser { Id = otherUserId, UserName = "other" };

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(currentUserId))
                .Returns(true);

            _mockPostRepository
                .Setup(x => x.FilterByMonth(request))
                .Returns(new List<AppPost>().BuildMockQueryable());

            _mockUserManager
                .Setup(x => x.FindByIdAsync(otherUserId.ToString()))
                .ReturnsAsync(otherUser);

            var result = await _royaltyService.GetRoyaltyReportByUserAsync(
                request,
                currentUserId
            );

            Assert.Empty(result);
        }

        [Fact]
        public async Task FilterByUser_UserNotFound_ThrowsNotFound()
        {
            var userId = Guid.NewGuid();
            var request = CreateValidRequest(userId);

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(userId))
                .Returns(true);

            _mockPostRepository
                .Setup(x => x.FilterByMonth(request))
                .Returns(new List<AppPost>().BuildMockQueryable());

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _royaltyService.GetRoyaltyReportByUserAsync(request, userId)
            );
            Assert.Equal(ErrorMessages.User.UserNotFound, ex.ErrorCode);
        }
    }
}
