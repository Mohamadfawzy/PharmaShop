
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;

namespace Contracts.IServices;

public interface IProductService
{
    Task<AppResponse<int>> CreateProductAsync(ProductCreateDto dto, CancellationToken ct);
    Task<AppResponse> UpdateProductAsync(int productId, ProductUpdateDto dto, CancellationToken ct);
    Task<AppResponse<ProductDetailsDto>> GetProductByIdAsync(int productId, CancellationToken ct);

}
