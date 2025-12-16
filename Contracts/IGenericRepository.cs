using System.Linq.Expressions;

namespace Contracts;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct= default);
    Task<IEnumerable<T>> GetAllAsync(
           Expression<Func<T, bool>>? filter = null,
           Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
           int? skip = null,
           int? take = null);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null, CancellationToken ct = default);
    Task<T?> GetByIdNoTrackingAsync(int id, CancellationToken ct = default);
}
