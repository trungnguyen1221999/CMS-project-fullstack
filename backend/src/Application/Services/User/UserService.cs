using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Users.Requests;
using Application.Contracts.Users.Responses;
using Application.UnitOfWork;
using AutoMapper;
using Domain;
using static Application.Exceptions.CustomException;
using Microsoft.AspNetCore.Identity;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Services.User
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(UserManager<AppUser> userManager, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<PageResult<UserListItemResponse>> GetAllAsync(
            PagingRequest request
        )
        {
            return await _unitOfWork.Users.GetAllWithRolesAsync(request);
        }

        public async Task<UserResponse> GetByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId)
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            return user;
        }

        public async Task CreateAsync(CreateUserRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new BadRequestException(ErrorMessages.User.UserAlreadyExists);

            var newUser = _mapper.Map<CreateUserRequest, AppUser>(request);
            newUser.UserName = newUser.Email;
            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
                throw new BadRequestException(ErrorMessages.User.CreateFailed);
        }

        public async Task UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var user = await GetUserOrThrowAsync(id);

            _mapper.Map(request, user);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException(ErrorMessages.User.UpdateFailed);
        }

        public async Task DeleteAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                throw new BadRequestException(ErrorMessages.User.InvalidIds);

            var affected = await _unitOfWork.Users.DeleteByIdsAsync(ids);
            if (affected == 0)
                throw new NotFoundException(ErrorMessages.User.UserNotFound);
        }

        public async Task ChangeMyPasswordAsync(
            Guid id,
            ChangeMyPasswordRequest request
        )
        {
            var user = await GetUserOrThrowAsync(id);

            var sameAsCurrent = await _userManager.CheckPasswordAsync(user, request.NewPassword);
            if (sameAsCurrent)
                throw new BadRequestException(
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
                throw new BadRequestException(code);
            }
        }

        public async Task SetPasswordAsync(Guid id, SetPasswordRequest request)
        {
            var user = await GetUserOrThrowAsync(id);

            var sameAsCurrent = await _userManager.CheckPasswordAsync(user, request.NewPassword);
            if (sameAsCurrent)
                throw new BadRequestException(
                    ErrorMessages.User.ChangePassword.NewPasswordSameAsCurrent
                );

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(ErrorMessages.User.SetPasswordFailed);
        }

        public async Task ChangeEmailAsync(Guid id, ChangeEmailRequest request)
        {
            var user = await GetUserOrThrowAsync(id);
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.Email);
            var result = await _userManager.ChangeEmailAsync(user, request.Email, token);
            if (!result.Succeeded)
                throw new BadRequestException(ErrorMessages.User.ChangeEmailFailed);
        }

        public async Task AssignRolesToUserAsync(Guid id, string[] roles)
        {
            var user = await GetUserOrThrowAsync(id);

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _unitOfWork.Users.RemoveUserFromRoles(id, currentRoles);

            var addRoles = await _userManager.AddToRolesAsync(user, roles);
            if (!addRoles.Succeeded)
                throw new BadRequestException(ErrorMessages.User.AssignRolesFailed);
        }

        private async Task<AppUser> GetUserOrThrowAsync(Guid userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);
        }

        private static string FormatErrors(IdentityResult result) =>
            string.Join(" | ", result.Errors.Select(e => e.Description));
    }
}
