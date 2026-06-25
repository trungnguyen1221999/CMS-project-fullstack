using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Repositories;
using Application.Services;
using AutoMapper;
using Domain;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            UserManager<User> userManager,
            IMapper mapper
        )
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ReadResponseDto<PageResult<UserListItemDto>>> GetAllAsync(
            string? keyWord,
            int currentPage,
            int pageSize
        )
        {
            var users = await _userRepository.GetAllWithRolesAsync(keyWord, currentPage, pageSize);
            return new ReadResponseDto<PageResult<UserListItemDto>>
            {
                IsSuccess = true,
                Data = users,
            };
        }

        public async Task<ReadResponseDto<UserDto>> GetByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            if (user == null)
            {
                return new ReadResponseDto<UserDto>
                {
                    IsSuccess = false,
                    ErrorMessage = "User not found",
                };
            }
            return new ReadResponseDto<UserDto> { IsSuccess = true, Data = user };
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

        public async Task<WriteResponseDto> UpdateAsync(Guid id, UpdateUserRequestDto request)
        {
            var existingUser = await _userManager.FindByIdAsync(id.ToString());
            if (existingUser == null)
            {
                return new WriteResponseDto { IsSuccess = false, ErrorMessage = "User not found" };
            }

            _mapper.Map(request, existingUser);
            var result = await _userManager.UpdateAsync(existingUser);
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
