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

        IPostInSeriesRepository PostInSeries { get; }

        IPostActivityLogRepository PostActivityLogs { get; }
        Task<int> CompleteAsync();
    }
}
