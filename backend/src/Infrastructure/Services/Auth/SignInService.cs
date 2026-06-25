using Application.DTOs.Request.Auth;
using Application.DTOs.Response.Auth;
using Application.Services.Auth;
using Application.Services.Token;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Auth
{
    public class SignInService : ISignInService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public SignInService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<SignInResponseDto> SignInAsync(SignInRequestDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser == null)
            {
                return new SignInResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "User not found.",
                };
            }
            var result = await _signInManager.CheckPasswordSignInAsync(
                existingUser,
                request.Password,
                false
            );

            if (!result.Succeeded)
            {
                return new SignInResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid password.",
                };
            }
            var token = await _tokenService.GenerateAccessToken(existingUser);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpiryDays = _configuration.GetValue<int>(
                "JwtSettings:RefreshTokenExpiryDays"
            );
            existingUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);
            existingUser.RefreshToken = refreshToken;
            existingUser.IsActive = true;
            var updateUser = await _userManager.UpdateAsync(existingUser);

            if (!updateUser.Succeeded)
            {
                var errors = updateUser.Errors.Select(e => e.Description);

                return new SignInResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = string.Join(" ,", errors),
                };
            }

            return new SignInResponseDto
            {
                IsSuccess = true,
                Token = token,
                RefreshToken = refreshToken,
            };
        }
    }
}
