using Application.Constants;
using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Repositories;
using AutoMapper;
using Domain;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
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
                    ErrorCode = ErrorMessages.User.UserNotFound,
                    ErrorMessage = ErrorMessages.User.UserNotFound,
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
                    ErrorCode = ErrorMessages.User.UserAlreadyExists,
                    ErrorMessage = ErrorMessages.User.UserAlreadyExists,
                };
            }

            var newUser = _mapper.Map<CreateUserRequestDto, User>(request);
            newUser.UserName = newUser.Email;
            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return new WriteResponseDto
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.CreateFailed,
                    ErrorMessage = errors.Any()
                        ? string.Join(" | ", errors)
                        : ErrorMessages.User.CreateFailed,
                };
            }

            return new WriteResponseDto { IsSuccess = true };
        }

        public async Task<WriteResponseDto> UpdateAsync(Guid id, UpdateUserRequestDto request)
        {
            var existingUser = await _userManager.FindByIdAsync(id.ToString());
            if (existingUser == null)
            {
                return new WriteResponseDto
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.UserNotFound,
                    ErrorMessage = ErrorMessages.User.UserNotFound,
                };
            }

            _mapper.Map(request, existingUser);
            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return new WriteResponseDto
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.UpdateFailed,
                    ErrorMessage = errors.Any()
                        ? string.Join(" | ", errors)
                        : ErrorMessages.User.UpdateFailed,
                };
            }

            return new WriteResponseDto { IsSuccess = true };
        }

        public async Task<WriteResponseDto> DeleteAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new WriteResponseDto
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.InvalidIds,
                    ErrorMessage = ErrorMessages.User.InvalidIds,
                };
            }

            var affected = await _userRepository.DeleteByIdsAsync(ids);
            if (affected == 0)
            {
                return new WriteResponseDto
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.UsersNotFound,
                    ErrorMessage = ErrorMessages.User.UsersNotFound,
                };
            }

            return new WriteResponseDto { IsSuccess = true };
        }

        public async Task<WriteResponseDto> ChangeMyPasswordAsync(
            Guid id,
            ChangeMyPasswordRequest request
        )
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                var code = ErrorMessages.User.UserNotFound;
                return new WriteResponseDto
                {
                    IsSuccess = false,
                    ErrorCode = code,
                    ErrorMessage = code,
                };
            }

            var sameAsCurrent = await _userManager.CheckPasswordAsync(user, request.NewPassword);
            if (sameAsCurrent)
            {
                var code = ErrorMessages.User.ChangePassword.NewPasswordSameAsCurrent;
                return new WriteResponseDto
                {
                    IsSuccess = false,
                    ErrorCode = code,
                    ErrorMessage = code,
                };
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description).ToList();
                var code = result.Errors.Any(x => x.Code == "PasswordMismatch")
                    ? ErrorMessages.Auth.CurrentPasswordIncorrect
                    : ErrorMessages.Auth.ChangePasswordFailed;

                return new WriteResponseDto
                {
                    IsSuccess = false,
                    ErrorCode = code,
                    ErrorMessage = errors.Any() ? string.Join(" | ", errors) : code,
                };
            }

            return new WriteResponseDto { IsSuccess = true };
        }
    }
}
