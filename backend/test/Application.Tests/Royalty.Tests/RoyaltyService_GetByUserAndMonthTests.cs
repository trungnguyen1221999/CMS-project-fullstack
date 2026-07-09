using Application.Contracts.Royaltys.Request;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using AppPost = Domain.Cores.Content.Post;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Royalty.Tests
{
    public partial class RoyaltyServiceTest
    {
        [Fact]
        public async Task GetRoyaltyReportByUserAndMonthAsync_GroupsByUserAndMonth()
        {
            var userId = Guid.NewGuid();
            var request = CreateValidRequest(userId);
            var user = new AppUser { Id = userId, UserName = "trung" };
            var posts = CreateFakePosts(userId);

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(userId))
                .Returns(true);

            _mockPostRepository
                .Setup(x => x.FilterByMonth(request))
                .Returns(posts.BuildMockQueryable());

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            var result = await _royaltyService.GetRoyaltyReportByUserAndMonthAsync(
                request,
                userId
            );

            // 4 posts: 2 in Jan 2025, 2 in Feb 2025 → 2 groups
            Assert.Equal(2, result.Count);

            var jan = result.First(r => r.Month == 1);
            Assert.Equal(2025, jan.Year);
            Assert.Equal(userId, jan.UserId);
            Assert.Equal(1, jan.NumberOfDraftPosts);
            Assert.Equal(1, jan.NumberOfPublishPosts);
            Assert.Equal(1, jan.NumberOfPaidPublishPosts);
            Assert.Equal(0, jan.NumberOfUnpaidPublishPosts);

            var feb = result.First(r => r.Month == 2);
            Assert.Equal(1, feb.NumberOfPublishPosts);
            Assert.Equal(0, feb.NumberOfPaidPublishPosts);
            Assert.Equal(1, feb.NumberOfUnpaidPublishPosts);
            Assert.Equal(1, feb.NumberOfRejectedPosts);
        }

        [Fact]
        public async Task GetRoyaltyReportByUserAndMonthAsync_OrderedByYearAndMonthDesc()
        {
            var userId = Guid.NewGuid();
            var request = CreateValidRequest(userId);
            var user = new AppUser { Id = userId, UserName = "trung" };
            var posts = CreateFakePosts(userId);

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(userId))
                .Returns(true);

            _mockPostRepository
                .Setup(x => x.FilterByMonth(request))
                .Returns(posts.BuildMockQueryable());

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            var result = await _royaltyService.GetRoyaltyReportByUserAndMonthAsync(
                request,
                userId
            );

            Assert.True(result[0].Month >= result[1].Month || result[0].Year > result[1].Year);
        }

        [Fact]
        public async Task GetRoyaltyReportByUserAndMonthAsync_NoPosts_ReturnsEmpty()
        {
            var userId = Guid.NewGuid();
            var request = CreateValidRequest(userId);
            var user = new AppUser { Id = userId, UserName = "trung" };

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(userId))
                .Returns(true);

            _mockPostRepository
                .Setup(x => x.FilterByMonth(request))
                .Returns(new List<AppPost>().BuildMockQueryable());

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            var result = await _royaltyService.GetRoyaltyReportByUserAndMonthAsync(
                request,
                userId
            );

            Assert.Empty(result);
        }
    }
}
