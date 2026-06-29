using Application.Repositories;

namespace Application.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        Task<int> CompleteAsync();
    }
}
