
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;

namespace Contracts.IServices;

public interface IProductService
{
    Task<AppResponse<int>> CreateProductAsync(ProductCreateDto dto, CancellationToken ct);
    Task<AppResponse<List<ProductListItemDto>>> SearchProductsAsync(ProductSearchQueryParams query, CancellationToken ct);
    Task<AppResponse<List<ProductListItemDto>>> GetBestSellersAsync(int storeId, int periodDays, int page, int pageSize, CancellationToken ct);
    Task<AppResponse<ProductDetailsDto>> GetProductAsync(int id, bool includeInactive, CancellationToken ct);
    Task<AppResponse<bool>> UpdateProductAsync(int id, ProductCreateDto dto, CancellationToken ct);
    Task<AppResponse<bool>> SetProductStateAsync(int id, string action, CancellationToken ct);
    Task<AppResponse<List<ProductListItemDto>>> GetDeletedProductsAsync(int page, int pageSize, CancellationToken ct);
}
