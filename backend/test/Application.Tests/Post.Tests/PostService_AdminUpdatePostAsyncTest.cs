using System.Linq.Expressions;
using Application.Constants;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using static Application.Exceptions.CustomException;
using AppPost = Domain.Cores.Content.Post;

namespace Application.Tests.Post.Tests
{
    public partial class PostServiceTest
    {
        [Fact]
        public async Task AdminUpdatePostAsync_PostNotFound_ReturnsFailure()
        {
            var request = CreateValidCreateUpdatePostRequest();
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost>().BuildMockQueryable());

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _adminPostService.UpdatePostAsync(request, postId, userId)
            );
            Assert.Equal(ErrorMessages.Post.PostNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminUpdatePostAsync_SlugAlreadyExists_ReturnsFailure()
        {
            var request = CreateValidCreateUpdatePostRequest();
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingPost = CreateFakePost(postId, userId);
            var otherPost = CreateFakePost(Guid.NewGuid(), userId);

            var callCount = 0;
            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        return new List<AppPost> { existingPost }.BuildMockQueryable();
                    return new List<AppPost> { otherPost }.BuildMockQueryable();
                });

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _adminPostService.UpdatePostAsync(request, postId, userId)
            );
            Assert.Equal(ErrorMessages.Post.SlugAlreadyExists, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminUpdatePostAsync_CategoryNotFound_ReturnsFailure()
        {
            var request = CreateValidCreateUpdatePostRequest();
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingPost = CreateFakePost(postId, userId);

            var callCount = 0;
            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        return new List<AppPost> { existingPost }.BuildMockQueryable();
                    return new List<AppPost>().BuildMockQueryable();
                });

            _mockMapper.Setup(x => x.Map(request, existingPost));

            _mockUnitOfWork
                .Setup(x => x.Categories.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory>().BuildMockQueryable());

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _adminPostService.UpdatePostAsync(request, postId, userId)
            );
            Assert.Equal(ErrorMessages.Category.CategoryNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task AdminUpdatePostAsync_Success_ReturnsSuccess()
        {
            var request = CreateValidCreateUpdatePostRequest();
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingPost = CreateFakePost(postId, userId);
            var category = new PostCategory
            {
                Id = request.CategoryId,
                Name = "Cat",
                Slug = "cat",
            };

            var callCount = 0;
            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        return new List<AppPost> { existingPost }.BuildMockQueryable();
                    return new List<AppPost>().BuildMockQueryable();
                });

            _mockMapper.Setup(x => x.Map(request, existingPost));

            _mockUnitOfWork
                .Setup(x => x.Categories.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory> { category }.BuildMockQueryable());

            _mockUnitOfWork.Setup(x => x.PostTags.ClearAllTagsFromPost(postId));

            _mockUnitOfWork
                .Setup(x => x.Tags.Find(It.IsAny<Expression<Func<Tag, bool>>>()))
                .Returns(new List<Tag>().BuildMockQueryable());

            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            await _adminPostService.UpdatePostAsync(request, postId, userId);

            _mockUnitOfWork.Verify(x => x.PostTags.ClearAllTagsFromPost(postId), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AdminUpdatePostAsync_SaveFails_ReturnsFailure()
        {
            var request = CreateValidCreateUpdatePostRequest();
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingPost = CreateFakePost(postId, userId);
            var category = new PostCategory
            {
                Id = request.CategoryId,
                Name = "Cat",
                Slug = "cat",
            };

            var callCount = 0;
            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1)
                        return new List<AppPost> { existingPost }.BuildMockQueryable();
                    return new List<AppPost>().BuildMockQueryable();
                });

            _mockMapper.Setup(x => x.Map(request, existingPost));

            _mockUnitOfWork
                .Setup(x => x.Categories.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                .Returns(new List<PostCategory> { category }.BuildMockQueryable());

            _mockUnitOfWork.Setup(x => x.PostTags.ClearAllTagsFromPost(It.IsAny<Guid>()));

            _mockUnitOfWork
                .Setup(x => x.Tags.Find(It.IsAny<Expression<Func<Tag, bool>>>()))
                .Returns(new List<Tag>().BuildMockQueryable());

            _mockUnitOfWork.Setup(x => x.PostTags.AddTagToPost(It.IsAny<Guid>(), It.IsAny<Guid>()));
            _mockUnitOfWork.Setup(x => x.Tags.Add(It.IsAny<Tag>()));
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(0);

            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => _adminPostService.UpdatePostAsync(request, postId, userId)
            );
            Assert.Equal(ErrorMessages.Post.UpdatePostFailed, ex.ErrorCode);
        }
    }
}
