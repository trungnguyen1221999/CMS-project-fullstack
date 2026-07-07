using Domain.Cores.Content;
using Domain.Cores.Identity;

namespace Application.Repositories
{
    public interface IPostActivityLogRepository : IRepository<PostActivityLog, Guid>
    {
        Task<string> GetRejectReasonAsync(Post post, User user);
    }
}
