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
        public async Task AdminDeletePostAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid() };
            var userId = Guid.NewGuid();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _adminPostService.DeletePostAsync(ids, userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task AdminDeletePostAsync_PostsNotFound_ReturnsFailure()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid() };
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, UserName = "admin" };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockPermissionService.Setup(x => x.HasDeletePostPermission(userId)).Returns(true);

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost>().BuildMockQueryable());

            // Act
            var result = await _adminPostService.DeletePostAsync(ids, userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Post.PostNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task AdminDeletePostAsync_PartialPostsFound_ReturnsFailure()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var ids = new[] { id1, id2 };
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, UserName = "admin" };
            var post = CreateFakePost(id1, userId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockPermissionService.Setup(x => x.HasDeletePostPermission(userId)).Returns(true);

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            // Act
            var result = await _adminPostService.DeletePostAsync(ids, userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Post.PostNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task AdminDeletePostAsync_NoPermissionAndNotAuthor_ReturnsInsufficientPermission()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var ids = new[] { postId };
            var userId = Guid.NewGuid();
            var otherAuthorId = Guid.NewGuid();
            var user = new AppUser { Id = userId, UserName = "author" };
            var post = CreateFakePost(postId, otherAuthorId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockPermissionService.Setup(x => x.HasDeletePostPermission(userId)).Returns(false);

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            // Act
            var result = await _adminPostService.DeletePostAsync(ids, userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Post.InsufficientPostPermission, result.ErrorCode);
        }

        [Fact]
        public async Task AdminDeletePostAsync_AuthorDeletesOwnPost_ReturnsSuccess()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var ids = new[] { postId };
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, UserName = "author" };
            var post = CreateFakePost(postId, userId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockPermissionService.Setup(x => x.HasDeletePostPermission(userId)).Returns(false);

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUnitOfWork.Setup(x => x.Posts.RemoveRange(It.IsAny<IEnumerable<AppPost>>()));
            _mockUnitOfWork.Setup(x => x.PostTags.ClearAllTagsFromPost(postId));
            _mockUnitOfWork.Setup(x => x.PostInSeries.ClearPostFromAllSeries(postId));
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminPostService.DeletePostAsync(ids, userId);

            // Assert
            Assert.True(result.IsSuccess);
            _mockUnitOfWork.Verify(
                x => x.Posts.RemoveRange(It.IsAny<IEnumerable<AppPost>>()),
                Times.Once
            );
            _mockUnitOfWork.Verify(x => x.PostTags.ClearAllTagsFromPost(postId), Times.Once);
            _mockUnitOfWork.Verify(
                x => x.PostInSeries.ClearPostFromAllSeries(postId),
                Times.Once
            );
        }

        [Fact]
        public async Task AdminDeletePostAsync_WithPermission_DeletesOthersPosts()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var ids = new[] { postId };
            var userId = Guid.NewGuid();
            var otherAuthorId = Guid.NewGuid();
            var user = new AppUser { Id = userId, UserName = "admin" };
            var post = CreateFakePost(postId, otherAuthorId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockPermissionService.Setup(x => x.HasDeletePostPermission(userId)).Returns(true);

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUnitOfWork.Setup(x => x.Posts.RemoveRange(It.IsAny<IEnumerable<AppPost>>()));
            _mockUnitOfWork.Setup(x => x.PostTags.ClearAllTagsFromPost(postId));
            _mockUnitOfWork.Setup(x => x.PostInSeries.ClearPostFromAllSeries(postId));
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminPostService.DeletePostAsync(ids, userId);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AdminDeletePostAsync_SaveFails_ReturnsFailure()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var ids = new[] { postId };
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, UserName = "admin" };
            var post = CreateFakePost(postId, userId);

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockPermissionService.Setup(x => x.HasDeletePostPermission(userId)).Returns(true);

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUnitOfWork.Setup(x => x.Posts.RemoveRange(It.IsAny<IEnumerable<AppPost>>()));
            _mockUnitOfWork.Setup(x => x.PostTags.ClearAllTagsFromPost(postId));
            _mockUnitOfWork.Setup(x => x.PostInSeries.ClearPostFromAllSeries(postId));
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _adminPostService.DeletePostAsync(ids, userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Post.DeleteFailed, result.ErrorCode);
        }
    }
}
