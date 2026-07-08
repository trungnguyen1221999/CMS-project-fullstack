using Application.Contracts.Common;

namespace Application.Contracts.Posts.Request
{
    public class GetAllPostsRequest : PagingRequest
    {
        public Guid? CategoryId { get; set; }
    }
}
