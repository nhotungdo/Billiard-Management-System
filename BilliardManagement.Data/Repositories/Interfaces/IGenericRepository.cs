using System.Linq.Expressions;

namespace BilliardManagement.Data.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);
        Task<T?> GetByIdAsync(object id);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
