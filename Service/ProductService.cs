using Contracts;
using Contracts.IServices;
using Entities.Models;
using FluentValidation;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Extensions;
using Service.Mappings;
using Service.Validators;
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
    private readonly IValidator<ProductUpdateDto> _updateValidator;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMyImageService imageService,
        ILogger<ProductService> logger,
        IValidator<ProductUpdateDto> updateValidator)
    {
        this.unitOfWork = unitOfWork;
        this.imageService = imageService;
        this.logger = logger;
        _updateValidator = updateValidator;
    }

    public async Task<AppResponse<int>> CreateProductAsync(ProductCreateDto dto, CancellationToken ct)
    {
        try
        {
            // 1) Resolve PharmacyId from JWT (do NOT trust dto)
            var pharmacyId = GetPharmacyIdOrDefault(); //

            // 2) Validate basic required fields
            var errors = ValidateCreateDto(dto);
            if (errors.Count > 0)
                return AppResponse<int>.ValidationErrors(errors);

            // 3) FK checks
            var categoryExists = await unitOfWork.Categories.ExistsByIdAsync(dto.CategoryId, ct);
            if (!categoryExists)
            {
                return AppResponse<int>.ValidationErrors(new Dictionary<string, string[]>
                {
                    ["CategoryId"] = ["Category does not exist"]
                });
            }

            // Brand optional: only if you have Brands repository/table
            // if (dto.BrandId.HasValue)
            // {
            //     var brandExists = await unitOfWork.Brands.ExistsByIdAsync(dto.BrandId.Value, ct);
            //     if (!brandExists)
            //         return AppResponse<int>.ValidationErrors(new Dictionary<string, string[]>
            //         {
            //             ["BrandId"] = ["Brand does not exist"]
            //         });
            // }

            // 4) Normalize optional code fields (trim, empty->null)
            var barcode = NormalizeCode(dto.Barcode);
            var internationalCode = NormalizeCode(dto.InternationalCode);
            var stockCode = NormalizeCode(dto.StockProductCode);

            // 5) Create entity
            var product = new Product
            {
                PharmacyId = pharmacyId,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,

                Barcode = barcode,
                InternationalCode = internationalCode,
                StockProductCode = stockCode,

                Name = dto.Name.Trim(),
                NameEn = dto.NameEn.Trim(),
                Slug = NormalizeNullable(dto.Slug),
                Description = dto.Description,
                DescriptionEn = dto.DescriptionEn,

                SearchKeywords = NormalizeNullable(dto.SearchKeywords),

                NormalizedName = NormalizeForSearch(dto.Name),
                NormalizedNameEn = NormalizeForSearch(dto.NameEn),

                DosageForm = NormalizeNullable(dto.DosageForm),
                Strength = NormalizeNullable(dto.Strength),
                PackSize = NormalizeNullable(dto.PackSize),
                Unit = NormalizeNullable(dto.Unit),

                RequiresPrescription = dto.RequiresPrescription,
                EarnPoints = dto.EarnPoints,
                HasExpiry = dto.HasExpiry,

                AgeRestricted = dto.AgeRestricted,
                MinAge = dto.MinAge,

                RequiresColdChain = dto.RequiresColdChain,
                ControlledSubstance = dto.ControlledSubstance,
                StorageConditions = NormalizeNullable(dto.StorageConditions),

                IsTaxable = dto.IsTaxable,
                VatRate = dto.VatRate,
                TaxCategoryCode = NormalizeNullable(dto.TaxCategoryCode),

                MinOrderQty = dto.MinOrderQty,
                MaxOrderQty = dto.MaxOrderQty,
                MaxPerDayQty = dto.MaxPerDayQty,

                IsReturnable = dto.IsReturnable,
                ReturnWindowDays = dto.ReturnWindowDays,

                AllowSplitSale = dto.AllowSplitSale,
                SplitLevel = dto.SplitLevel,

                WeightGrams = dto.WeightGrams,
                LengthMm = dto.LengthMm,
                WidthMm = dto.WidthMm,
                HeightMm = dto.HeightMm,

                TrackInventory = dto.TrackInventory,
                IsFeatured = dto.IsFeatured,
                IsActive = dto.IsActive,

                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.Products.AddAsync(product, ct);
            await unitOfWork.CompleteAsync(ct);

            // (Optional) Invalidate product caches if you have them
            // InvalidateProductCaches(pharmacyId);

            return AppResponse<int>.Ok(product.Id, "Product created successfully");
        }
        catch (DbUpdateException dbEx) when (IsUniqueCodeViolation(dbEx))
        {
            // Map unique index violations to friendly validation errors
            var mapped = MapUniqueViolationToErrors(dbEx);
            logger.LogWarning(dbEx, "CreateProductAsync unique constraint violation");
            return AppResponse<int>.ValidationErrors(mapped);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateProductAsync failed");
            return AppResponse<int>.InternalError("Failed to create product");
        }
    }

    public async Task<AppResponse<ProductDetailsDto>> GetProductByIdAsync(int productId, CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault();

            // Load product (no tracking is OK for read)
            var list = await unitOfWork.Products.GetAllAsync(
                selector: p => p.Adapt<ProductDetailsDto>(),
                criteria: p =>
                    p.Id == productId &&
                    p.PharmacyId == pharmacyId &&
                    p.DeletedAt == null,
                take: 1,
                asNoTracking: true,
                ct: ct);

            var product = list.FirstOrDefault();

            if (product is null)
                return AppResponse<ProductDetailsDto>.NotFound("Product not found");

            return AppResponse<ProductDetailsDto>.Ok(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetProductByIdAsync failed");
            return AppResponse<ProductDetailsDto>.InternalError("Failed to load product details");
        }
    }


    public async Task<AppResponse> UpdateProductAsync(int productId, ProductUpdateDto dto, CancellationToken ct)
    {
        try
        {
            // 1) Resolve pharmacyId (Multi-tenant)
            var pharmacyId = GetPharmacyIdOrDefault(); // استبدلها بالـ claims عندك

            // 2) FluentValidation for DTO rules
            var validation = await _updateValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return AppResponse.ValidationErrors(validation.ToFieldErrorsDictionary());

            // 3) Load product for update (tracking) + ensure same pharmacy + not deleted
            var product = await unitOfWork.Products.GetForUpdateAsync(productId, pharmacyId, ct);
            if (product is null)
                return AppResponse.NotFound("Product not found");

            // 4) FK checks
            var categoryExists = await unitOfWork.Categories.ExistsByIdAsync(dto.CategoryId, ct);
            if (!categoryExists)
            {
                return AppResponse.ValidationErrors(new Dictionary<string, string[]>
                {
                    ["CategoryId"] = ["Category does not exist"]
                });
            }

            // Brand optional (فعّل هذا فقط لو عندك Brands repo/table)
            // if (dto.BrandId.HasValue)
            // {
            //     var brandExists = await unitOfWork.Brands.ExistsByIdAsync(dto.BrandId.Value, ct);
            //     if (!brandExists)
            //     {
            //         return AppResponse.ValidationErrors(new Dictionary<string, string[]>
            //         {
            //             ["BrandId"] = ["Brand does not exist"]
            //         });
            //     }
            // }

            // 5) Concurrency: set original RowVersion
            unitOfWork.Products.SetRowVersionOriginalValue(product, dto.RowVersion);

            // 6) Normalize codes (trim, empty -> null)
            var barcode = NormalizeCode(dto.Barcode);
            var internationalCode = NormalizeCode(dto.InternationalCode);
            var stockCode = NormalizeCode(dto.StockProductCode);

            // 7) Apply updates
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;

            product.Barcode = barcode;
            product.InternationalCode = internationalCode;
            product.StockProductCode = stockCode;

            product.Name = dto.Name.Trim();
            product.NameEn = dto.NameEn.Trim();
            product.Slug = NormalizeNullable(dto.Slug);

            product.Description = dto.Description;
            product.DescriptionEn = dto.DescriptionEn;

            product.SearchKeywords = NormalizeNullable(dto.SearchKeywords);
            product.NormalizedName = NormalizeForSearch(dto.Name);
            product.NormalizedNameEn = NormalizeForSearch(dto.NameEn);

            product.DosageForm = NormalizeNullable(dto.DosageForm);
            product.Strength = NormalizeNullable(dto.Strength);
            product.PackSize = NormalizeNullable(dto.PackSize);
            product.Unit = NormalizeNullable(dto.Unit);

            product.RequiresPrescription = dto.RequiresPrescription;
            product.EarnPoints = dto.EarnPoints;
            product.HasExpiry = dto.HasExpiry;

            product.AgeRestricted = dto.AgeRestricted;
            product.MinAge = dto.MinAge;

            product.RequiresColdChain = dto.RequiresColdChain;
            product.ControlledSubstance = dto.ControlledSubstance;
            product.StorageConditions = NormalizeNullable(dto.StorageConditions);

            product.IsTaxable = dto.IsTaxable;
            product.VatRate = dto.VatRate;
            product.TaxCategoryCode = NormalizeNullable(dto.TaxCategoryCode);

            product.MinOrderQty = dto.MinOrderQty;
            product.MaxOrderQty = dto.MaxOrderQty;
            product.MaxPerDayQty = dto.MaxPerDayQty;

            product.IsReturnable = dto.IsReturnable;
            product.ReturnWindowDays = dto.ReturnWindowDays;

            product.AllowSplitSale = dto.AllowSplitSale;
            product.SplitLevel = dto.SplitLevel;

            product.WeightGrams = dto.WeightGrams;
            product.LengthMm = dto.LengthMm;
            product.WidthMm = dto.WidthMm;
            product.HeightMm = dto.HeightMm;

            product.TrackInventory = dto.TrackInventory;
            product.IsFeatured = dto.IsFeatured;
            product.IsActive = dto.IsActive;

            product.UpdatedAt = DateTime.UtcNow;

            // 8) Save
            await unitOfWork.CompleteAsync(ct);

            return AppResponse.Ok("Product updated successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            // 409 Conflict
            return AppResponse.Fail(
                "Concurrency conflict: the product was updated by another user. Please reload and try again.",
                AppErrorCode.Conflict);
        }
        catch (DbUpdateException dbEx) when (IsUniqueCodeViolation(dbEx))
        {
            logger.LogWarning(dbEx, "UpdateProductAsync unique constraint violation");
            return AppResponse.ValidationErrors(MapUniqueViolationToErrors(dbEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateProductAsync failed");
            return AppResponse.InternalError("Failed to update product");
        }
    }










    // ----------------- Helpers (place inside ProductService) -----------------

    private static Dictionary<string, string[]> ValidateCreateDto(ProductCreateDto dto)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (dto.CategoryId <= 0)
            errors["CategoryId"] = ["CategoryId is required"];

        if (string.IsNullOrWhiteSpace(dto.Name))
            errors["Name"] = ["Name is required"];

        if (string.IsNullOrWhiteSpace(dto.NameEn))
            errors["NameEn"] = ["NameEn is required"];

        // Mirrors CK_Products_Tax
        if (dto.VatRate < 0 || dto.VatRate > 100)
            errors["VatRate"] = ["VatRate must be between 0 and 100"];

        // Mirrors CK_Products_Age
        if (!dto.AgeRestricted && dto.MinAge is not null)
            errors["MinAge"] = ["MinAge must be null when AgeRestricted is false"];

        if (dto.AgeRestricted && (dto.MinAge is null || dto.MinAge < 0))
            errors["MinAge"] = ["MinAge is required and must be >= 0 when AgeRestricted is true"];

        // Mirrors CK_Products_OrderLimits
        if (dto.MinOrderQty <= 0)
            errors["MinOrderQty"] = ["MinOrderQty must be > 0"];

        if (dto.MaxOrderQty is not null && dto.MaxOrderQty < dto.MinOrderQty)
            errors["MaxOrderQty"] = ["MaxOrderQty must be >= MinOrderQty"];

        if (dto.MaxPerDayQty is not null && dto.MaxPerDayQty <= 0)
            errors["MaxPerDayQty"] = ["MaxPerDayQty must be > 0"];

        // Mirrors CK_Products_ReturnWindow
        if (!dto.IsReturnable && dto.ReturnWindowDays is not null)
            errors["ReturnWindowDays"] = ["ReturnWindowDays must be null when IsReturnable is false"];

        if (dto.IsReturnable && dto.ReturnWindowDays is not null && dto.ReturnWindowDays <= 0)
            errors["ReturnWindowDays"] = ["ReturnWindowDays must be > 0 when provided"];

        // Mirrors CK_Products_Dimensions
        if (dto.WeightGrams is not null && dto.WeightGrams < 0)
            errors["WeightGrams"] = ["WeightGrams must be >= 0"];

        if (dto.LengthMm is not null && dto.LengthMm < 0)
            errors["LengthMm"] = ["LengthMm must be >= 0"];

        if (dto.WidthMm is not null && dto.WidthMm < 0)
            errors["WidthMm"] = ["WidthMm must be >= 0"];

        if (dto.HeightMm is not null && dto.HeightMm < 0)
            errors["HeightMm"] = ["HeightMm must be >= 0"];

        // Mirrors CK_Products_SplitRules
        if (!dto.AllowSplitSale && dto.SplitLevel is not null)
            errors["SplitLevel"] = ["SplitLevel must be null when AllowSplitSale is false"];

        if (dto.AllowSplitSale && dto.SplitLevel is not (1 or 2))
            errors["SplitLevel"] = ["SplitLevel must be 1 or 2 when AllowSplitSale is true"];

        return errors;
    }

    private static string? NormalizeCode(string? value)
    {
        var v = value?.Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }

    private static string? NormalizeNullable(string? value)
    {
        var v = value?.Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }

    // Basic normalization for search (Arabic/English friendly)
    private static string? NormalizeForSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var v = value.Trim().ToLowerInvariant();

        // Collapse spaces
        v = string.Join(' ', v.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return v;
    }

    private static bool IsUniqueCodeViolation(DbUpdateException ex)
    {
        // SQL Server unique index violation numbers:
        // 2601: Cannot insert duplicate key row in object with unique index
        // 2627: Violation of UNIQUE KEY constraint
        if (ex.InnerException is SqlException sqlEx)
            return sqlEx.Number is 2601 or 2627;

        return false;
    }

    private static Dictionary<string, string[]> MapUniqueViolationToErrors(DbUpdateException ex)
    {
        // Best-effort mapping based on index/constraint name in message
        var msg = ex.InnerException?.Message ?? ex.Message;

        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (msg.Contains("UX_Products_Pharmacy_Barcode", StringComparison.OrdinalIgnoreCase))
            errors["Barcode"] = ["Barcode already exists for this pharmacy"];
        else if (msg.Contains("UX_Products_Pharmacy_InternationalCode", StringComparison.OrdinalIgnoreCase))
            errors["InternationalCode"] = ["InternationalCode already exists for this pharmacy"];
        else if (msg.Contains("UX_Products_Pharmacy_StockProductCode", StringComparison.OrdinalIgnoreCase))
            errors["StockProductCode"] = ["StockProductCode already exists for this pharmacy"];
        else
            errors["Codes"] = ["A unique code already exists for this pharmacy"];

        return errors;
    }

    private int GetPharmacyIdOrDefault()
    {
        // TODO: Replace this with your actual implementation
       
        // return currentUser.PharmacyId;
        return 1;
    }


    #region CreateProductWithImagesAsync
    private Product MapToProduct(ProductCreateDto dto, List<string>? imageIds = null)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var product = dto.Adapt<Product>(); // using Mapster 
        product.CreatedAt = DateTime.UtcNow;

        // Attach images if provided
        //if (imageIds != null && imageIds.Any())
        //{
        //    product.ProductImage = imageIds
        //        .Select((id, index) => new ProductImage
        //        {
        //            ImageUrl = id,
        //            IsPrimary = index == 0 // أول صورة تعتبر الصورة الرئيسية
        //        })
        //        .ToList();
        //}
        return product;
    }
#endregion
}