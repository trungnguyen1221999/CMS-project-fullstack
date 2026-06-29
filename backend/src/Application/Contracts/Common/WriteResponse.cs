namespace Application.Contracts.Common
{
    public class WriteResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public static WriteResponse Success() => new() { IsSuccess = true };

        public static WriteResponse Failure(string errorCode, string? errorMessage = null) =>
            new()
            {
                IsSuccess = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage ?? errorCode,
            };
    }
}
