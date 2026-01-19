using Entities.Models;
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;

namespace Contracts;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetForUpdateAsync(int id, int pharmacyId, CancellationToken ct);

    // لتعيين OriginalValue للـ RowVersion بدون كشف DbContext في Service
    void SetRowVersionOriginalValue(Product entity, byte[] rowVersion);

    Task<PagedResult<ProductListItemDto>> SearchAsync(
    int pharmacyId,
    ProductListQueryDto query,
    CancellationToken ct);
    Task<(OpenBoxResultDto? Result, Dictionary<string, string[]>? FieldErrors, string? ErrorMessage)> ConvertUnitsAsync(int pharmacyId, int productId, OpenBoxDto dto, CancellationToken ct);
    Task<(ReceiveStockResultDto? Result, Dictionary<string, string[]>? FieldErrors, string? ErrorMessage)> ReceiveStockAsync(int pharmacyId, int productId, ReceiveStockDto dto, CancellationToken ct);
    Task<(StockAdjustmentResultDto? Result, Dictionary<string, string[]>? FieldErrors, string? ErrorMessage)> AdjustStockAsync(int pharmacyId, int productId, StockAdjustmentDto dto, CancellationToken ct);
}
