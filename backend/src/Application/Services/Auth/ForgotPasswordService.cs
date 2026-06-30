using Application.Constants;
using Application.Contracts.Common;
using Application.Contracts.Users.Requests;
using Application.Services.Otp;
using Microsoft.AspNetCore.Identity;
using AppUser = Domain.Cores.Identity.User;

namespace Application.Services.Auth
{
    public class ForgotPasswordService : IForgotPasswordService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;

        public ForgotPasswordService(
            UserManager<AppUser> userManager,
            IEmailService emailService
        )
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<WriteResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return WriteResponse.Failure(ErrorMessages.User.UserNotFound);

            await _emailService.SendOtpAsync(request.Email);
            return WriteResponse.Success();
        }

        public async Task<WriteResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return WriteResponse.Failure(ErrorMessages.User.UserNotFound);

            var isValid = await _emailService.ValidateOtpAsync(request.Email, request.Code);
            if (!isValid)
                return WriteResponse.Failure(ErrorMessages.Auth.CodeInvalid);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!result.Succeeded)
                return WriteResponse.Failure(
                    ErrorMessages.Auth.ResetPasswordFailed,
                    string.Join(" | ", result.Errors.Select(e => e.Description))
                );

            await _emailService.RemoveOtpAsync(request.Email);
            return WriteResponse.Success();
        }
    }
}
