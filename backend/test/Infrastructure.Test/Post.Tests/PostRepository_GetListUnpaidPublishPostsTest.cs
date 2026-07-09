using Application.Contracts.Posts.Response;
using AutoMapper;
using Domain.Cores.Content;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Test.Post.Tests
{
    public class PostRepository_GetListUnpaidPublishPostsTest : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PostRepository _repository;

        private readonly Guid _authorA = Guid.NewGuid();
        private readonly Guid _authorB = Guid.NewGuid();

        public PostRepository_GetListUnpaidPublishPostsTest()
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
                // AuthorA: 2 unpaid published, 1 paid published, 1 draft
                CreatePost(_authorA, PostStatus.Published, isPaid: false),
                CreatePost(_authorA, PostStatus.Published, isPaid: false),
                CreatePost(_authorA, PostStatus.Published, isPaid: true),
                CreatePost(_authorA, PostStatus.Draft, isPaid: false),
                // AuthorB: 1 unpaid published
                CreatePost(_authorB, PostStatus.Published, isPaid: false),
                // AuthorA: rejected, waiting
                CreatePost(_authorA, PostStatus.Rejected, isPaid: false),
                CreatePost(_authorA, PostStatus.WaitingForApproval, isPaid: false),
            };

            _context.Posts.AddRange(posts);
            _context.SaveChanges();
        }

        private static Domain.Cores.Content.Post CreatePost(
            Guid authorId,
            PostStatus status,
            bool isPaid
        ) =>
            new()
            {
                Id = Guid.NewGuid(),
                AuthorUserId = authorId,
                AuthorUserName = "user",
                Status = status,
                IsPaid = isPaid,
                Name = $"Post-{Guid.NewGuid():N}",
                Slug = $"post-{Guid.NewGuid():N}",
                CategoryId = Guid.NewGuid(),
                CategorySlug = "tech",
                CategoryName = "Tech",
                CreatedAt = DateTime.UtcNow,
            };

        [Fact]
        public async Task ReturnsOnlyUnpaidPublishedPostsForUser()
        {
            var result = await _repository.GetListUnpaidPublishPosts(_authorA);

            Assert.Equal(2, result.Count);
            Assert.All(result, p =>
            {
                Assert.Equal(_authorA, p.AuthorUserId);
                Assert.Equal(PostStatus.Published, p.Status);
                Assert.False(p.IsPaid);
            });
        }

        [Fact]
        public async Task DoesNotReturnOtherUserPosts()
        {
            var result = await _repository.GetListUnpaidPublishPosts(_authorA);

            Assert.DoesNotContain(result, p => p.AuthorUserId == _authorB);
        }

        [Fact]
        public async Task DoesNotReturnPaidPosts()
        {
            var result = await _repository.GetListUnpaidPublishPosts(_authorA);

            Assert.DoesNotContain(result, p => p.IsPaid);
        }

        [Fact]
        public async Task DoesNotReturnNonPublishedPosts()
        {
            var result = await _repository.GetListUnpaidPublishPosts(_authorA);

            Assert.DoesNotContain(result, p => p.Status != PostStatus.Published);
        }

        [Fact]
        public async Task UserWithNoUnpaidPosts_ReturnsEmpty()
        {
            var unknownUserId = Guid.NewGuid();

            var result = await _repository.GetListUnpaidPublishPosts(unknownUserId);

            Assert.Empty(result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
