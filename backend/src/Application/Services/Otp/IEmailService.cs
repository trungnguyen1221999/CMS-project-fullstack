namespace Application.Services.Otp
{
    public interface IEmailService
    {
        Task SendOtpAsync(string email);
        Task<bool> ValidateOtpAsync(string email, string code);
        Task RemoveOtpAsync(string email);
    }
}
