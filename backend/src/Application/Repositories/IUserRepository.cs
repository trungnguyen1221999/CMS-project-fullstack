using Application.Contracts.Users.Responses;
using Domain;
using Domain.Cores.Identity;

namespace Application.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<UserResponse?> GetByIdWithRolesAsync(Guid userId);
        Task<PageResult<UserListItemResponse>> GetAllWithRolesAsync(
            string? keyWord,
            int currentPage,
            int pageSize
        );
        Task<int> DeleteByIdsAsync(IEnumerable<Guid> ids);
        Task RemoveUserFromRoles(Guid userId, IEnumerable<string> roleNames);
    }
}
