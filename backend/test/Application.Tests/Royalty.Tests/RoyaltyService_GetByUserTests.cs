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
        public async Task GetRoyaltyReportByUserAsync_GroupsByUserOnly()
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

            var result = await _royaltyService.GetRoyaltyReportByUserAsync(request, userId);

            // All 4 posts same user → 1 group
            Assert.Single(result);
            var report = result[0];
            Assert.Equal(userId, report.UserId);
            Assert.Equal("trung", report.UserName);
            Assert.Equal(1, report.NumberOfDraftPosts);
            Assert.Equal(2, report.NumberOfPublishPosts);
            Assert.Equal(1, report.NumberOfPaidPublishPosts);
            Assert.Equal(1, report.NumberOfUnpaidPublishPosts);
            Assert.Equal(1, report.NumberOfRejectedPosts);
            Assert.Equal(0, report.NumberOfWaitingApprovalPosts);
        }

        [Fact]
        public async Task GetRoyaltyReportByUserAsync_NoUserId_ReturnsAllUsers()
        {
            var currentUserId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = CreateValidRequest(); // no userId filter

            var posts = new List<AppPost>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    AuthorUserId = currentUserId,
                    AuthorUserName = "trung",
                    Status = PostStatus.Published,
                    IsPaid = true,
                    Name = "Post A",
                    Slug = "post-a",
                    CategoryId = Guid.NewGuid(),
                    CategorySlug = "tech",
                    CategoryName = "Tech",
                    CreatedAt = new DateTime(2025, 1, 10),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    AuthorUserId = otherUserId,
                    AuthorUserName = "minh",
                    Status = PostStatus.Draft,
                    Name = "Post B",
                    Slug = "post-b",
                    CategoryId = Guid.NewGuid(),
                    CategorySlug = "life",
                    CategoryName = "Life",
                    CreatedAt = new DateTime(2025, 2, 5),
                },
            };

            _mockPermissionService
                .Setup(x => x.HasRoyaltyReportViewPermission(currentUserId))
                .Returns(true);

            _mockPostRepository
                .Setup(x => x.FilterByMonth(request))
                .Returns(posts.BuildMockQueryable());

            var result = await _royaltyService.GetRoyaltyReportByUserAsync(request, currentUserId);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetRoyaltyReportByUserAsync_NoPosts_ReturnsEmpty()
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

            var result = await _royaltyService.GetRoyaltyReportByUserAsync(request, userId);

            Assert.Empty(result);
        }
    }
}
