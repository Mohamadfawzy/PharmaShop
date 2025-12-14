
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;

namespace Contracts.IServices;

public interface IProductService
{
    Task<bool> AddProductImagesAsync(IEnumerable<Stream> imageStreams, int productId, string rootPath, CancellationToken ct);
    Task<int> CreateProductAsync(ProductCreateDto productDto, CancellationToken ct);
    Task<AppResponse<ProductSubDetailsDto>> CreateProductWithImagesAsync(ProductCreateDto dto, IEnumerable<Stream> imageStreams, string rootPath , CancellationToken ct = default);
    Task DeleteProductImageAsync(int productId, int imageId, string rootPath, CancellationToken ct);
    Task<AppResponse<List<ProductSubDetailsDto>>> GetProductsAsync(ProductParameters parameters);
    Task<AppResponse> SoftDeleteProductAsync(int productId, CancellationToken ct);
    Task<AppResponse> UpdateProductAsync(int productId, ProductUpdateDto dto, CancellationToken ct);
    Task<AppResponse> UpdateProductIsActiveAsync(int productId, bool isActive, CancellationToken ct);
}
