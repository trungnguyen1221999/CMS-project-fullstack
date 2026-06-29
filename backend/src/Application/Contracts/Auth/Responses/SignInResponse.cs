using Application.Contracts.Common;

namespace Application.Contracts.Auth.Responses
{
    public class SignInResponse : WriteResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }

        public static SignInResponse Success(string token, string refreshToken) =>
            new() { IsSuccess = true, Token = token, RefreshToken = refreshToken };

        public new static SignInResponse Failure(string errorCode, string? errorMessage = null) =>
            new()
            {
                IsSuccess = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage ?? errorCode,
            };
    }
}
