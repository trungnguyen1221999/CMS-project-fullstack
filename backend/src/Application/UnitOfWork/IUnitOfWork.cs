using Application.Repositories;

namespace Application.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IPostRepository Posts { get; }

        ICategoryRepository Categories { get; }
        ITagRepository Tags { get; }

        IPostTagsRepository PostTags { get; }
        Task<int> CompleteAsync();
    }
}
