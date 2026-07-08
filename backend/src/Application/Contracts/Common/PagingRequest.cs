namespace Application.Contracts.Common
{
    public class PagingRequest
    {
        public string? Keyword { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
