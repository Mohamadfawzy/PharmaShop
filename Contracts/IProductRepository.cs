using Entities.Models;
using Shared.Models.Dtos.Product;

namespace Contracts;

public interface IProductRepository : IGenericRepository<Product>
{
    Task AddProductImagesAsync(IEnumerable<ProductImage> productImages);
    Task<bool> ExistsByBarcodeAsync(string barcode, CancellationToken ct = default);
    Task<List<ProductUpdateDto>> GetAll();
    IQueryable<ProductSubDetailsDto> GetAllQueryable();
    Task<ProductImage?> GetProductImageByIdAsync(int productId, int imageId, CancellationToken ct);
    IQueryable<Product> Query();
    void RemoveProductImage(ProductImage image);
}
