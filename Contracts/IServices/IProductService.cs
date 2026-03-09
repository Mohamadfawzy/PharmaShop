
using Shared.Models.Dtos.Product;
using Shared.Models.Dtos.Product.Units;
using Shared.Models.RequestFeatures;
using Shared.Responses;

namespace Contracts.IServices;

public interface IProductService
{
    Task<AppResponse<int>> CreateProductAsync(ProductCreateDto dto, CancellationToken ct);
    Task<AppResponse<ProductDetailsDto>> GetProductByIdAsync(int productId, bool includeDeleted, CancellationToken ct);
    Task<AppResponse<List<ProductDetailsDto>>> GetDeletedProductsAsync(int skip, int take, CancellationToken ct);

    Task<AppResponse> UpdateProductAsync(int productId, ProductUpdateDto dto, CancellationToken ct);
    Task<AppResponse> SetActiveAsync(int productId, bool isActive, ProductStateChangeDto dto, CancellationToken ct);
    Task<AppResponse> SetDeletedAsync(int productId, bool isDeleted, ProductStateChangeDto dto, CancellationToken ct);
    Task<AppResponse<List<ProductListItemDto>>> GetProductsAsync(ProductListQueryDto query, CancellationToken ct);
}
