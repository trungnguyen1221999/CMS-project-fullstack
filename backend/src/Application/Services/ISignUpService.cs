using Application.DTOs.Request.Auth;
using Application.DTOs.Response.Auth;

namespace Application.Services
{
    public interface ISignUpService
    {
        Task<SignUpResponseDto> SignUpAsync(SignUpRequestDto request);
    }
}
