
using Entities.Models;

namespace Contracts;

public interface ICategoryRepository : IGenericRepository<Category>
{
    //Task AddAsync(Category category, CancellationToken ct);
    Task<bool> ExistsByIdAsync(int categoryId, CancellationToken ct);
    Task<Category?> GetByIdAsync(int categoryId, CancellationToken ct);
    Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken ct);
    Task<bool> UpdateImageUrlAsync(int id, string imageUrl, CancellationToken ct);
    //Task UpdateAsync(object category, CancellationToken ct);
}
