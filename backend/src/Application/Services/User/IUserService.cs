using Application.Contracts.Users.Requests;
using Application.Contracts.Users.Responses;
using Domain;

namespace Application.Services.User
{
    public interface IUserService
    {
        Task<UserResponse> GetByIdAsync(Guid userId);
        Task<PageResult<UserListItemResponse>> GetAllAsync(
            string? keyWord,
            int currentPage,
            int pageSize
        );
        Task CreateAsync(CreateUserRequest request);

        Task UpdateAsync(Guid id, UpdateUserRequest request);

        Task DeleteAsync(List<Guid> ids);

        Task ChangeMyPasswordAsync(Guid id, ChangeMyPasswordRequest request);

        Task SetPasswordAsync(Guid id, SetPasswordRequest request);

        Task ChangeEmailAsync(Guid id, ChangeEmailRequest request);
        Task AssignRolesToUserAsync(Guid id, string[] roles);
    }
}
