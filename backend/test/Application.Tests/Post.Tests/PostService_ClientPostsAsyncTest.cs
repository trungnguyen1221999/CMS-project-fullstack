using System.Linq.Expressions;
using Application.Constants;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using AppPost = Domain.Cores.Content.Post;

namespace Application.Tests.Post.Tests
{
    public partial class PostServiceTest
    {
        [Fact]
        public async Task ClientGetAllPostsAsync_ReturnsSuccess()
        {
            // Arrange
            var request = new PostPagingRequest { CurrentPage = 1, PageSize = 10 };
            var fakePageResult = new PageResult<PostInListResponse>
            {
                CurrentPage = 1,
                PageSize = 10,
                TotalCount = 1,
                Result = [new PostInListResponse { Name = "Published Post" }],
            };

            _mockUnitOfWork
                .Setup(x => x.Posts.GetPublishedPostsAsync(request))
                .ReturnsAsync(fakePageResult);

            // Act
            var result = await _postService.ClientGetAllPostsAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Data!.TotalCount);
            _mockUnitOfWork.Verify(x => x.Posts.GetPublishedPostsAsync(request), Times.Once);
        }

        [Fact]
        public async Task ClientGetPostByIdAsync_PostNotFound_ReturnsFailure()
        {
            // Arrange
            var postId = Guid.NewGuid();

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost>().BuildMockQueryable());

            // Act
            var result = await _postService.ClientGetPostByIdAsync(postId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Post.PostNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task ClientGetPostByIdAsync_PostNotPublished_ReturnsFailure()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var post = CreateFakePost(postId, Guid.NewGuid(), PostStatus.Draft);

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            // Act
            var result = await _postService.ClientGetPostByIdAsync(postId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Post.PostNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task ClientGetPostByIdAsync_PublishedPost_ReturnsSuccess()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var post = CreateFakePost(postId, Guid.NewGuid(), PostStatus.Published);
            var postResponse = new PostResponse { AuthorUserId = post.AuthorUserId };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockMapper.Setup(x => x.Map<AppPost, PostResponse>(post)).Returns(postResponse);

            // Act
            var result = await _postService.ClientGetPostByIdAsync(postId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task ClientGetPostsByCategoryAsync_ReturnsSuccess()
        {
            // Arrange
            var categorySlug = "tech";
            var request = new PostPagingRequest { CurrentPage = 1, PageSize = 10 };
            var fakePageResult = new PageResult<PostInListResponse>
            {
                CurrentPage = 1,
                PageSize = 10,
                TotalCount = 2,
                Result =
                [
                    new PostInListResponse { Name = "Post 1" },
                    new PostInListResponse { Name = "Post 2" },
                ],
            };

            _mockUnitOfWork
                .Setup(x => x.Posts.GetPostsByCategoryAsync(categorySlug, request))
                .ReturnsAsync(fakePageResult);

            // Act
            var result = await _postService.ClientGetPostsByCategoryAsync(categorySlug, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data!.TotalCount);
            _mockUnitOfWork.Verify(
                x => x.Posts.GetPostsByCategoryAsync(categorySlug, request),
                Times.Once
            );
        }

        [Fact]
        public async Task ClientGetPostsByTagAsync_ReturnsSuccess()
        {
            // Arrange
            var tagSlug = "csharp";
            var request = new PostPagingRequest { CurrentPage = 1, PageSize = 10 };
            var fakePageResult = new PageResult<PostInListResponse>
            {
                CurrentPage = 1,
                PageSize = 10,
                TotalCount = 1,
                Result = [new PostInListResponse { Name = "C# Post" }],
            };

            _mockUnitOfWork
                .Setup(x => x.Posts.GetPostsByTagAsync(tagSlug, request))
                .ReturnsAsync(fakePageResult);

            // Act
            var result = await _postService.ClientGetPostsByTagAsync(tagSlug, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Data!.TotalCount);
            _mockUnitOfWork.Verify(x => x.Posts.GetPostsByTagAsync(tagSlug, request), Times.Once);
        }
    }
}
