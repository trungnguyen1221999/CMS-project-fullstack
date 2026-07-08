using Application.Contracts.Auth.Requests;

namespace Application.Services.Auth
{
    public interface ISignUpService
    {
        Task SignUpAsync(SignUpRequest request);
    }
}
