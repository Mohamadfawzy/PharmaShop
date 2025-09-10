
using Shared.Models.Dtos.Product;

namespace Contracts.IServices;

public interface IProductService
{
    Task<object> CreateProductWithImagesAsync(ProductCreateDto dto,Stream imageFile, string rootPath);
    Task<object> ReadAllProducts();
}
