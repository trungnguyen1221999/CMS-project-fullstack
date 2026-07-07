using Application.Contracts.Posts.Request;
using Application.Contracts.Posts.Response;
using AutoMapper;
using Domain.Cores.Content;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Test.Post.Tests
{
    public class PostRepository_GetAllPostsAsyncTest : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PostRepository _repository;

        // Fixed user IDs
        private readonly Guid _authorId = Guid.NewGuid();
        private readonly Guid _editorId = Guid.NewGuid();
        private readonly Guid _otherAuthorId = Guid.NewGuid();

        public PostRepository_GetAllPostsAsyncTest()
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
                // Author's own posts (all statuses)
                CreatePost(_authorId, PostStatus.Draft, "My Draft Post"),
                CreatePost(_authorId, PostStatus.WaitingForApproval, "My Pending Post"),
                CreatePost(_authorId, PostStatus.Published, "My Published Post"),
                CreatePost(_authorId, PostStatus.Rejected, "My Rejected Post"),
                // Other author's posts (all statuses)
                CreatePost(_otherAuthorId, PostStatus.Draft, "Other Draft 1"),
                CreatePost(_otherAuthorId, PostStatus.Draft, "Other Draft 2"),
                CreatePost(_otherAuthorId, PostStatus.Draft, "Other Draft 3"),
                CreatePost(_otherAuthorId, PostStatus.WaitingForApproval, "Other Pending"),
                CreatePost(_otherAuthorId, PostStatus.Published, "Other Published"),
                CreatePost(_otherAuthorId, PostStatus.Rejected, "Other Rejected"),
            };

            _context.Posts.AddRange(posts);
            _context.SaveChanges();
        }

        private static Domain.Cores.Content.Post CreatePost(
            Guid authorUserId,
            PostStatus status,
            string name
        )
        {
            return new Domain.Cores.Content.Post
            {
                Id = Guid.NewGuid(),
                AuthorUserId = authorUserId,
                Status = status,
                Name = name,
                Slug = Guid.NewGuid().ToString(),
                CategoryId = Guid.NewGuid(),
                CategorySlug = "test-cat",
                CategoryName = "Test Category",
            };
        }

        private static GetAllPostsRequest CreateRequest() =>
            new() { CurrentPage = 1, PageSize = 50 };

        [Fact]
        public async Task Author_NoApprovePermission_ReturnsOnlyOwnPosts()
        {
            // Act
            var result = await _repository.GetAllPostsAsync(
                CreateRequest(),
                _authorId,
                hasApprovePostPermission: false
            );

            // Assert: author sees only their own 4 posts
            Assert.Equal(4, result.TotalCount);
            Assert.All(
                result.Result,
                p =>
                    Assert.Equal(
                        _authorId,
                        _context.Posts.First(x => x.Name == p.Name).AuthorUserId
                    )
            );
        }

        [Fact]
        public async Task Editor_WithApprovePermission_SeesOwnPostsAndOthersNonDraft()
        {
            // Act
            var result = await _repository.GetAllPostsAsync(
                CreateRequest(),
                _editorId,
                hasApprovePostPermission: true
            );

            // Assert: editor sees others' non-draft posts (Pending + Published + Rejected = 3 from other author + 3 from author)
            // But NOT other's Draft posts (3 draft from otherAuthor excluded)
            // Editor has no own posts, so: author's (Pending + Published + Rejected = 3) + other's (Pending + Published + Rejected = 3) = 6
            // Plus author's Draft is NOT visible (editor is not the author)
            Assert.Equal(6, result.TotalCount);
            Assert.DoesNotContain(result.Result, p => p.Name.Contains("Other Draft"));
        }

        [Fact]
        public async Task Editor_WithApprovePermission_SeesOwnDraftPosts()
        {
            // Arrange: add a draft post for the editor
            _context.Posts.Add(CreatePost(_editorId, PostStatus.Draft, "Editor Draft"));
            _context.SaveChanges();

            // Act
            var result = await _repository.GetAllPostsAsync(
                CreateRequest(),
                _editorId,
                hasApprovePostPermission: true
            );

            // Assert: editor sees own draft + all non-draft posts from everyone
            Assert.Contains(result.Result, p => p.Name == "Editor Draft");
            // 6 non-draft from others + 1 own draft = 7
            Assert.Equal(7, result.TotalCount);
        }

        [Fact]
        public async Task Author_NoApprovePermission_DoesNotSeeOthersDrafts()
        {
            // Act
            var result = await _repository.GetAllPostsAsync(
                CreateRequest(),
                _authorId,
                hasApprovePostPermission: false
            );

            // Assert
            Assert.DoesNotContain(result.Result, p => p.Name.Contains("Other"));
        }

        [Fact]
        public async Task Author_WithApprovePermission_SeesOwnDraftAndOthersNonDraft()
        {
            // Act: author who also has approve permission
            var result = await _repository.GetAllPostsAsync(
                CreateRequest(),
                _authorId,
                hasApprovePostPermission: true
            );

            // Assert: own posts (4: Draft + Pending + Published + Rejected)
            //       + other's non-draft (Pending + Published + Rejected = 3)
            //       = 7 total
            Assert.Equal(7, result.TotalCount);
            Assert.Contains(result.Result, p => p.Name == "My Draft Post");
            Assert.DoesNotContain(result.Result, p => p.Name.Contains("Other Draft"));
        }

        [Fact]
        public async Task KeywordFilter_ReturnsMatchingPostsOnly()
        {
            // Arrange
            var request = new GetAllPostsRequest
            {
                Keyword = "Published",
                CurrentPage = 1,
                PageSize = 50,
            };

            // Act
            var result = await _repository.GetAllPostsAsync(
                request,
                _authorId,
                hasApprovePostPermission: false
            );

            // Assert: only author's own post matching keyword
            Assert.Equal(1, result.TotalCount);
            Assert.Equal("My Published Post", result.Result.First().Name);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
