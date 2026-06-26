namespace Application.Contracts.Common
{
    public class WriteResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
