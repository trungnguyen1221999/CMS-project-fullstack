using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.DTOs.Response.Auth;
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
            return await _userRepository.GetAllWithRolesAsync();
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

        public async Task<ResponseDto<User>> CreateAsync(CreateUserRequestDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new ResponseDto<User>
                {
                    IsSuccess = false,
                    ErrorMessage = "User is already exist",
                };
            }

            var newUser = _mapper.Map<CreateUserRequestDto, User>(request);
            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);

                return new ResponseDto<User>
                {
                    IsSuccess = false,
                    ErrorMessage = string.Join(", ", errors),
                };
            }
            return new ResponseDto<User> { IsSuccess = true, Data = newUser };
        }
    }
}
