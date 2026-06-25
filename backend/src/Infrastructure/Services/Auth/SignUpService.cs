using Application.DTOs.Request.Auth;
using Application.DTOs.Response.Auth;
using Application.Services.Auth;
using AutoMapper;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Auth
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

        public async Task<SignUpResponseDto> SignUpAsync(SignUpRequestDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new SignUpResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "User with this email already exists.",
                };
            }

            var user = _mapper.Map<SignUpRequestDto, User>(request);
            user.UserName = user.Email;
            user.CreatedAt = DateTime.UtcNow;
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));

                return new SignUpResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = string.IsNullOrWhiteSpace(errors)
                        ? "Failed to create user."
                        : errors,
                };
            }
            var addRole = await _userManager.AddToRoleAsync(user, "User");
            if (!addRole.Succeeded)
            {
                return new SignUpResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to assign role.",
                };
            }
            return new SignUpResponseDto { IsSuccess = true };
        }
    }
}
