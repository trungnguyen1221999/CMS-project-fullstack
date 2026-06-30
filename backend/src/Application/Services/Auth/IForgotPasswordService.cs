using Application.Contracts.Common;
using Application.Contracts.Users.Requests;

namespace Application.Services.Auth
{
    public interface IForgotPasswordService
    {
        Task<WriteResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<WriteResponse> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
