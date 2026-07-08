using System.Linq.Expressions;
using Application.Constants;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using static Application.Exceptions.CustomException;
using AppPost = Domain.Cores.Content.Post;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Post.Tests
{
    public partial class PostServiceTest
    {
        [Fact]
        public async Task AdminDeletePostAsync_UserNotFound_ReturnsFailure()
        {
            var ids = new[] { Guid.NewGuid() };
            var userId = Guid.NewGuid();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _adminPostService.DeletePostAsync(ids, userId)
            );
            Assert.Equal(ErrorMessages.User.UserNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminDeletePostAsync_PostsNotFound_ReturnsFailure()
        {
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

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _adminPostService.DeletePostAsync(ids, userId)
            );
            Assert.Equal(ErrorMessages.Post.PostNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminDeletePostAsync_PartialPostsFound_ReturnsFailure()
        {
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

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _adminPostService.DeletePostAsync(ids, userId)
            );
            Assert.Equal(ErrorMessages.Post.PostNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminDeletePostAsync_NoPermissionAndNotAuthor_ReturnsInsufficientPermission()
        {
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

            var ex = await Assert.ThrowsAsync<ForbiddenException>(
                () => _adminPostService.DeletePostAsync(ids, userId)
            );
            Assert.Equal(ErrorMessages.Post.InsufficientPostPermission, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminDeletePostAsync_AuthorDeletesOwnPost_ReturnsSuccess()
        {
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

            await _adminPostService.DeletePostAsync(ids, userId);

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

            await _adminPostService.DeletePostAsync(ids, userId);
        }

        [Fact]
        public async Task AdminDeletePostAsync_SaveFails_ReturnsFailure()
        {
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

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _adminPostService.DeletePostAsync(ids, userId)
            );
            Assert.Equal(ErrorMessages.Post.DeleteFailed, ex.ErrorCode);
        }
    }
}
