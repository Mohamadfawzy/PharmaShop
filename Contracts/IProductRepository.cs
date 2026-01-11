using Entities.Models;
using Shared.Models.Dtos.Product;

namespace Contracts;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetForUpdateAsync(int id, int pharmacyId, CancellationToken ct);

    // لتعيين OriginalValue للـ RowVersion بدون كشف DbContext في Service
    void SetRowVersionOriginalValue(Product entity, byte[] rowVersion);
}
