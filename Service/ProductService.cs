using Contracts;
using Contracts.IServices;
using Entities.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Extensions;
using Service.Mappings;
using Shared.Enums;
using Shared.Models.Dtos.Product;
using Shared.Models.RequestFeatures;
using Shared.Responses;
using System.Net;
namespace Service;

public class ProductService : IProductService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMyImageService imageService;
    private readonly ILogger<ProductService> logger;

    public ProductService(IUnitOfWork unitOfWork, IMyImageService imageService, ILogger<ProductService> logger)
    {
        this.unitOfWork = unitOfWork;
        this.imageService = imageService;
        this.logger = logger;
    }

    public async Task<AppResponse<List<ProductSubDetailsDto>>> GetProductsAsync(ProductParameters parameters)
    {
        var query = unitOfWork.Products.Query()
                .ApplyFilters(parameters) // Filtering
                .ApplySort(parameters);// Sorting

        var totalCount = await query.CountAsync();
        var items = await query
            .ApplyPagination(parameters)  // Pagination
            .ProjectToType<ProductSubDetailsDto>()
            .ToListAsync();


        var pagination = PaginationInfo.Create(parameters.PageNumber, parameters.PageSize, totalCount);

        return AppResponse<List<ProductSubDetailsDto>>.Ok(items, pagination);

        //var result = AppResponse<List<ProductSubDetailsDto>>.Ok(items);
        //result.Pagination = PaginationInfo.Create(parameters.PageNumber, parameters.PageSize, totalCount);
        //return result;

    }

    // ==========================================================================================================================================================
    public async Task<int> CreateProductAsync(ProductCreateDto productDto, CancellationToken ct)
    {
        if (productDto == null)
            throw new ArgumentNullException(nameof(productDto));

        // Example: prevent duplicate barcode
        var exists = await unitOfWork.Products.ExistsByBarcodeAsync(productDto.Barcode, ct);
        if (exists)
            throw new InvalidOperationException("Product with the same barcode already exists.");

        var product = productDto.Adapt<Product>();
        product.CreatedAt = DateTime.UtcNow;
        product.IsActive = true;

        await unitOfWork.Products.AddAsync(product, ct);
        await unitOfWork.CompleteAsync(ct);
        return product.Id;
    }

    public async Task<bool> AddProductImagesAsync(IEnumerable<Stream> imageStreams, int productId, string rootPath, CancellationToken ct)
    {
        // Validate input images
        if (imageStreams == null || !imageStreams.Any())
            return false;

        // 1️ Ensure the product exists
        var product = await unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            throw new Exception("Product not found");

        var productImages = new List<ProductImage>();
        var savedImageNames = new List<string>();

        // Begin database transaction
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            int sortOrder = 1;

            foreach (var stream in imageStreams)
            {
                // 2️ Save image files to the file system
                var fileName = await imageService.SaveImageAsync(stream, rootPath, ct: ct);

                savedImageNames.Add(fileName);

                // 3️ Prepare database entity
                productImages.Add(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = fileName,
                    IsMain = sortOrder == 1, // First image is the main image
                    SortOrder = sortOrder,
                    CreatedAt = DateTime.UtcNow
                });

                sortOrder++;
            }

            // throw new InvalidOperationException("Rollback test");

            // 4️ Add image records to the database
            await unitOfWork.Products.AddProductImagesAsync(productImages);

            // 5️ Persist changes
            await unitOfWork.CompleteAsync(ct);

            // 6️ Commit the transaction
            await transaction.CommitAsync(ct);

            return true;
        }
        catch
        {
            // 7️ Roll back database changes
            await transaction.RollbackAsync(ct);

            // 8️ Remove any images saved to the file system
            await CleanupImagesAsync(savedImageNames, rootPath);
            throw;
        }
    }

    public async Task DeleteProductImageAsync(int productId, int imageId, string rootPath, CancellationToken ct)
    {
        var image = await unitOfWork.Products.GetProductImageByIdAsync(productId, imageId, ct);
        if (image == null)
            throw new Exception("Image not found.");

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Remove database record
            unitOfWork.Products.RemoveProductImage(image);
            await unitOfWork.CompleteAsync(ct);

            // Remove files from disk
            await imageService.RemoveImageAsync(image.ImageUrl, rootPath);

            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            await CleanupImagesAsync(new List<string> { image.ImageUrl }, rootPath);
            throw;
        }
    }

    public async Task<AppResponse> UpdateProductAsync(int productId, ProductUpdateDto dto, CancellationToken ct)
    {
        if (dto == null)
            return AppResponse.ValidationError("Invalid product data.");

        var product = await unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            return AppResponse.NotFound("Product not found.");

        // Optional: prevent duplicate barcode
        if (!string.Equals(product.Barcode, dto.Barcode, StringComparison.OrdinalIgnoreCase))
        {
            var barcodeExists = await unitOfWork.Products
                .ExistsByBarcodeAsync(dto.Barcode, ct);

            if (barcodeExists)
                return AppResponse.Conflict("Barcode already exists.");
        }

        // Map updated fields
        product.Name = dto.Name;
        product.NameEn = dto.NameEn;
        product.Description = dto.Description;
        product.DescriptionEn = dto.DescriptionEn;
        product.Barcode = dto.Barcode;
        product.Price = dto.Price;
        product.OldPrice = dto.OldPrice;
        product.IsAvailable = dto.IsAvailable;
        product.IsActive = dto.IsActive;
        product.Points = dto.Points;
        product.PromoDisc = dto.PromoDisc;
        product.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Products.Update(product);
        await unitOfWork.CompleteAsync(ct);

        return AppResponse.Ok(title: "Product Updated",
            detail: "Product data updated successfully."
        );
    }

    public async Task<AppResponse> SoftDeleteProductAsync(int productId, CancellationToken ct)
    {
        // 1️ Validate input
        if (productId <= 0)
            return AppResponse.Fail("Invalid product id");

        // 2️ Execute soft delete
        var deleted = await unitOfWork.Products.SoftDeleteAsync(productId, ct);

        if (!deleted)
            return AppResponse.Fail("Product not found or already deleted",AppErrorCode.NotFound);

        // 3️ Commit (in case UnitOfWork manages SaveChanges or other operations)
        await unitOfWork.CompleteAsync(ct);

        // 4️ Success response
        return AppResponse.Ok("Product soft deleted successfully");
    }

    public async Task<AppResponse> UpdateProductIsActiveAsync(int productId, bool isActive,CancellationToken ct)
    {
        if (productId <= 0)
            return AppResponse.Fail("Invalid product id",AppErrorCode.BadRequest);

        var updated = await unitOfWork.Products
            .UpdateIsActiveAsync(productId, isActive, ct);

        if (!updated)
            return AppResponse.Fail("Product not found or Marked as Deleted or already in the requested state",AppErrorCode.NotFound);

        await unitOfWork.CompleteAsync(ct);

        return AppResponse.Ok("Product status updated successfully");
    }


    #region CreateProductWithImagesAsync
    public async Task<AppResponse<ProductSubDetailsDto>> CreateProductWithImagesAsync(ProductCreateDto dto, IEnumerable<Stream> imageStreams, string rootPath, CancellationToken ct = default)
    {
        // -----------------------------
        // Guard clauses
        // -----------------------------
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(imageStreams);
        if (string.IsNullOrWhiteSpace(rootPath))
            return AppResponse<ProductSubDetailsDto>.Fail("Root path is required");

        ct.ThrowIfCancellationRequested();

        List<string>? imageIds = null;

        try
        {
            // -----------------------------
            // STEP 1: Save product images
            // -----------------------------
            imageIds = await SaveProductImagesAsync(imageStreams, dto.Name, rootPath, ct);

            // -----------------------------
            // STEP 2: Map DTO -> Entity
            // -----------------------------
            var product = MapToProduct(dto, imageIds);

            // -----------------------------
            // STEP 3: Save Product in DB
            // -----------------------------
            await SaveProductInfoToDbAsync(product, ct);

            // -----------------------------
            // STEP 4: Map back to DTO
            // -----------------------------
            var resultDto = product.ToSubDetailsDto();
            return AppResponse<ProductSubDetailsDto>.Created(resultDto);
        }
        catch (OperationCanceledException)
        {
            // Cleanup any saved images
            if (imageIds != null && imageIds.Any())
                await CleanupImagesAsync(imageIds, rootPath);

            logger.LogWarning("Operation was cancelled while creating product '{ProductName}'", dto.Name);
            return AppResponse<ProductSubDetailsDto>.Fail("Operation cancelled", AppErrorCode.None);
        }
        catch (Exception ex)
        {
            // Cleanup any saved images
            if (imageIds != null && imageIds.Any())
                await CleanupImagesAsync(imageIds, rootPath);

            logger.LogError(ex, "Unexpected error while creating product '{ProductName}'", dto.Name + ex.Message + ex.InnerException?.Message);
            return AppResponse<ProductSubDetailsDto>.InternalError("Failed to create product" + dto.Name + ex.Message + ex.InnerException?.Message);
        }
    }

    private async Task<List<string>> SaveProductImagesAsync(IEnumerable<Stream> imageStreams, string productName, string rootPath, CancellationToken ct)
    {
        var imageIds = new List<string>();

        logger.LogInformation("Start saving {ImageCount} image(s) for product '{ProductName}'",
            imageStreams.Count(), productName);

        foreach (var stream in imageStreams)
        {
            ct.ThrowIfCancellationRequested();

            if (stream == null)
            {
                logger.LogWarning("Null image stream detected for '{ProductName}'. Skipping...", productName);
                continue;
            }

            if (stream.CanSeek)
                stream.Position = 0;

            var imageId = await imageService.SaveImageAsync(stream, rootPath, ct: ct);

            if (string.IsNullOrWhiteSpace(imageId))
            {
                logger.LogError("Image service returned empty id for product '{ProductName}'", productName);

                // Cleanup any images already saved
                foreach (var savedId in imageIds)
                    await imageService.RemoveImageAsync(savedId, rootPath);

                throw new InvalidOperationException("Image service returned an empty identifier");
            }

            imageIds.Add(imageId);
            logger.LogInformation("Image saved successfully. ImageId={ImageId} for '{ProductName}'",
                imageId, productName);
        }

        if (imageIds.Count == 0)
        {
            throw new ArgumentException("At least one valid image must be provided", nameof(imageStreams));
        }

        return imageIds;
    }

    private Product MapToProduct(ProductCreateDto dto, List<string>? imageIds = null)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var product = dto.Adapt<Product>(); // using Mapster 
        product.CreatedAt = DateTime.UtcNow;

        // Attach images if provided
        if (imageIds != null && imageIds.Any())
        {
            product.ProductImages = imageIds
                .Select((id, index) => new ProductImage
                {
                    ImageUrl = id,
                    IsMain = index == 0 // أول صورة تعتبر الصورة الرئيسية
                })
                .ToList();
        }
        return product;
    }

    private async Task SaveProductInfoToDbAsync(Product product, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(product);

        ct.ThrowIfCancellationRequested();

        logger.LogInformation("Inserting product '{ProductName}' into DB", product.Name);

        await unitOfWork.Products.AddAsync(product, ct);
        await unitOfWork.CompleteAsync(ct);

        logger.LogInformation("Product saved successfully. ProductId={ProductId}", product.Id);
    }

    private async Task CleanupImagesAsync(IEnumerable<string> imageIds, string rootPath)
    {
        if (imageIds == null || !imageIds.Any())
            return;

        foreach (var id in imageIds)
        {
            try
            {
                await imageService.RemoveImageAsync(id, rootPath);
                logger.LogInformation("Cleaned up image {ImageId}", id);
            }
            catch (Exception ex)
            {
                // نسجل الخطأ لكن ما نوقفش العملية
                logger.LogError(ex, "Failed to cleanup image {ImageId}", id);
            }
        }
    }
    #endregion
}