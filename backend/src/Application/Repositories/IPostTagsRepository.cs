using Domain.Cores.Content;

namespace Application.Repositories
{
    public interface IPostTagsRepository : IRepository<PostTag, Guid>
    {
        void AddTagToPost(Guid postId, Guid tagId);
        void ClearAllTagsFromPost(Guid postId);
    }
}
