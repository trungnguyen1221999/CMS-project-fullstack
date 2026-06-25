using Application.DTOs;
using Domain.Cores.Identity;

namespace Application.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<UserDto?> GetByIdWithRolesAsync(Guid userId);
        Task<IEnumerable<UserListItemDto>> GetAllWithRolesAsync();
        Task RemoveUserFromRoles(Guid userId, IEnumerable<string> roleNames);
    }
}
