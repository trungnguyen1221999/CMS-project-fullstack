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
        public async Task GetRoyaltyReportByMonthAsync_GroupsByMonthOnly()
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

            var result = await _royaltyService.GetRoyaltyReportByMonthAsync(request, userId);

            // 2 in Jan, 2 in Feb → 2 groups
            Assert.Equal(2, result.Count);

            var jan = result.First(r => r.Month == 1);
            Assert.Equal(2025, jan.Year);
            Assert.Equal(1, jan.NumberOfDraftPosts);
            Assert.Equal(1, jan.NumberOfPublishPosts);

            var feb = result.First(r => r.Month == 2);
            Assert.Equal(1, feb.NumberOfUnpaidPublishPosts);
            Assert.Equal(1, feb.NumberOfRejectedPosts);
        }

        [Fact]
        public async Task GetRoyaltyReportByMonthAsync_OrderedDesc()
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

            var result = await _royaltyService.GetRoyaltyReportByMonthAsync(request, userId);

            Assert.Equal(2, result[0].Month); // Feb first
            Assert.Equal(1, result[1].Month); // Jan second
        }

        [Fact]
        public async Task GetRoyaltyReportByMonthAsync_NoPosts_ReturnsEmpty()
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

            var result = await _royaltyService.GetRoyaltyReportByMonthAsync(request, userId);

            Assert.Empty(result);
        }
    }
}
