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
                    .SetProperty(p => p.ImageId, imageUrl),
                ct);

        return affectedRows > 0;
    }

    public async Task<bool> UpdateImageMetaAsync(
         int id,
         string? imageId,
         byte? imageFormat,
         CancellationToken ct)
    {
        // IMPORTANT:
        // - imageId can be null to clear image fields (optional behavior).
        // - version is incremented atomically in the database to avoid race conditions.

        var dateTimeNow = DateTime.Now;
        var affectedRows = await context.Categories
            .Where(c => c.Id == id && !c.IsDeleted)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.ImageId, imageId)
                .SetProperty(c => c.ImageFormat, imageFormat)
                .SetProperty(c => c.ImageUpdatedAt, dateTimeNow)
                .SetProperty(c => c.ImageVersion, c => c.ImageVersion + 1)
                .SetProperty(c => c.UpdatedAt, dateTimeNow),
                ct);

        return affectedRows > 0;
    }


    public async Task AddCategoryAuditLogsRangeAsync(List<CategoryAuditLog> logs)
    {
        if (logs == null || logs.Count == 0)
            return;

        await context.CategoryAuditLogs.AddRangeAsync(logs);
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

    public async Task<bool> SetActiveStatusAsync(int categoryId, bool isActive, CancellationToken ct)
    {
        var affectedRows = await context.Categories
            .Where(c => c.Id == categoryId && !c.IsDeleted)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.IsActive, isActive)
                .SetProperty(c => c.UpdatedAt, DateTime.UtcNow),
                ct
            );

        return affectedRows > 0;
    }
}
