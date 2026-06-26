using Application.Contracts.Auth.Requests;
using Application.Contracts.Auth.Responses;

namespace Application.Services.Auth
{
    public interface ISignUpService
    {
        Task<SignUpResponse> SignUpAsync(SignUpRequest request);
    }
}
