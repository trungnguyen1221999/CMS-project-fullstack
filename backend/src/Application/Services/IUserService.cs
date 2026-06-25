using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;

namespace Application.Services
{
    public interface IUserService
    {
        Task<ReadResponseDto<UserDto>> GetByIdAsync(Guid userId);
        Task<ReadResponseDto<IEnumerable<UserDto>>> GetAllAsync();
        Task<WriteResponseDto> CreateAsync(CreateUserRequestDto request);
    }
}
