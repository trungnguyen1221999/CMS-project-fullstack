using Application.Contracts.Common;

namespace Application.Contracts.Auth.Responses
{
    public class SignInResponse : WriteResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
