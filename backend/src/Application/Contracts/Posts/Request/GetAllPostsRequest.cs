namespace Application.Contracts.Posts.Request
{
    public class GetAllPostsRequest : PostPagingRequest
    {
        public string? Keyword { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
