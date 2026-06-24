using Application.DTOs;
using Application.Repositories;
using Application.Services;
using AutoMapper;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            UserManager<User> userManager
        )
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                userDtos.Add(_mapper.Map<User, UserDto>(user));
            }
            return userDtos;
        }

        public async Task<UserDto?> GetByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;
            var userDto = _mapper.Map<User, UserDto>(user);

            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles;
            return userDto;
        }
    }
}
