using Application.DTOs.Request.Auth;
using Application.DTOs.Response.Auth;
using Application.Services;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Auth
{
    public class SignUpService : ISignUpService
    {
        private readonly UserManager<User> _userManager;

        public SignUpService(UserManager<User> userManager)
        {
            _userManager = userManager;
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

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
                Email = request.Email,
                IsActive = true,
            };

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

            return new SignUpResponseDto { IsSuccess = true };
        }
    }
}
