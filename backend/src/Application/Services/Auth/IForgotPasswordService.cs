using Application.Contracts.Users.Requests;

namespace Application.Services.Auth
{
    public interface IForgotPasswordService
    {
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
    }
}
