using Application.DTOs.Request.Auth;
using Application.DTOs.Response.Auth;

namespace Application.Services
{
    public interface ISignInService
    {
        Task<SignInResponseDto> SignInAsync(SignInRequestDto request);
    }
}
