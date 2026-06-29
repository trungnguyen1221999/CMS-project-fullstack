using Application.Constants;
using Application.Contracts.Auth.Requests;
using Application.Contracts.Auth.Responses;
using Application.Services.Token;
using Domain.Cores.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Services.Auth
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

        public async Task<SignInResponse> SignInAsync(SignInRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser == null)
                return SignInResponse.Failure(ErrorMessages.User.UserNotFound);

            var result = await _signInManager.CheckPasswordSignInAsync(
                existingUser,
                request.Password,
                false
            );

            if (!result.Succeeded)
                return SignInResponse.Failure(ErrorMessages.Auth.InvalidPassword);

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
                return SignInResponse.Failure(
                    ErrorMessages.User.UpdateFailed,
                    string.Join(" | ", updateUser.Errors.Select(e => e.Description))
                );

            return SignInResponse.Success(token, refreshToken);
        }
    }
}
