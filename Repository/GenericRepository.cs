using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly RepositoryContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(RepositoryContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id,CancellationToken ct= default) =>
        await _dbSet.FindAsync(id);

    public async Task<T?> GetByIdNoTrackingAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);
    }

    public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? criteria = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? skip = null,
            int? take = null)
    {
        IQueryable<T> query = _dbSet;

        // Apply Filter
        if (criteria != null)
        {
            query = query.Where(criteria);
        }

        // Apply OrderBy
        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // Apply Skip
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        // Apply Take
        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression) =>
        await _dbSet.Where(expression).ToListAsync();

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await _dbSet.AddAsync(entity);

    public void Update(T entity) =>
        _dbSet.Update(entity);

    public void Remove(T entity) =>
        _dbSet.Remove(entity);

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null,CancellationToken ct = default)
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
            query = query.Where(filter);

        return await query.CountAsync(ct);
    }


    public Task DeleteAsync(T entity, CancellationToken ct)
    {
        throw new NotImplementedException();
    }




}
