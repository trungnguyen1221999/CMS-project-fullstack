using Application.Contracts.Auth.Requests;
using Application.Contracts.Auth.Responses;

namespace Application.Services.Auth
{
    public interface ISignInService
    {
        Task<SignInResponse> SignInAsync(SignInRequest request);
    }
}
