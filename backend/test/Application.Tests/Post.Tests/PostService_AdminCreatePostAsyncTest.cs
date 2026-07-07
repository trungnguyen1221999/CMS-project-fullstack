using System.Linq.Expressions;
using Application.Constants;
using Application.Contracts.Posts.Request;
using Domain.Cores.Content;
using Moq;
using Test.Shared.Helpers;
using AppPost = Domain.Cores.Content.Post;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Tests.Post.Tests
{
    public partial class PostServiceTest
    {
        private static CreateUpdatePostRequest CreateValidCreateUpdatePostRequest() =>
            new()
            {
                Name = "New Post",
                Slug = "new-post",
                Description = "Description",
                CategoryId = Guid.NewGuid(),
                Content = "Content",
                Tags = ["tag1", "tag2"],
            };

        [Fact]
        public async Task AdminCreatePostAsync_SlugAlreadyExists_ReturnsFailure()
        {
            // Arrange
            var request = CreateValidCreateUpdatePostRequest();
            var userId = Guid.NewGuid();
            var existingPost = CreateFakePost(Guid.NewGuid(), userId);

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost> { existingPost }.BuildMockQueryable());

                            // Act
                            var result = await _adminPostService.CreatePostAsync(request, userId);

                            // Assert
                            Assert.False(result.IsSuccess);
                            Assert.Equal(ErrorMessages.Post.SlugAlreadyExists, result.ErrorCode);
        }

        [Fact]
        public async Task AdminCreatePostAsync_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var request = CreateValidCreateUpdatePostRequest();
            var userId = Guid.NewGuid();

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost>().BuildMockQueryable());

                            _mockUserManager
                                .Setup(x => x.FindByIdAsync(userId.ToString()))
                                .ReturnsAsync((AppUser?)null);

            // Act
            var result = await _adminPostService.CreatePostAsync(request, userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.User.UserNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task AdminCreatePostAsync_CategoryNotFound_ReturnsFailure()
        {
            // Arrange
            var request = CreateValidCreateUpdatePostRequest();
            var userId = Guid.NewGuid();
            var user = new AppUser
            {
                Id = userId,
                FirstName = "Test",
                LastName = "User",
                UserName = "testuser",
            };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost>().BuildMockQueryable());

                            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

                            _mockMapper
                                .Setup(x => x.Map<CreateUpdatePostRequest, AppPost>(request))
                                .Returns(new AppPost { Id = Guid.NewGuid() });

                            _mockUnitOfWork
                                .Setup(x => x.Categories.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                                .Returns(new List<PostCategory>().BuildMockQueryable());

            // Act
            var result = await _adminPostService.CreatePostAsync(request, userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Category.CategoryNotFound, result.ErrorCode);
        }

        [Fact]
        public async Task AdminCreatePostAsync_Success_ReturnsSuccess()
        {
            // Arrange
            var request = CreateValidCreateUpdatePostRequest();
            var userId = Guid.NewGuid();
            var categoryId = request.CategoryId;
            var user = new AppUser
            {
                Id = userId,
                FirstName = "Test",
                LastName = "User",
                UserName = "testuser",
            };
            var category = new PostCategory
            {
                Id = categoryId,
                Name = "Cat",
                Slug = "cat",
            };
            var mappedPost = new AppPost { Id = Guid.NewGuid() };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost>().BuildMockQueryable());

                            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

                            _mockMapper
                                .Setup(x => x.Map<CreateUpdatePostRequest, AppPost>(request))
                                .Returns(mappedPost);

                            _mockUnitOfWork
                                .Setup(x => x.Categories.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                                .Returns(new List<PostCategory> { category }.BuildMockQueryable());

                            _mockUnitOfWork.Setup(x => x.Posts.Add(It.IsAny<AppPost>()));

                            _mockUnitOfWork
                                .Setup(x => x.Tags.Find(It.IsAny<Expression<Func<Tag, bool>>>()))
                                .Returns(new List<Tag>().BuildMockQueryable());

            _mockUnitOfWork.Setup(x => x.Tags.Add(It.IsAny<Tag>()));
            _mockUnitOfWork.Setup(x => x.PostTags.AddTagToPost(It.IsAny<Guid>(), It.IsAny<Guid>()));
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _adminPostService.CreatePostAsync(request, userId);

            // Assert
            Assert.True(result.IsSuccess);
            _mockUnitOfWork.Verify(x => x.Posts.Add(It.IsAny<AppPost>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AdminCreatePostAsync_SaveFails_ReturnsFailure()
        {
            // Arrange
            var request = CreateValidCreateUpdatePostRequest();
            var userId = Guid.NewGuid();
            var user = new AppUser
            {
                Id = userId,
                FirstName = "Test",
                LastName = "User",
                UserName = "testuser",
            };
            var category = new PostCategory
            {
                Id = request.CategoryId,
                Name = "Cat",
                Slug = "cat",
            };

            _mockUnitOfWork
                .Setup(x => x.Posts.Find(It.IsAny<Expression<Func<AppPost, bool>>>()))
                .Returns(new List<AppPost>().BuildMockQueryable());

                            _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

                            _mockMapper
                                .Setup(x => x.Map<CreateUpdatePostRequest, AppPost>(request))
                                .Returns(new AppPost { Id = Guid.NewGuid() });

                            _mockUnitOfWork
                                .Setup(x => x.Categories.Find(It.IsAny<Expression<Func<PostCategory, bool>>>()))
                                .Returns(new List<PostCategory> { category }.BuildMockQueryable());

                            _mockUnitOfWork
                                .Setup(x => x.Tags.Find(It.IsAny<Expression<Func<Tag, bool>>>()))
                                .Returns(new List<Tag>().BuildMockQueryable());

            _mockUnitOfWork.Setup(x => x.Tags.Add(It.IsAny<Tag>()));
            _mockUnitOfWork.Setup(x => x.PostTags.AddTagToPost(It.IsAny<Guid>(), It.IsAny<Guid>()));
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(0);

            // Act
            var result = await _adminPostService.CreatePostAsync(request, userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorMessages.Post.CreatePostFailed, result.ErrorCode);
        }
    }
}
