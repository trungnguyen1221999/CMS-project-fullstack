using Application.Repositories;
using Domain.Cores.Content;
using Domain.Cores.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PostActivityLogRepository
        : RepositoryBase<PostActivityLog, Guid>,
            IPostActivityLogRepository
    {
        public PostActivityLogRepository(ApplicationDbContext context)
            : base(context) { }

        public async Task<List<PostActivityLog>> GetActivityLogsAsync(Post post, User user)
        {
            var logs = await _context
                .PostActivityLogs.Where(l => l.PostId == post.Id && l.UserId == user.Id)
                .ToListAsync();
            return logs;
        }

        public async Task<string> GetRejectReasonAsync(Post post, User user)
        {
            var log = await _context
                .PostActivityLogs.Where(l =>
                    l.PostId == post.Id && l.UserId == user.Id && post.Status == PostStatus.Rejected
                )
                .FirstOrDefaultAsync();

            return log?.Note ?? string.Empty;
        }
    }
}
