namespace Application.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task<int> CompleteAsync();
    }
}
