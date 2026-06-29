using Application.Constants;
using Application.Contracts.Auth.Requests;
using Application.Contracts.Auth.Responses;
using AutoMapper;
using Domain.Constants;
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
                return SignUpResponse.Failure(ErrorMessages.User.UserAlreadyExists);

            var user = _mapper.Map<SignUpRequest, User>(request);
            user.UserName = user.Email;
            user.CreatedAt = DateTime.UtcNow;

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return SignUpResponse.Failure(
                    ErrorMessages.User.CreateFailed,
                    string.Join(" | ", result.Errors.Select(e => e.Description))
                );

            var addRole = await _userManager.AddToRoleAsync(user, Roles.User);
            if (!addRole.Succeeded)
                return SignUpResponse.Failure(
                    ErrorMessages.Auth.FailedToAssignRole,
                    string.Join(" | ", addRole.Errors.Select(e => e.Description))
                );

            return SignUpResponse.Success();
        }
    }
}
