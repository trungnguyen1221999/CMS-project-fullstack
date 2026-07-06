namespace Application.Contracts.Posts.Request
{
    public class PostPagingRequest
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
