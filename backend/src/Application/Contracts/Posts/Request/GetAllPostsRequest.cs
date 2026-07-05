namespace Application.Contracts.Posts.Request
{
    public class GetAllPostsRequest
    {
        public string? Keyword { get; set; }
        public Guid? CategoryId { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
