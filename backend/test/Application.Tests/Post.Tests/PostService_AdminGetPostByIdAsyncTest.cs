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
        private static AppPost CreateFakePost(
            Guid postId,
            Guid authorUserId,
            PostStatus status = PostStatus.Published
        )
        {
            return new AppPost
            {
                Id = postId,
                AuthorUserId = authorUserId,
                Status = status,
                Name = "Test Post",
                Slug = "test-post",
                CategoryId = Guid.NewGuid(),
                CategorySlug = "test-cat",
                CategoryName = "Test Category",
            };
        }

        [Fact]
        public async Task AdminGetPostByIdAsync_PostNotFound_ReturnsFailure()
        {
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost>().BuildMockQueryable());

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _adminPostService.GetPostByIdAsync(postId, userId)
            );
            Assert.Equal(ErrorMessages.Post.PostNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminGetPostByIdAsync_UserNotFound_ReturnsFailure()
        {
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var post = CreateFakePost(postId, Guid.NewGuid());

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _adminPostService.GetPostByIdAsync(postId, userId)
            );
            Assert.Equal(ErrorMessages.User.UserNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminGetPostByIdAsync_AuthorViewsOwnDraft_ReturnsSuccess()
        {
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var post = CreateFakePost(postId, userId, PostStatus.Draft);
            var user = new AppUser { Id = userId, Email = "author@test.com" };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            var result = await _adminPostService.GetPostByIdAsync(postId, userId);

            Assert.Equal(postId, result.Id);
        }

        [Fact]
        public async Task AdminGetPostByIdAsync_NonAuthorViewsDraft_ReturnsPostNotFound()
        {
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var post = CreateFakePost(postId, authorId, PostStatus.Draft);
            var user = new AppUser { Id = userId, Email = "editor@test.com" };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _adminPostService.GetPostByIdAsync(postId, userId)
            );
            Assert.Equal(ErrorMessages.Post.PostNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminGetPostByIdAsync_EditorWithPermission_ReturnsSuccess()
        {
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var post = CreateFakePost(postId, authorId, PostStatus.WaitingForApproval);
            var user = new AppUser { Id = userId, Email = "editor@test.com" };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            _mockPermissionService.Setup(x => x.HasApprovedPostPermission(userId)).Returns(true);

            var result = await _adminPostService.GetPostByIdAsync(postId, userId);

            Assert.Equal(postId, result.Id);
        }

        [Fact]
        public async Task AdminGetPostByIdAsync_UserWithoutPermission_ReturnsInsufficientPermission()
        {
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var post = CreateFakePost(postId, authorId, PostStatus.Published);
            var user = new AppUser { Id = userId, Email = "viewer@test.com" };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            _mockPermissionService.Setup(x => x.HasApprovedPostPermission(userId)).Returns(false);

            var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
                _adminPostService.GetPostByIdAsync(postId, userId)
            );
            Assert.Equal(ErrorMessages.Post.InsufficientPostPermission, ex.ErrorCode);
        }
    }
}
