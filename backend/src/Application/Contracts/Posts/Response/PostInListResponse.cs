using Domain.Cores.Content;

namespace Application.Contracts.Posts.Response
{
    public class PostInListResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Thumbnail { get; set; }
        public int ViewCount { get; set; }
        public string CategorySlug { set; get; } = string.Empty;

        public string CategoryName { set; get; } = string.Empty;
        public string AuthorUserName { set; get; } = string.Empty;
        public string AuthorName { set; get; } = string.Empty;

        public PostStatus Status { set; get; }
        public bool IsPaid { get; set; }
        public decimal RoyaltyAmount { get; set; }
        public DateTime? PaidDate { get; set; }

        public class AutoMapperProfile : AutoMapper.Profile
        {
            public AutoMapperProfile()
            {
                CreateMap<Post, PostInListResponse>();
            }
        }
    }
}
