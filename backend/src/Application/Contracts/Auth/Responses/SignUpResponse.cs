using Application.Contracts.Common;

namespace Application.Contracts.Auth.Responses
{
    public class SignUpResponse : WriteResponse
    {
        public new static SignUpResponse Success() => new() { IsSuccess = true };

        public new static SignUpResponse Failure(string errorCode, string? errorMessage = null) =>
            new()
            {
                IsSuccess = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage ?? errorCode,
            };
    }
}
