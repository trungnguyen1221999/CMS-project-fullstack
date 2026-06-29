namespace Application.Contracts.Common
{
    public class ReadResponse<T> : WriteResponse
    {
        public T? Data { get; set; }

        public static ReadResponse<T> Success(T data) =>
            new() { IsSuccess = true, Data = data };

        public new static ReadResponse<T> Failure(string errorCode, string? errorMessage = null) =>
            new()
            {
                IsSuccess = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage ?? errorCode,
            };
    }
}
