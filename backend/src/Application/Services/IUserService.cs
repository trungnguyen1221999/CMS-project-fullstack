using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;

namespace Application.Services
{
    public interface IUserService
    {
        Task<ReadResponseDto<UserDto>> GetByIdAsync(Guid userId);
        Task<ReadResponseDto<IEnumerable<UserListItemDto>>> GetAllAsync();
        Task<WriteResponseDto> CreateAsync(CreateUserRequestDto request);
    }
}
