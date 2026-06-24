using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Domain.Cores.Identity;

namespace Application.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(Guid userId);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<ResponseDto<User>> CreateAsync(CreateUserRequestDto request);
    }
}
