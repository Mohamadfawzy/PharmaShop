using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
     
    private readonly RepositoryContext context;

    public CategoryRepository(RepositoryContext context) : base(context)
    {
        this.context = context;
    }

    public async Task<bool> ExistsByIdAsync(int categoryId, CancellationToken ct)
    {
        return await context.Categories
            .AnyAsync(c => c.Id == categoryId && c.IsActive, ct);
    }

    public async Task<bool> UpdateImageUrlAsync(int id, string imageUrl, CancellationToken ct)
    {
        var affectedRows = await context.Categories
            .Where(p => p.Id == id && !p.IsDeleted)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(p => p.ImageUrl, imageUrl),
                ct);

        return affectedRows > 0;
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken ct)
    {
        return await context.Categories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == null && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<List<Category>> GetAllForTreeAsync(CancellationToken ct)
    {
        return await context.Categories
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }
}
