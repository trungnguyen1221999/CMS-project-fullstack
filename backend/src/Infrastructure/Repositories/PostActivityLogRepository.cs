using Application.Repositories;
using Domain.Cores.Content;

namespace Infrastructure.Repositories
{
    public class PostActivityLogRepository
        : RepositoryBase<PostActivityLog, Guid>,
            IPostActivityLogRepository
    {
        public PostActivityLogRepository(ApplicationDbContext context)
            : base(context) { }
    }
}
