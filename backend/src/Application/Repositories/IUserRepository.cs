using Application.DTOs;
using Domain;
using Domain.Cores.Identity;

namespace Application.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<UserDto?> GetByIdWithRolesAsync(Guid userId);
        Task<PageResult<UserListItemDto>> GetAllWithRolesAsync(
            string? keyWord,
            int currentPage,
            int pageSize
        );
        Task RemoveUserFromRoles(Guid userId, IEnumerable<string> roleNames);
    }
}
