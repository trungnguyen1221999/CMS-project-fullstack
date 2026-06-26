using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Domain;

namespace Application.Services
{
    public interface IUserService
    {
        Task<ReadResponseDto<UserDto>> GetByIdAsync(Guid userId);
        Task<ReadResponseDto<PageResult<UserListItemDto>>> GetAllAsync(
            string? keyWord,
            int currentPage,
            int pageSize
        );
        Task<WriteResponseDto> CreateAsync(CreateUserRequestDto request);

        Task<WriteResponseDto> UpdateAsync(Guid id, UpdateUserRequestDto request);

        Task<WriteResponseDto> DeleteAsync(List<Guid> ids);

        Task<WriteResponseDto> ChangeMyPasswordAsync(Guid id, ChangeMyPasswordRequest request);
    }
}
