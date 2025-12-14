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
    Task<bool> SoftDeleteAsync(int productId, CancellationToken ct);
    Task<bool> UpdateIsActiveAsync(int productId, bool isActive, CancellationToken ct);
    void UpdateProduct(Product product);
}
