
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;

namespace Contracts.IServices;

public interface IProductService
{
    Task<AppResponse<ProductSubDetailsDto>> CreateProductWithImagesAsync(ProductCreateDto dto, IEnumerable<Stream> imageStreams, string rootPath , CancellationToken ct = default);
    Task<AppResponse<List<ProductSubDetailsDto>>> GetProductsAsync(ProductParameters parameters);
    //Task<AppResponse<List<ProductSubDetailsDto>>> ReadAllProducts(int pageNumber, int pageSize);
}
