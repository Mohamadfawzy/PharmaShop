using Contracts;
using Entities.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Product;

namespace Repository;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly RepositoryContext context;

    public ProductRepository(RepositoryContext context) : base(context)
    {
        this.context = context;
    }

    public async Task<List<ProductUpdateDto>> GetAll()
    {
        var products = await context.Products
            .ProjectToType<ProductUpdateDto>()
            .ToListAsync();
        return products;
    }

    public async Task AddProductImagesAsync(IEnumerable<ProductImage> productImages)
    {
        if (productImages == null)
            throw new ArgumentNullException(nameof(productImages));

        var images = productImages.ToList();
        if (!images.Any())
            return;

        await context.ProductImages.AddRangeAsync(images);
        await context.SaveChangesAsync();
    }

    public async Task<ProductImage?> GetProductImageByIdAsync(int productId, int imageId, CancellationToken ct)
    {
        return await context.ProductImages
            .FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId, ct);
    }

    public async Task<bool> ExistsByBarcodeAsync(string barcode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;

        return await context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Barcode == barcode, ct);
    }


    public void RemoveProductImage(ProductImage image)
    {
        context.ProductImages.Remove(image);
    }


    public IQueryable<ProductSubDetailsDto> GetAllQueryable()
    {
        return context.Products.ProjectToType<ProductSubDetailsDto>();
    }

    public IQueryable<Product> Query()
    {
        return context.Products.AsQueryable();
    }




}
