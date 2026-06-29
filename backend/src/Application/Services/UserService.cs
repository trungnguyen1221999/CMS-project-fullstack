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
            return ReadResponse<PageResult<UserListItemResponse>>.Success(users);
        }

        public async Task<ReadResponse<UserResponse>> GetByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            if (user == null)
                return ReadResponse<UserResponse>.Failure(ErrorMessages.User.UserNotFound);

            return ReadResponse<UserResponse>.Success(user);
        }

        public async Task<WriteResponse> CreateAsync(CreateUserRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return WriteResponse.Failure(ErrorMessages.User.UserAlreadyExists);

            var newUser = _mapper.Map<CreateUserRequest, User>(request);
            newUser.UserName = newUser.Email;
            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
                return WriteResponse.Failure(
                    ErrorMessages.User.CreateFailed,
                    FormatErrors(result)
                );

            return WriteResponse.Success();
        }

        public async Task<WriteResponse> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var existingUser = await _userManager.FindByIdAsync(id.ToString());
            if (existingUser == null)
                return WriteResponse.Failure(ErrorMessages.User.UserNotFound);

            _mapper.Map(request, existingUser);
            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
                return WriteResponse.Failure(
                    ErrorMessages.User.UpdateFailed,
                    FormatErrors(result)
                );

            return WriteResponse.Success();
        }

        public async Task<WriteResponse> DeleteAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return WriteResponse.Failure(ErrorMessages.User.InvalidIds);

            var affected = await _userRepository.DeleteByIdsAsync(ids);
            if (affected == 0)
                return WriteResponse.Failure(ErrorMessages.User.UsersNotFound);

            return WriteResponse.Success();
        }

        public async Task<WriteResponse> ChangeMyPasswordAsync(
            Guid id,
            ChangeMyPasswordRequest request
        )
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return WriteResponse.Failure(ErrorMessages.User.UserNotFound);

            var sameAsCurrent = await _userManager.CheckPasswordAsync(user, request.NewPassword);
            if (sameAsCurrent)
                return WriteResponse.Failure(
                    ErrorMessages.User.ChangePassword.NewPasswordSameAsCurrent
                );

            var result = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword
            );

            if (!result.Succeeded)
            {
                var code = result.Errors.Any(x => x.Code == "PasswordMismatch")
                    ? ErrorMessages.Auth.CurrentPasswordIncorrect
                    : ErrorMessages.Auth.ChangePasswordFailed;
                return WriteResponse.Failure(code, FormatErrors(result));
            }

            return WriteResponse.Success();
        }

        public async Task<WriteResponse> SetPasswordAsync(Guid id, SetPasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return WriteResponse.Failure(ErrorMessages.User.UserNotFound);

            var sameAsCurrent = await _userManager.CheckPasswordAsync(user, request.NewPassword);
            if (sameAsCurrent)
                return WriteResponse.Failure(
                    ErrorMessages.User.ChangePassword.NewPasswordSameAsCurrent
                );

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!result.Succeeded)
                return WriteResponse.Failure(
                    ErrorMessages.User.SetPasswordFailed,
                    FormatErrors(result)
                );

            return WriteResponse.Success();
        }

        private static string FormatErrors(IdentityResult result) =>
            string.Join(" | ", result.Errors.Select(e => e.Description));
    }
}
