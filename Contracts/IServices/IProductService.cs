
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;

namespace Contracts.IServices;

public interface IProductService
{
    Task<AppResponse<int>> CreateProductAsync(ProductCreateDto dto, CancellationToken ct);
    Task<AppResponse<List<ProductListItemDto>>> SearchProductsAsync(ProductSearchQueryParams query, CancellationToken ct);
}
