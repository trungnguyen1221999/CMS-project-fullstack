using Application.Repositories;
using Domain.Cores.Content;

namespace Infrastructure.Repositories
{
    public class PostTagsRepository : RepositoryBase<PostTag, Guid>, IPostTagsRepository
    {
        public PostTagsRepository(ApplicationDbContext context)
            : base(context) { }

        public void AddTagToPost(Guid postId, Guid tagId)
        {
            _context.Add(new PostTag { PostId = postId, TagId = tagId });
        }

        public void ClearAllTagsFromPost(Guid postId)
        {
            var postTags = _context.PostTags.Where(pt => pt.PostId == postId);
            _context.RemoveRange(postTags);
        }
    }
}
