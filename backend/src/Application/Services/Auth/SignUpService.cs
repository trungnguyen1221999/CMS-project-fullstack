using Application.Constants;
using Application.Contracts.Auth.Requests;
using Application.Contracts.Auth.Responses;
using AutoMapper;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Auth
{
    public class SignUpService : ISignUpService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public SignUpService(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<SignUpResponse> SignUpAsync(SignUpRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new SignUpResponse
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.UserAlreadyExists,
                    ErrorMessage = ErrorMessages.User.UserAlreadyExists,
                };
            }

            var user = _mapper.Map<SignUpRequest, User>(request);
            user.UserName = user.Email;
            user.CreatedAt = DateTime.UtcNow;

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return new SignUpResponse
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.User.CreateFailed,
                    ErrorMessage = errors.Any()
                        ? string.Join(" | ", errors)
                        : ErrorMessages.User.CreateFailed,
                };
            }

            var addRole = await _userManager.AddToRoleAsync(user, "User");
            if (!addRole.Succeeded)
            {
                var errors = addRole.Errors.Select(e => e.Description).ToList();
                return new SignUpResponse
                {
                    IsSuccess = false,
                    ErrorCode = ErrorMessages.Auth.FailedToAssignRole,
                    ErrorMessage = errors.Any()
                        ? string.Join(" | ", errors)
                        : ErrorMessages.Auth.FailedToAssignRole,
                };
            }

            return new SignUpResponse { IsSuccess = true };
        }
    }
}
