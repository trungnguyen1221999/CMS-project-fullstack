namespace Application.Common
{
    public class OperationResult
    {
        public bool Succeeded { get; set; }
        public string? ErrorCode { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
