using System.Linq.Expressions;
using Contracts;
using Microsoft.EntityFrameworkCore;

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

    public async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

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
}
