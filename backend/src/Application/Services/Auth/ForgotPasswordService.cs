using Application.Constants;
using Application.Contracts.Users.Requests;
using Application.Services.Otp;
using Microsoft.AspNetCore.Identity;
using static Application.Exceptions.CustomException;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Services.Auth
{
    public class ForgotPasswordService : IForgotPasswordService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;

        public ForgotPasswordService(UserManager<AppUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user =
                await _userManager.FindByEmailAsync(request.Email)
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            await _emailService.SendOtpAsync(request.Email);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user =
                await _userManager.FindByEmailAsync(request.Email)
                ?? throw new NotFoundException(ErrorMessages.User.UserNotFound);

            var isValid = await _emailService.ValidateOtpAsync(request.Email, request.Code);
            if (!isValid)
                throw new BadRequestException(ErrorMessages.Auth.CodeInvalid);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!result.Succeeded)
                throw new BadRequestException(ErrorMessages.Auth.ResetPasswordFailed);

            await _emailService.RemoveOtpAsync(request.Email);
        }
    }
}
