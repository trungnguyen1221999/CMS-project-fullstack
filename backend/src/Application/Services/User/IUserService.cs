using Application.Contracts.Common;
using Application.Contracts.Users.Requests;
using Application.Contracts.Users.Responses;
using Domain;

namespace Application.Services.User
{
    public interface IUserService
    {
        Task<ReadResponse<UserResponse>> GetByIdAsync(Guid userId);
        Task<ReadResponse<PageResult<UserListItemResponse>>> GetAllAsync(
            string? keyWord,
            int currentPage,
            int pageSize
        );
        Task<WriteResponse> CreateAsync(CreateUserRequest request);

        Task<WriteResponse> UpdateAsync(Guid id, UpdateUserRequest request);

        Task<WriteResponse> DeleteAsync(List<Guid> ids);

        Task<WriteResponse> ChangeMyPasswordAsync(Guid id, ChangeMyPasswordRequest request);

        Task<WriteResponse> SetPasswordAsync(Guid id, SetPasswordRequest request);

        Task<WriteResponse> ChangeEmailAsync(Guid id, ChangeEmailRequest request);
        Task<WriteResponse> AssignRolesToUserAsync(Guid id, string[] roles);
    }
}
