using Application.Contracts.Common;
using Application.Contracts.Posts.Response;
using AutoMapper;
using Domain.Cores.Content;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Test.Post.Tests
{
    public class PostRepository_ClientQueriesTest : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PostRepository _repository;

        private readonly Guid _authorId = Guid.NewGuid();

        public PostRepository_ClientQueriesTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg =>
                cfg.AddMaps(typeof(PostInListResponse.AutoMapperProfile).Assembly)
            );
            var mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();

            _repository = new PostRepository(_context, mapper);

            SeedData();
        }

        private void SeedData()
        {
            var posts = new List<Domain.Cores.Content.Post>
            {
                CreatePost(PostStatus.Draft, "Draft Post", "draft-post", "tech", null),
                CreatePost(
                    PostStatus.WaitingForApproval,
                    "Pending Post",
                    "pending-post",
                    "tech",
                    null
                ),
                CreatePost(
                    PostStatus.Published,
                    "Published Tech Post",
                    "published-tech",
                    "tech",
                    "csharp,dotnet"
                ),
                CreatePost(
                    PostStatus.Published,
                    "Published Life Post",
                    "published-life",
                    "life",
                    "travel"
                ),
                CreatePost(
                    PostStatus.Published,
                    "Another Tech Post",
                    "another-tech",
                    "tech",
                    "csharp"
                ),
                CreatePost(PostStatus.Rejected, "Rejected Post", "rejected-post", "tech", null),
            };

            _context.Posts.AddRange(posts);
            _context.SaveChanges();
        }

        private Domain.Cores.Content.Post CreatePost(
            PostStatus status,
            string name,
            string slug,
            string categorySlug,
            string? tags
        )
        {
            return new Domain.Cores.Content.Post
            {
                Id = Guid.NewGuid(),
                AuthorUserId = _authorId,
                Status = status,
                Name = name,
                Slug = slug,
                CategoryId = Guid.NewGuid(),
                CategorySlug = categorySlug,
                CategoryName = categorySlug == "tech" ? "Technology" : "Life",
                Tags = tags,
            };
        }

        // --- GetPublishedPostsAsync ---

        [Fact]
        public async Task GetPublishedPostsAsync_ReturnsOnlyPublishedPosts()
        {
            // Act
            var result = await _repository.GetPublishedPostsAsync(
                new PagingRequest { CurrentPage = 1, PageSize = 50 }
            );

            // Assert: 3 published posts
            Assert.Equal(3, result.TotalCount);
            Assert.All(result.Result, p => Assert.Equal(PostStatus.Published, p.Status));
        }

        [Fact]
        public async Task GetPublishedPostsAsync_PagingWorks()
        {
            // Act: page 1, size 2
            var result = await _repository.GetPublishedPostsAsync(
                new PagingRequest { CurrentPage = 1, PageSize = 2 }
            );

            // Assert
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.Result.Count);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(2, result.PageSize);
        }

        // --- GetPostsByCategoryAsync ---

        [Fact]
        public async Task GetPostsByCategoryAsync_ReturnsOnlyMatchingPublishedPosts()
        {
            // Act
            var result = await _repository.GetPostsByCategoryAsync(
                "tech",
                new PagingRequest { CurrentPage = 1, PageSize = 50 }
            );

            // Assert: 2 published tech posts
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Result, p => Assert.Equal("tech", p.CategorySlug));
        }

        [Fact]
        public async Task GetPostsByCategoryAsync_NoMatchingCategory_ReturnsEmpty()
        {
            // Act
            var result = await _repository.GetPostsByCategoryAsync(
                "nonexistent",
                new PagingRequest { CurrentPage = 1, PageSize = 50 }
            );

            // Assert
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Result);
        }

        [Fact]
        public async Task GetPostsByCategoryAsync_ExcludesNonPublished()
        {
            // Act: "tech" has Draft, Pending, Rejected posts too, but only Published should return
            var result = await _repository.GetPostsByCategoryAsync(
                "tech",
                new PagingRequest { CurrentPage = 1, PageSize = 50 }
            );

            // Assert
            Assert.DoesNotContain(result.Result, p => p.Name == "Draft Post");
            Assert.DoesNotContain(result.Result, p => p.Name == "Pending Post");
            Assert.DoesNotContain(result.Result, p => p.Name == "Rejected Post");
        }

        // --- GetPostsByTagAsync ---

        [Fact]
        public async Task GetPostsByTagAsync_ReturnsOnlyMatchingPublishedPosts()
        {
            // Act
            var result = await _repository.GetPostsByTagAsync(
                "csharp",
                new PagingRequest { CurrentPage = 1, PageSize = 50 }
            );

            // Assert: 2 published posts
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetPostsByTagAsync_NoMatchingTag_ReturnsEmpty()
        {
            // Act
            var result = await _repository.GetPostsByTagAsync(
                "python",
                new PagingRequest { CurrentPage = 1, PageSize = 50 }
            );

            // Assert
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Result);
        }

        [Fact]
        public async Task GetPostsByTagAsync_UniqueTag_ReturnsSinglePost()
        {
            // Act
            var result = await _repository.GetPostsByTagAsync(
                "travel",
                new PagingRequest { CurrentPage = 1, PageSize = 50 }
            );

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Equal("Published Life Post", result.Result.First().Name);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
