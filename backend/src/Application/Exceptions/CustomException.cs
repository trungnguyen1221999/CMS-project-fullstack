namespace Application.Exceptions
{
    public abstract class CustomException : Exception
    {
        public string ErrorCode { get; }

        protected CustomException(string errorCode, string? message = null)
            : base(message ?? errorCode)
        {
            ErrorCode = errorCode;
        }

        public class NotFoundException : CustomException
        {
            public NotFoundException(string errorCode, string? message = null)
                : base(errorCode, message) { }
        }

        public class ForbiddenException : CustomException
        {
            public ForbiddenException(string errorCode, string? message = null)
                : base(errorCode, message) { }
        }

        public class BadRequestException : CustomException
        {
            public BadRequestException(string errorCode, string? message = null)
                : base(errorCode, message) { }
        }
    }
}
