using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Users.Requests;
using Application.Contracts.Users.Responses;
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

        public async Task<ReadResponse<PageResult<UserListItemResponse>>> GetAllAsync(
            string? keyWord,
            int currentPage,
            int pageSize
        )
        {
            var users = await _userRepository.GetAllWithRolesAsync(keyWord, currentPage, pageSize);
            return new ReadResponse<PageResult<UserListItemResponse>>
            {
                IsSuccess = true,
                Data = users,
            };
        }

        public async Task<ReadResponse<UserResponse>> GetByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            if (user == null)
            {
                return new ReadResponse<UserResponse>
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.UserNotFound,
                    ErrorMessage = ErrorMessages.User.UserNotFound,
                };
            }

            return new ReadResponse<UserResponse> { IsSuccess = true, Data = user };
        }

        public async Task<WriteResponse> CreateAsync(CreateUserRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new WriteResponse
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.UserAlreadyExists,
                    ErrorMessage = ErrorMessages.User.UserAlreadyExists,
                };
            }

            var newUser = _mapper.Map<CreateUserRequest, User>(request);
            newUser.UserName = newUser.Email;
            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return new WriteResponse
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.CreateFailed,
                    ErrorMessage = errors.Any()
                        ? string.Join(" | ", errors)
                        : ErrorMessages.User.CreateFailed,
                };
            }

            return new WriteResponse { IsSuccess = true };
        }

        public async Task<WriteResponse> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var existingUser = await _userManager.FindByIdAsync(id.ToString());
            if (existingUser == null)
            {
                return new WriteResponse
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
                return new WriteResponse
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.UpdateFailed,
                    ErrorMessage = errors.Any()
                        ? string.Join(" | ", errors)
                        : ErrorMessages.User.UpdateFailed,
                };
            }

            return new WriteResponse { IsSuccess = true };
        }

        public async Task<WriteResponse> DeleteAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new WriteResponse
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.InvalidIds,
                    ErrorMessage = ErrorMessages.User.InvalidIds,
                };
            }

            var affected = await _userRepository.DeleteByIdsAsync(ids);
            if (affected == 0)
            {
                return new WriteResponse
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.UsersNotFound,
                    ErrorMessage = ErrorMessages.User.UsersNotFound,
                };
            }

            return new WriteResponse { IsSuccess = true };
        }

        public async Task<WriteResponse> ChangeMyPasswordAsync(
            Guid id,
            ChangeMyPasswordRequest request
        )
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                var code = ErrorMessages.User.UserNotFound;
                return new WriteResponse
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
                return new WriteResponse
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

                return new WriteResponse
                {
                    IsSuccess = false,
                    ErrorCode = code,
                    ErrorMessage = errors.Any() ? string.Join(" | ", errors) : code,
                };
            }

            return new WriteResponse { IsSuccess = true };
        }
    }
}
