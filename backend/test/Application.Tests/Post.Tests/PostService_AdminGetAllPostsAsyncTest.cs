using Application.Constants;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;
using Domain.Cores.Content;
using Moq;
using static Application.Exceptions.CustomException;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Post.Tests
{
    public partial class PostServiceTest
    {
        private static GetAllPostsRequest CreateValidGetAllPostsRequest() =>
            new()
            {
                Keyword = "test",
                CategoryId = Guid.NewGuid(),
                CurrentPage = 1,
                PageSize = 10,
            };

        private static PageResult<PostInListResponse> CreateFakePageResult() =>
            new()
            {
                CurrentPage = 1,
                PageSize = 10,
                TotalCount = 4,
                Result = new List<PostInListResponse>
                {
                    new PostInListResponse { Name = "Test Post Draft", Status = PostStatus.Draft },
                    new PostInListResponse
                    {
                        Name = "Test Post Pending",
                        Status = PostStatus.WaitingForApproval,
                    },
                    new PostInListResponse
                    {
                        Name = "Test Post Published",
                        Status = PostStatus.Published,
                    },
                    new PostInListResponse
                    {
                        Name = "Test Post Rejected",
                        Status = PostStatus.Rejected,
                    },
                },
            };

        [Fact]
        public async Task AdminGetAllPostsAsync_UserNotFound_ReturnsFailure()
        {
            var request = CreateValidGetAllPostsRequest();
            var userId = Guid.NewGuid();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _adminPostService.GetAllPostsAsync(request, userId)
            );
            Assert.Equal(ErrorMessages.User.UserNotFound, ex.ErrorCode);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockPermissionService.Verify(x => x.HasApprovedPostPermission(userId), Times.Never);
        }

        [Fact]
        public async Task AdminGetAllPostsAsync_UserHasApprovePermission_ReturnsAllNonDraftPosts()
        {
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, Email = "admin@test.com" };
            var request = CreateValidGetAllPostsRequest();
            var fakePageResult = CreateFakePageResult();

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            _mockPermissionService.Setup(x => x.HasApprovedPostPermission(userId)).Returns(true);

            _mockUnitOfWork
                .Setup(x => x.Posts.GetAllPostsAsync(request, userId, true))
                .ReturnsAsync(fakePageResult);

            var result = await _adminPostService.GetAllPostsAsync(request, userId);

            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount - 1);

            _mockUnitOfWork.Verify(
                x => x.Posts.GetAllPostsAsync(request, userId, true),
                Times.Once
            );
        }

        [Fact]
        public async Task AdminGetAllPostsAsync_UserNoApprovePermission_ReturnsOwnPosts()
        {
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, Email = "author@test.com" };
            var request = CreateValidGetAllPostsRequest();
            var fakePageResult = CreateFakePageResult();

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            _mockPermissionService.Setup(x => x.HasApprovedPostPermission(userId)).Returns(false);

            _mockUnitOfWork
                .Setup(x => x.Posts.GetAllPostsAsync(request, userId, false))
                .ReturnsAsync(fakePageResult);

            var result = await _adminPostService.GetAllPostsAsync(request, userId);

            Assert.NotNull(result);
            Assert.Equal(4, result.TotalCount);

            _mockUnitOfWork.Verify(
                x => x.Posts.GetAllPostsAsync(request, userId, false),
                Times.Once
            );
        }
    }
}
