using Application.DTOs.Request.Auth;
using Application.DTOs.Response.Auth;
using Application.Services;

namespace Infrastructure.Services.Auth
{
    public class SignInService : ISignInService
    {
        public Task<SignInResponseDto> SignInAsync(SignInRequestDto request)
        {
            throw new NotImplementedException();
        }
    }
}
