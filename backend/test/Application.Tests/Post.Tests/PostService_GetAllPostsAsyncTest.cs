using System;
using System.Collections.Generic;
using System.Text;
using Application.Constants;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;
using Domain.Cores.Identity;
using Moq;
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
                TotalCount = 1,
                Result = [new PostInListResponse { Name = "Test Post" }],
            };

        [Fact]
        public async Task GetAllPostsAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var request = CreateValidGetAllPostsRequest();

            var userId = Guid.NewGuid();
            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser)null);
            // Act

            var result = await _postService.GetAllPostsAsync(request, userId);
            // Assert

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);

            //Verify
            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockPermissionService.Verify(x => x.HasApprovedPostPermission(userId), Times.Never);
        }

        [Fact]
        public async Task GetAllPostsAsync_UserHasApprovePermission_ReturnsAllPosts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, Email = "admin@test.com" };
            var request = CreateValidGetAllPostsRequest();
            var fakePageResult = CreateFakePageResult();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockPermissionService
                .Setup(x => x.HasApprovedPostPermission(userId))
                .Returns(true);

            _mockUnitOfWork
                .Setup(x => x.Posts.GetAllPostsAsync(request, userId, true))
                .ReturnsAsync(fakePageResult);

            // Act
            var result = await _postService.GetAllPostsAsync(request, userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.TotalCount);

            // Verify: repo called with hasApprovePostPermission = true
            _mockUnitOfWork.Verify(
                x => x.Posts.GetAllPostsAsync(request, userId, true),
                Times.Once);
        }

        [Fact]
        public async Task GetAllPostsAsync_UserNoApprovePermission_ReturnsOwnPosts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, Email = "author@test.com" };
            var request = CreateValidGetAllPostsRequest();
            var fakePageResult = CreateFakePageResult();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockPermissionService
                .Setup(x => x.HasApprovedPostPermission(userId))
                .Returns(false);

            _mockUnitOfWork
                .Setup(x => x.Posts.GetAllPostsAsync(request, userId, false))
                .ReturnsAsync(fakePageResult);

            // Act
            var result = await _postService.GetAllPostsAsync(request, userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);

            // Verify: repo called with hasApprovePostPermission = false
            _mockUnitOfWork.Verify(
                x => x.Posts.GetAllPostsAsync(request, userId, false),
                Times.Once);
        }
    }
}
