using System.Linq.Expressions;

namespace Contracts;


public interface IGenericRepository<T> where T : class
{
    // -----------------------------
    // Basic CRUD
    // -----------------------------
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<T?> GetByIdNoTrackingAsync(int id, CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    void Update(T entity);
    Task UpdateAsync(T entity, CancellationToken ct = default);

    void Remove(T entity);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    // -----------------------------
    // Query helpers
    // -----------------------------
    Task<bool> AnyAsync(Expression<Func<T, bool>>? criteria = null, CancellationToken ct = default);

    Task<int> CountAsync(Expression<Func<T, bool>>? criteria = null, CancellationToken ct = default);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> criteria,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default);

    Task<T?> SingleOrDefaultAsync(
        Expression<Func<T, bool>> criteria,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default);

    Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>>? criteria = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? skip = null,
        int? take = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default);

    Task<List<T>> FindAsync(
        Expression<Func<T, bool>> criteria,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default);

    // -----------------------------
    // Projection (DTO) - اختياري لكنه مفيد للأداء
    // -----------------------------
    Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? criteria = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? skip = null,
        int? take = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default);
}