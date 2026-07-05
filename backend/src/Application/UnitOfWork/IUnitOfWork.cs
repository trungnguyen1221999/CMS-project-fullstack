using Application.Repositories;

namespace Application.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IPostRepository Posts { get; }
        Task<int> CompleteAsync();
    }
}
