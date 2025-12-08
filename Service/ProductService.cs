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
namespace Service;

public class ProductService : IProductService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IImageService imageService;
    private readonly ILogger<ProductService> logger;

    public ProductService(IUnitOfWork unitOfWork, IImageService imageService, ILogger<ProductService> logger)
    {
        this.unitOfWork = unitOfWork;
        this.imageService = imageService;
        this.logger = logger;
    }

    // public Task<List<ProductUpdateDto>> ReadAllProducts()
    // {
    //     return unitOfWork.Products.GetAll();
    // }

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

        var result = AppResponse<List<ProductSubDetailsDto>>.Success(items);
        result.Pagination = PaginationInfo.Create(parameters.PageNumber, parameters.PageSize, totalCount);

        return result;
    }

    public async Task<AppResponse<List<ProductSubDetailsDto>>> ReadAllProducts(int pageNumber, int pageSize)
    {
        var query = unitOfWork.Products.GetAllQueryable();

        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = AppResponse<List<ProductSubDetailsDto>>.Success(products);
        result.Pagination = PaginationInfo.Create(pageNumber, pageSize, totalCount);

        return result;

    }

    // ==========================================================================================================================================================

    #region CreateProductWithImagesAsync
    public async Task<AppResponse<ProductSubDetailsDto>> CreateProductWithImagesAsync(ProductCreateDto dto, IEnumerable<Stream> imageStreams, string rootPath, CancellationToken ct = default)
    {
        // -----------------------------
        // Guard clauses
        // -----------------------------
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(imageStreams);
        if (string.IsNullOrWhiteSpace(rootPath))
            return AppResponse<ProductSubDetailsDto>.ValidationError("Root path is required");

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
            await SaveProductToDbAsync(product, ct);

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

            var imageId = await imageService.SaveImageAsync(stream, rootPath, ct);

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

        var product = dto.Adapt<Product>(); // using Mapster (أو AutoMapper لو حابب)
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
    private async Task SaveProductToDbAsync(Product product, CancellationToken ct)
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