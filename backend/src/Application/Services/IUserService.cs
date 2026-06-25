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
    }
}
