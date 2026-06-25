using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
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

        public async Task<ReadResponseDto<IEnumerable<UserDto>>> GetAllAsync()
        {
            var users = await _userRepository.GetAllWithRolesAsync();
            return new ReadResponseDto<IEnumerable<UserDto>> { IsSuccess = true, Data = users };
        }

        public async Task<ReadResponseDto<UserDto>> GetByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ReadResponseDto<UserDto>
                {
                    IsSuccess = false,
                    ErrorMessage = "User not found",
                };
            }

            var userDto = _mapper.Map<User, UserDto>(user);

            var roles = await _userManager.GetRolesAsync(user);
            userDto.Roles = roles;
            return new ReadResponseDto<UserDto> { IsSuccess = true, Data = userDto };
        }

        public async Task<WriteResponseDto> CreateAsync(CreateUserRequestDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new WriteResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "User is already exist",
                };
            }

            var newUser = _mapper.Map<CreateUserRequestDto, User>(request);
            newUser.UserName = newUser.Email;
            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);

                return new WriteResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = string.Join(", ", errors),
                };
            }
            return new WriteResponseDto { IsSuccess = true };
        }
    }
}
