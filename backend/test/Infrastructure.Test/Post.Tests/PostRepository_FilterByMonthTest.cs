using Application.Contracts.Posts.Response;
using Application.Contracts.Royaltys.Request;
using AutoMapper;
using Domain.Cores.Content;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Test.Post.Tests
{
    public class PostRepository_FilterByMonthTest : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PostRepository _repository;

        private readonly Guid _authorA = Guid.NewGuid();
        private readonly Guid _authorB = Guid.NewGuid();

        public PostRepository_FilterByMonthTest()
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
                CreatePost(_authorA, "trung", PostStatus.Published, true, new DateTime(2024, 11, 10)),
                CreatePost(_authorA, "trung", PostStatus.Draft, false, new DateTime(2024, 12, 5)),
                CreatePost(_authorA, "trung", PostStatus.Published, false, new DateTime(2025, 1, 15)),
                CreatePost(_authorB, "minh", PostStatus.Published, true, new DateTime(2025, 1, 20)),
                CreatePost(_authorA, "trung", PostStatus.Rejected, false, new DateTime(2025, 2, 10)),
                CreatePost(_authorB, "minh", PostStatus.WaitingForApproval, false, new DateTime(2025, 3, 1)),
                CreatePost(_authorA, "trung", PostStatus.Published, true, new DateTime(2025, 4, 5)),
            };

            _context.Posts.AddRange(posts);
            _context.SaveChanges();
        }

        private Domain.Cores.Content.Post CreatePost(
            Guid authorId,
            string authorUserName,
            PostStatus status,
            bool isPaid,
            DateTime createdAt
        )
        {
            return new Domain.Cores.Content.Post
            {
                Id = Guid.NewGuid(),
                AuthorUserId = authorId,
                AuthorUserName = authorUserName,
                Status = status,
                IsPaid = isPaid,
                Name = $"Post-{Guid.NewGuid():N}",
                Slug = $"post-{Guid.NewGuid():N}",
                CategoryId = Guid.NewGuid(),
                CategorySlug = "tech",
                CategoryName = "Tech",
                CreatedAt = createdAt,
            };
        }

        [Fact]
        public void FilterByMonth_ReturnsOnlyPostsInRange()
        {
            var request = new RoyaltyReportByUserAndMonthRequest
            {
                FromMonth = 1,
                FromYear = 2025,
                ToMonth = 3,
                ToYear = 2025,
            };

            var result = _repository.FilterByMonth(request).ToList();

            // Jan, Feb, Mar 2025: 4 posts (excludes Nov 2024, Dec 2024, Apr 2025)
            Assert.Equal(4, result.Count);
            Assert.All(result, p =>
            {
                Assert.True(p.CreatedAt >= new DateTime(2025, 1, 1));
                Assert.True(p.CreatedAt < new DateTime(2025, 4, 1));
            });
        }

        [Fact]
        public void FilterByMonth_CrossYear_ReturnsCorrectRange()
        {
            var request = new RoyaltyReportByUserAndMonthRequest
            {
                FromMonth = 11,
                FromYear = 2024,
                ToMonth = 2,
                ToYear = 2025,
            };

            var result = _repository.FilterByMonth(request).ToList();

            // Nov 2024, Dec 2024, Jan 2025, Feb 2025 → 5 posts
            Assert.Equal(5, result.Count);
            Assert.All(result, p =>
            {
                Assert.True(p.CreatedAt >= new DateTime(2024, 11, 1));
                Assert.True(p.CreatedAt < new DateTime(2025, 3, 1));
            });
        }

        [Fact]
        public void FilterByMonth_SingleMonth_ReturnsCorrect()
        {
            var request = new RoyaltyReportByUserAndMonthRequest
            {
                FromMonth = 1,
                FromYear = 2025,
                ToMonth = 1,
                ToYear = 2025,
            };

            var result = _repository.FilterByMonth(request).ToList();

            // Jan 2025 only: 2 posts
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(1, p.CreatedAt.Month));
        }

        [Fact]
        public void FilterByMonth_NoPostsInRange_ReturnsEmpty()
        {
            var request = new RoyaltyReportByUserAndMonthRequest
            {
                FromMonth = 6,
                FromYear = 2025,
                ToMonth = 8,
                ToYear = 2025,
            };

            var result = _repository.FilterByMonth(request).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void FilterByMonth_AllPosts_ReturnsAll()
        {
            var request = new RoyaltyReportByUserAndMonthRequest
            {
                FromMonth = 1,
                FromYear = 2024,
                ToMonth = 12,
                ToYear = 2025,
            };

            var result = _repository.FilterByMonth(request).ToList();

            Assert.Equal(7, result.Count);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
