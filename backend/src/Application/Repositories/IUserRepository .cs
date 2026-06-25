using Application.DTOs;
using Domain.Cores.Identity;

namespace Application.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<IEnumerable<UserDto>> GetAllWithRolesAsync();
        Task RemoveUserFromRoles(Guid userId, IEnumerable<string> roleNames);
    }
}
