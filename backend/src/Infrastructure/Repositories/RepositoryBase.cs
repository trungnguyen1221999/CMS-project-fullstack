using System.Linq.Expressions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories
{
    public class RepositoryBase<T, TKey> : IRepository<T, TKey>
        where T : class
    {
        private readonly DbSet<T> _dbSet;
        protected readonly ApplicationDbContext _context;

        public RepositoryBase(ApplicationDbContext context)
        {
            _dbSet = context.Set<T>();
            _context = context;
        }

        //Write
        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        //Build Sql query
        public IQueryable<T?> Find(Expression<Func<T, bool>> expression)
        {
            return _dbSet.AsNoTracking().Where(expression);
        }

        public IEnumerable<T?> FindList(Expression<Func<T, bool>> expression)
        {
            return _dbSet.AsNoTracking().Where(expression);
        }

        //Read
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(TKey key)
        {
            return await _dbSet.FindAsync(key);
        }
    }
}
