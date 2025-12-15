
using Entities.Models;

namespace Contracts;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken ct);
    Task<bool> ExistsByIdAsync(int categoryId, CancellationToken ct);
    Task<Category?> GetByIdAsync(int categoryId, CancellationToken ct);
}
