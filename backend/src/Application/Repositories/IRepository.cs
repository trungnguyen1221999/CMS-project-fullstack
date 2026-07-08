using System.Linq.Expressions;
using Application.Contracts.Common;

namespace Application.Repositories
{
    public interface IRepository<T, TKey>
        where T : class
    {
        //Read
        Task<T?> GetByIdAsync(TKey key);
        Task<IEnumerable<T>> GetAllAsync();

        //Build Sql query
        IQueryable<T> Find(Expression<Func<T, bool>> expression);
        IEnumerable<T> FindList(Expression<Func<T, bool>> expression);

        //Write

        void Add(T entity);
        void AddRange(IEnumerable<T> entities);

        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
