using Domain.Cores.Identity;

namespace Application.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task RemoveUserFromRoles(Guid userId, IEnumerable<string> roleNames);
    }
}
