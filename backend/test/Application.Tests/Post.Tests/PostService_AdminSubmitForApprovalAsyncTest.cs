using System.Linq.Expressions;
using Application.Constants;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using AppPost = Domain.Cores.Content.Post;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Post.Tests
{
    public partial class PostServiceTest
    {
        [Fact]
        public async Task AdminSubmitPostForApprovalAsync_PostNotFound_ReturnsFailure()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost>().BuildMockQueryable());

            // Act
            var result = await _adminPostService.SubmitPostForApprovalAsync(
                postId,
                userId,
                "please review"
            );

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Post.PostNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task AdminSubmitPostForApprovalAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var post = CreateFakePost(postId, userId, PostStatus.Draft);

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _adminPostService.SubmitPostForApprovalAsync(
                postId,
                userId,
                null
            );

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task AdminSubmitPostForApprovalAsync_NotAuthor_ReturnsInsufficientPermission()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherAuthorId = Guid.NewGuid();
            var post = CreateFakePost(postId, otherAuthorId, PostStatus.Draft);
            var user = new AppUser { Id = userId, UserName = "otheruser" };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            // Act
            var result = await _adminPostService.SubmitPostForApprovalAsync(
                postId,
                userId,
                null
            );

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Post.InsufficientPostPermission, result.ErrorCode);
        }

        [Fact]
        public async Task AdminSubmitPostForApprovalAsync_Success_ReturnsSuccess()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var post = CreateFakePost(postId, userId, PostStatus.Draft);
            var user = new AppUser { Id = userId, UserName = "author" };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUnitOfWork
                .Setup(x => x.Posts.SubmitForApproval(post, user, "please review"))
                .ReturnsAsync(true);

            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminPostService.SubmitPostForApprovalAsync(
                postId,
                userId,
                "please review"
            );

            // Assert
            Assert.True(result.IsSuccess);
            _mockUnitOfWork.Verify(
                x => x.Posts.SubmitForApproval(post, user, "please review"),
                Times.Once
            );
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AdminSubmitPostForApprovalAsync_SaveFails_ReturnsFailure()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var post = CreateFakePost(postId, userId, PostStatus.Draft);
            var user = new AppUser { Id = userId, UserName = "author" };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUnitOfWork
                .Setup(x => x.Posts.SubmitForApproval(post, user, null))
                .ReturnsAsync(true);

            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _adminPostService.SubmitPostForApprovalAsync(
                postId,
                userId,
                null
            );

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Post.SubmitForApprovalFailed, result.ErrorCode);
        }
    }
}
