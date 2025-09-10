using Contracts;
using Contracts.IServices;
using Entities.Models;
using Service.Mappings;
using Shared.Models.Dtos.Product;

namespace Service;

public class ProductService : IProductService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IImageService imageService;

    public ProductService(IUnitOfWork unitOfWork, IImageService imageService)
    {
        this.unitOfWork = unitOfWork;
        this.imageService = imageService;
    }

    public Task<object> ReadAllProducts()
    {
        return unitOfWork.Products.GetAll();
    }

    public Task<object> AddProducts()
    {
        return unitOfWork.Products.GetAll();
    }

    public async Task<object> CreateProductWithImagesAsync(ProductCreateDto dto, Stream imageFile, string rootPath)
    {
        string imageId = await imageService.SaveImageAsync(imageFile, rootPath);

        var product = dto.ToEntity();

        product.ProductImages = new List<ProductImage>()
        {
            new ProductImage {ImageUrl = imageId},
        };

        await unitOfWork.Products.AddAsync(product);
        await unitOfWork.CompleteAsync();
        return new {ProductName = product.Name, ProductImageUrl = imageId};
    }
    
    public async Task<object> CreateProductWithImagesAsyncOld(ProductCreateDto dto, Stream imageFile, string rootPath)
    {
        string imageId = await imageService.SaveImageAsync(imageFile, rootPath);

        var product = new Product
        {
            SubCategoryId = null,
            Name = "Panadol Extra",
            NameEn = "Panadol Extra",
            Description = "مسكن للصداع وآلام الجسم",
            DescriptionEn = "Pain reliever for headache and body aches",
            Barcode = "1234567890123",
            InternationalCode = "INT-987654",
            StockProductCode = "STK-4567",
            Price = 75.50m,
            OldPrice = 85.00m,
            IsAvailable = true,
            IsIntegrated = false,
            IntegratedAt = null,
            CategoryId = 2,
            CreatedAt = DateTime.Now,
            UpdatedAt = null,
            CreatedBy = "Admin",
            IsActive = true,
            Points = 15.5m,
            PromoDisc = 10m,
            PromoEndDate = DateTime.Now.AddMonths(1),
            IsGroupOffer = false,
            ProductImages = [new ProductImage { ImageUrl = imageId, IsMain = true }, new ProductImage { ImageUrl = imageId, IsMain = true }]
        };

        await unitOfWork.Products.AddAsync(product);
        await unitOfWork.CompleteAsync();
        return new {ProductName = product.Name, ProductImageUrl = imageId};
    }
}
