using Application.Constants;
using Application.Contracts.Auth.Requests;
using AutoMapper;
using Domain.Constants;
using Microsoft.AspNetCore.Identity;
using static Application.Exceptions.CustomException;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Services.Auth
{
    public class SignUpService : ISignUpService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public SignUpService(UserManager<AppUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task SignUpAsync(SignUpRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new BadRequestException(ErrorMessages.User.UserAlreadyExists);

            var user = _mapper.Map<SignUpRequest, AppUser>(request);
            user.UserName = user.Email;
            user.CreatedAt = DateTime.UtcNow;

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new BadRequestException(ErrorMessages.User.CreateFailed);

            var addRole = await _userManager.AddToRoleAsync(user, Roles.User);
            if (!addRole.Succeeded)
                throw new BadRequestException(ErrorMessages.Auth.FailedToAssignRole);
        }
    }
}
