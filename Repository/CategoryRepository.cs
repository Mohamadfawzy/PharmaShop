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

    public async Task<Category?> GetByIdAsync(int categoryId, CancellationToken ct)
    {
        return await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryId && !c.IsDeleted, ct);
    }


    //public async Task AddAsync(Category category, CancellationToken ct)
    //{
    //    await context.Categories.AddAsync(category, ct);
    //}


}
