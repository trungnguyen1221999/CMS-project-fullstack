using Application.DTOs.Request.Auth;
using Application.DTOs.Response.Auth;

namespace Application.Services.Auth
{
    public interface ISignUpService
    {
        Task<SignUpResponseDto> SignUpAsync(SignUpRequestDto request);
    }
}
