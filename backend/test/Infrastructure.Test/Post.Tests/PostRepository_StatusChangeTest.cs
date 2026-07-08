using Application.Contracts.Posts.Response;
using AutoMapper;
using Domain.Cores.Content;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AppUser = Domain.Cores.Identity.User;

namespace Infrastructure.Test.Post.Tests
{
    public class PostRepository_StatusChangeTest : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PostRepository _repository;

        private readonly Guid _authorId = Guid.NewGuid();
        private readonly Guid _adminId = Guid.NewGuid();

        public PostRepository_StatusChangeTest()
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
        }

        private Domain.Cores.Content.Post CreatePost(
            PostStatus status,
            string name = "Test Post",
            string slug = "test-post"
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
                CategorySlug = "tech",
                CategoryName = "Technology",
            };
        }

        private AppUser CreateUser(Guid? id = null)
        {
            return new AppUser
            {
                Id = id ?? _adminId,
                UserName = "admin",
                FirstName = "Admin",
                LastName = "User",
            };
        }

        // --- Approve ---

        [Fact]
        public async Task Approve_ChangesStatusToPublished()
        {
            // Arrange
            var post = CreatePost(PostStatus.WaitingForApproval);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var user = CreateUser();

            // Act
            var result = await _repository.Approve(post, user, "looks good");

            // Assert
            Assert.True(result);
            Assert.Equal(PostStatus.Published, post.Status);
        }

        [Fact]
        public async Task Approve_CreatesActivityLog()
        {
            // Arrange
            var post = CreatePost(PostStatus.WaitingForApproval);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var user = CreateUser();

            // Act
            await _repository.Approve(post, user, "approved note");
            await _context.SaveChangesAsync();

            // Assert
            var log = await _context.PostActivityLogs.FirstOrDefaultAsync(l => l.PostId == post.Id);
            Assert.NotNull(log);
            Assert.Equal(PostStatus.WaitingForApproval, log.FromStatus);
            Assert.Equal(PostStatus.Published, log.ToStatus);
            Assert.Equal(user.UserName, log.UserName);
            Assert.Equal(user.Id, log.UserId);
            Assert.Equal("approved note", log.Note);
        }

        [Fact]
        public async Task Approve_WithNullNote_CreatesActivityLogWithNullNote()
        {
            // Arrange
            var post = CreatePost(PostStatus.WaitingForApproval);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var user = CreateUser();

            // Act
            await _repository.Approve(post, user, null);
            await _context.SaveChangesAsync();

            // Assert
            var log = await _context.PostActivityLogs.FirstOrDefaultAsync(l => l.PostId == post.Id);
            Assert.NotNull(log);
            Assert.Null(log.Note);
        }

        // --- Reject ---

        [Fact]
        public async Task Reject_ChangesStatusToRejected()
        {
            // Arrange
            var post = CreatePost(PostStatus.WaitingForApproval);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var user = CreateUser();

            // Act
            var result = await _repository.Reject(post, user, "needs changes");

            // Assert
            Assert.True(result);
            Assert.Equal(PostStatus.Rejected, post.Status);
        }

        [Fact]
        public async Task Reject_CreatesActivityLog()
        {
            // Arrange
            var post = CreatePost(PostStatus.WaitingForApproval);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var user = CreateUser();

            // Act
            await _repository.Reject(post, user, "rejected note");
            await _context.SaveChangesAsync();

            // Assert
            var log = await _context.PostActivityLogs.FirstOrDefaultAsync(l => l.PostId == post.Id);
            Assert.NotNull(log);
            Assert.Equal(PostStatus.WaitingForApproval, log.FromStatus);
            Assert.Equal(PostStatus.Rejected, log.ToStatus);
            Assert.Equal("rejected note", log.Note);
        }

        // --- SubmitForApproval ---

        [Fact]
        public async Task SubmitForApproval_ChangesStatusToWaitingForApproval()
        {
            // Arrange
            var post = CreatePost(PostStatus.Draft);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var user = CreateUser(_authorId);

            // Act
            var result = await _repository.SubmitForApproval(post, user, "ready for review");

            // Assert
            Assert.True(result);
            Assert.Equal(PostStatus.WaitingForApproval, post.Status);
        }

        [Fact]
        public async Task SubmitForApproval_CreatesActivityLog()
        {
            // Arrange
            var post = CreatePost(PostStatus.Draft);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var user = CreateUser(_authorId);

            // Act
            await _repository.SubmitForApproval(post, user, "submit note");
            await _context.SaveChangesAsync();

            // Assert
            var log = await _context.PostActivityLogs.FirstOrDefaultAsync(l => l.PostId == post.Id);
            Assert.NotNull(log);
            Assert.Equal(PostStatus.Draft, log.FromStatus);
            Assert.Equal(PostStatus.WaitingForApproval, log.ToStatus);
            Assert.Equal(user.Id, log.UserId);
            Assert.Equal("submit note", log.Note);
        }

        [Fact]
        public async Task SubmitForApproval_FromRejected_ChangesStatusCorrectly()
        {
            // Arrange
            var post = CreatePost(PostStatus.Rejected);
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            var user = CreateUser(_authorId);

            // Act
            var result = await _repository.SubmitForApproval(post, user, "resubmit");
            await _context.SaveChangesAsync();

            // Assert
            Assert.True(result);
            Assert.Equal(PostStatus.WaitingForApproval, post.Status);

            var log = await _context.PostActivityLogs.FirstOrDefaultAsync(l => l.PostId == post.Id);
            Assert.NotNull(log);
            Assert.Equal(PostStatus.Rejected, log.FromStatus);
            Assert.Equal(PostStatus.WaitingForApproval, log.ToStatus);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
