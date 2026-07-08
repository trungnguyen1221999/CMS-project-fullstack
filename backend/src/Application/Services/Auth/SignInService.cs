using Application.Constants;
using Application.Contracts.Auth.Requests;
using Application.Contracts.Auth.Responses;
using Application.Services.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using static Application.Exceptions.CustomException;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Services.Auth
{
    public class SignInService : ISignInService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public SignInService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
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
            var existingUser =
                await _userManager.FindByEmailAsync(request.Email)
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            var result = await _signInManager.CheckPasswordSignInAsync(
                existingUser,
                request.Password,
                false
            );

            if (!result.Succeeded)
                throw new BadRequestException(ErrorMessages.Auth.InvalidPassword);

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
                throw new BadRequestException(ErrorMessages.User.UpdateFailed);

            return new SignInResponse { Token = token, RefreshToken = refreshToken };
        }
    }
}
