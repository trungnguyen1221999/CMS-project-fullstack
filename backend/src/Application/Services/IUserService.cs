using Application.DTOs;

namespace Application.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(Guid userId);
        Task<IEnumerable<UserDto>> GetAllAsync();
    }
}
