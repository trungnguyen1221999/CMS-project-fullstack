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
        public async Task AdminApprovePostAsync_PostNotFound_ReturnsFailure()
        {
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost>().BuildMockQueryable());

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _adminPostService.ApprovePostAsync(postId, userId, "approved")
            );
            Assert.Equal(ErrorMessages.Post.PostNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminApprovePostAsync_UserNotFound_ReturnsFailure()
        {
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var post = CreateFakePost(postId, Guid.NewGuid(), PostStatus.WaitingForApproval);

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _adminPostService.ApprovePostAsync(postId, userId, "approved")
            );
            Assert.Equal(ErrorMessages.User.UserNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminApprovePostAsync_Success_ReturnsSuccess()
        {
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var post = CreateFakePost(postId, Guid.NewGuid(), PostStatus.WaitingForApproval);
            var user = new AppUser { Id = userId, UserName = "admin" };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            _mockUnitOfWork.Setup(x => x.Posts.Approve(post, user, "approved")).ReturnsAsync(true);

            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            await _adminPostService.ApprovePostAsync(postId, userId, "approved");

            _mockUnitOfWork.Verify(x => x.Posts.Approve(post, user, "approved"), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AdminApprovePostAsync_SaveFails_ReturnsFailure()
        {
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var post = CreateFakePost(postId, Guid.NewGuid(), PostStatus.WaitingForApproval);
            var user = new AppUser { Id = userId, UserName = "admin" };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            _mockUnitOfWork.Setup(x => x.Posts.Approve(post, user, null)).ReturnsAsync(true);

            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(0);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _adminPostService.ApprovePostAsync(postId, userId, null)
            );
            Assert.Equal(ErrorMessages.Post.ApproveFailed, ex.ErrorCode);
        }
    }
}
