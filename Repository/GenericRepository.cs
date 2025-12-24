using Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly RepositoryContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(RepositoryContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // -----------------------------
    // Helpers
    // -----------------------------
    protected IQueryable<T> ApplyCriteria(
        IQueryable<T> query,
        Expression<Func<T, bool>>? criteria,
        Func<IQueryable<T>, IQueryable<T>>? include,
        bool asNoTracking)
    {
        if (asNoTracking)
            query = query.AsNoTracking();

        if (include is not null)
            query = include(query);

        if (criteria is not null)
            query = query.Where(criteria);

        return query;
    }

    // -----------------------------
    // Basic CRUD
    // -----------------------------
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // FindAsync supports tracking by default (good for updates)
        return await _dbSet.FindAsync(new object?[] { id }, ct);
    }

    public virtual async Task<T?> GetByIdNoTrackingAsync(int id, CancellationToken ct = default)
    {
        // Works for any entity with "Id" property
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);
    }

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public virtual Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        if (entities is null) throw new ArgumentNullException(nameof(entities));
        return _dbSet.AddRangeAsync(entities, ct);
    }

    public virtual void Update(T entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        _dbSet.Update(entity);
    }

    public virtual Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual void Remove(T entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        _dbSet.Remove(entity);
    }

    public virtual Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        if (entities is null) throw new ArgumentNullException(nameof(entities));
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    // -----------------------------
    // Query helpers
    // -----------------------------
    public virtual async Task<bool> AnyAsync(
        Expression<Func<T, bool>>? criteria = null,
        CancellationToken ct = default)
    {
        return criteria is null
            ? await _dbSet.AnyAsync(ct)
            : await _dbSet.AnyAsync(criteria, ct);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? criteria = null,
        CancellationToken ct = default)
    {
        return criteria is null
            ? await _dbSet.CountAsync(ct)
            : await _dbSet.CountAsync(criteria, ct);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> criteria,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        if (criteria is null) throw new ArgumentNullException(nameof(criteria));

        IQueryable<T> query = ApplyCriteria(_dbSet, criteria, include, asNoTracking);
        return await query.FirstOrDefaultAsync(ct);
    }

    public virtual async Task<T?> SingleOrDefaultAsync(
        Expression<Func<T, bool>> criteria,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        if (criteria is null) throw new ArgumentNullException(nameof(criteria));

        IQueryable<T> query = ApplyCriteria(_dbSet, criteria, include, asNoTracking);
        return await query.SingleOrDefaultAsync(ct);
    }

    public virtual async Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>>? criteria = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? skip = null,
        int? take = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        IQueryable<T> query = ApplyCriteria(_dbSet, criteria, include, asNoTracking);

        if (orderBy is not null)
            query = orderBy(query);

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        return await query.ToListAsync(ct);
    }

    public virtual async Task<List<T>> FindAsync(
        Expression<Func<T, bool>> criteria,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        if (criteria is null) throw new ArgumentNullException(nameof(criteria));

        IQueryable<T> query = ApplyCriteria(_dbSet, criteria, include, asNoTracking);
        return await query.ToListAsync(ct);
    }

    // -----------------------------
    // Projection (DTO) optional
    // -----------------------------
    public virtual async Task<List<TResult>> GetAllAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? criteria = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? skip = null,
        int? take = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        if (selector is null) throw new ArgumentNullException(nameof(selector));

        IQueryable<T> query = ApplyCriteria(_dbSet, criteria, include, asNoTracking);

        if (orderBy is not null)
            query = orderBy(query);

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        return await query.Select(selector).ToListAsync(ct);
    }
}