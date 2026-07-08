using System.Linq.Expressions;
using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using Domain;
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
        public async Task ClientGetAllPostsAsync_ReturnsSuccess()
        {
            var request = new PagingRequest { CurrentPage = 1, PageSize = 10 };
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

            var result = await _clientPostService.GetAllPostsAsync(request);

            Assert.Equal(1, result.TotalCount);
            _mockUnitOfWork.Verify(x => x.Posts.GetPublishedPostsAsync(request), Times.Once);
        }

        [Fact]
        public async Task ClientGetPostByIdAsync_PostNotFound_ReturnsFailure()
        {
            var postId = Guid.NewGuid();

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost>().BuildMockQueryable());

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _clientPostService.GetPostByIdAsync(postId)
            );
            Assert.Equal(ErrorMessages.Post.PostNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task ClientGetPostByIdAsync_PostNotPublished_ReturnsFailure()
        {
            var postId = Guid.NewGuid();
            var post = CreateFakePost(postId, Guid.NewGuid(), PostStatus.Draft);

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _clientPostService.GetPostByIdAsync(postId)
            );
            Assert.Equal(ErrorMessages.Post.PostNotFound, ex.ErrorCode);
        }

        [Fact]
        public async Task ClientGetPostByIdAsync_PublishedPost_ReturnsSuccess()
        {
            var postId = Guid.NewGuid();
            var post = CreateFakePost(postId, Guid.NewGuid(), PostStatus.Published);
            var postResponse = new PostResponse { AuthorUserId = post.AuthorUserId };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { post }.BuildMockQueryable());

            _mockMapper.Setup(x => x.Map<AppPost, PostResponse>(post)).Returns(postResponse);

            var result = await _clientPostService.GetPostByIdAsync(postId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ClientGetPostsByCategoryAsync_ReturnsSuccess()
        {
            var categorySlug = "tech";
            var request = new PagingRequest { CurrentPage = 1, PageSize = 10 };
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

            var result = await _clientPostService.GetPostsByCategoryAsync(categorySlug, request);

            Assert.Equal(2, result.TotalCount);
            _mockUnitOfWork.Verify(
                x => x.Posts.GetPostsByCategoryAsync(categorySlug, request),
                Times.Once
            );
        }

        [Fact]
        public async Task ClientGetPostsByTagAsync_ReturnsSuccess()
        {
            var tagSlug = "csharp";
            var request = new PagingRequest { CurrentPage = 1, PageSize = 10 };
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

            var result = await _clientPostService.GetPostsByTagAsync(tagSlug, request);

            Assert.Equal(1, result.TotalCount);
            _mockUnitOfWork.Verify(x => x.Posts.GetPostsByTagAsync(tagSlug, request), Times.Once);
        }
    }
}
