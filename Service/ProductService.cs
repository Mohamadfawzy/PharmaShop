using Contracts;
using Contracts.IServices;
using Entities.Models;
using FluentValidation;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Validators;
using Shared.Enums;
using Shared.Models.Dtos.Product;
using Shared.Models.Dtos.Product.Units;
using Shared.Models.RequestFeatures;
using Shared.Responses;
using System.Globalization;
namespace Service;

public class ProductService : IProductService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMyImageService imageService;
    private readonly ILogger<ProductService> logger;
    private readonly IValidator<ProductUpdateDto> _updateValidator;
    private readonly IValidator<ProductCreateDto> createValidator;
    private readonly ICurrentUserService currentUser;
    private readonly IValidator<ProductUnitCreateDto> productUnitCreateValidator;
    private readonly IValidator<ReceiveStockDto> receiveStockValidator;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMyImageService imageService,
        ILogger<ProductService> logger,
        IValidator<ProductUpdateDto> updateValidator,
        IValidator<ProductCreateDto> createValidator,
        ICurrentUserService currentUser,
        IValidator<ProductUnitCreateDto> productUnitCreateValidator,
        IValidator<ReceiveStockDto> receiveStockValidator)
    {
        this.unitOfWork = unitOfWork;
        this.imageService = imageService;
        this.logger = logger;
        _updateValidator = updateValidator;
        this.createValidator = createValidator;
        this.currentUser = currentUser;
        this.productUnitCreateValidator = productUnitCreateValidator;
        this.receiveStockValidator = receiveStockValidator;
    }

    //public async Task<AppResponse<int>> CreateProductAsync(ProductCreateDto dto, CancellationToken ct)
    //{
    //    try
    //    {
    //        // 1) Resolve PharmacyId from JWT (do NOT trust dto)
    //        var pharmacyId = GetPharmacyIdOrDefault(); //

    //        // 2) Validate basic required fields
    //        var errors = ValidateCreateDto(dto);
    //        if (errors.Count > 0)
    //            return AppResponse<int>.ValidationErrors(errors);

    //        // 3) FK checks
    //        var categoryExists = await unitOfWork.Categories.ExistsByIdAsync(dto.CategoryId, ct);
    //        if (!categoryExists)
    //        {
    //            return AppResponse<int>.ValidationErrors(new Dictionary<string, string[]>
    //            {
    //                ["CategoryId"] = ["Category does not exist"]
    //            });
    //        }

    //        // Brand optional: only if you have Brands repository/table
    //        // if (dto.BrandId.HasValue)
    //        // {
    //        //     var brandExists = await unitOfWork.Brands.ExistsByIdAsync(dto.BrandId.Value, ct);
    //        //     if (!brandExists)
    //        //         return AppResponse<int>.ValidationErrors(new Dictionary<string, string[]>
    //        //         {
    //        //             ["BrandId"] = ["Brand does not exist"]
    //        //         });
    //        // }

    //        // 4) Normalize optional code fields (trim, empty->null)
    //        var barcode = NormalizeCode(dto.Barcode);
    //        var internationalCode = NormalizeCode(dto.InternationalCode);
    //        var stockCode = NormalizeCode(dto.StockProductCode);

    //        // 5) Create entity
    //        var product = new Product
    //        {
    //            PharmacyId = pharmacyId,
    //            CategoryId = dto.CategoryId,
    //            BrandId = dto.BrandId,

    //            Barcode = barcode,
    //            InternationalCode = internationalCode,
    //            StockProductCode = stockCode,

    //            Name = dto.Name.Trim(),
    //            NameEn = dto.NameEn.Trim(),
    //            Slug = NormalizeNullable(dto.Slug),
    //            Description = dto.Description,
    //            DescriptionEn = dto.DescriptionEn,

    //            SearchKeywords = NormalizeNullable(dto.SearchKeywords),

    //            NormalizedName = NormalizeForSearch(dto.Name),
    //            NormalizedNameEn = NormalizeForSearch(dto.NameEn),

    //            DosageForm = NormalizeNullable(dto.DosageForm),
    //            Strength = NormalizeNullable(dto.Strength),
    //            PackSize = NormalizeNullable(dto.PackSize),
    //            Unit = NormalizeNullable(dto.Unit),

    //            RequiresPrescription = dto.RequiresPrescription,
    //            EarnPoints = dto.EarnPoints,
    //            HasExpiry = dto.HasExpiry,

    //            AgeRestricted = dto.AgeRestricted,
    //            MinAge = dto.MinAge,

    //            RequiresColdChain = dto.RequiresColdChain,
    //            ControlledSubstance = dto.ControlledSubstance,
    //            StorageConditions = NormalizeNullable(dto.StorageConditions),

    //            IsTaxable = dto.IsTaxable,
    //            VatRate = dto.VatRate,
    //            TaxCategoryCode = NormalizeNullable(dto.TaxCategoryCode),

    //            MinOrderQty = dto.MinOrderQty,
    //            MaxOrderQty = dto.MaxOrderQty,
    //            MaxPerDayQty = dto.MaxPerDayQty,

    //            IsReturnable = dto.IsReturnable,
    //            ReturnWindowDays = dto.ReturnWindowDays,

    //            AllowSplitSale = dto.AllowSplitSale,
    //            SplitLevel = dto.SplitLevel,

    //            WeightGrams = dto.WeightGrams,
    //            LengthMm = dto.LengthMm,
    //            WidthMm = dto.WidthMm,
    //            HeightMm = dto.HeightMm,

    //            TrackInventory = dto.TrackInventory,
    //            IsFeatured = dto.IsFeatured,
    //            IsActive = dto.IsActive,

    //            CreatedAt = DateTime.UtcNow
    //        };

    //        await unitOfWork.Products.AddAsync(product, ct);
    //        await unitOfWork.CompleteAsync(ct);

    //        // (Optional) Invalidate product caches if you have them
    //        // InvalidateProductCaches(pharmacyId);

    //        return AppResponse<int>.Ok(product.Id, "Product created successfully");
    //    }
    //    catch (DbUpdateException dbEx) when (IsUniqueCodeViolation(dbEx))
    //    {
    //        // Map unique index violations to friendly validation errors
    //        var mapped = MapUniqueViolationToErrors(dbEx);
    //        logger.LogWarning(dbEx, "CreateProductAsync unique constraint violation");
    //        return AppResponse<int>.ValidationErrors(mapped);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "CreateProductAsync failed");
    //        return AppResponse<int>.InternalError("Failed to create product");
    //    }
    //}


    public async Task<AppResponse<int>> CreateProductAsync(ProductCreateDto dto, CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault(); // استبدله بالـ claims عندك

            // 1) FluentValidation
            var validation = await createValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return AppResponse<int>.ValidationErrors(validation.ToFieldErrorsDictionary());

            // 2) FK checks
            var categoryExists = await unitOfWork.Categories.ExistsByIdAsync(dto.CategoryId, ct);
            if (!categoryExists)
            {
                return AppResponse<int>.ValidationErrors(new Dictionary<string, string[]>
                {
                    ["CategoryId"] = ["Category does not exist"]
                });
            }

            // Brand optional (فعّله إذا عندك Brands repo)
            // if (dto.BrandId.HasValue)
            // {
            //     var brandExists = await unitOfWork.Brands.ExistsByIdAsync(dto.BrandId.Value, ct);
            //     if (!brandExists)
            //     {
            //         return AppResponse<int>.ValidationErrors(new Dictionary<string, string[]>
            //         {
            //             ["BrandId"] = ["Brand does not exist"]
            //         });
            //     }
            // }

            // 3) Mapster: dto -> entity
            var product = dto.Adapt<Entities.Models.Product>();

            // 4) Multi-tenant + audit
            product.PharmacyId = pharmacyId;
            product.CreatedAt = DateTime.UtcNow;

            // 5) Normalize codes (trim, empty => null)
            product.Barcode = NormalizeCode(dto.Barcode);
            product.InternationalCode = NormalizeCode(dto.InternationalCode);
            product.StockProductCode = NormalizeCode(dto.StockProductCode);

            // 6) Normalize text fields
            product.Name = dto.Name.Trim();
            product.NameEn = dto.NameEn.Trim();
            product.Slug = NormalizeNullable(dto.Slug);
            product.SearchKeywords = NormalizeNullable(dto.SearchKeywords);

            product.DosageForm = NormalizeNullable(dto.DosageForm);
            product.Strength = NormalizeNullable(dto.Strength);
            product.PackSize = NormalizeNullable(dto.PackSize);
            product.Unit = NormalizeNullable(dto.Unit);

            product.StorageConditions = NormalizeNullable(dto.StorageConditions);
            product.TaxCategoryCode = NormalizeNullable(dto.TaxCategoryCode);

            // 7) Search normalization
            product.NormalizedName = NormalizeForSearch(product.Name);
            product.NormalizedNameEn = NormalizeForSearch(product.NameEn);

            // 8) Save
            await unitOfWork.Products.AddAsync(product, ct);
            await unitOfWork.CompleteAsync(ct);

            return AppResponse<int>.Ok(product.Id, "Product created successfully");
        }
        catch (DbUpdateException dbEx) when (IsUniqueCodeViolation(dbEx))
        {
            logger.LogWarning(dbEx, "CreateProductAsync unique constraint violation");
            return AppResponse<int>.ValidationErrors(MapUniqueViolationToErrors(dbEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateProductAsync failed");
            return AppResponse<int>.InternalError("Failed to create product");
        }
    }

    public async Task<AppResponse<ProductDetailsDto>> GetProductByIdAsync(int productId,bool includeDeleted,CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault();

            var list = await unitOfWork.Products.GetAllAsync(
                selector: p => p.Adapt<ProductDetailsDto>(),
                criteria: p =>
                    p.Id == productId &&
                    p.PharmacyId == pharmacyId &&
                    (includeDeleted || p.DeletedAt == null),
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
    public async Task<AppResponse<List<ProductDetailsDto>>> GetDeletedProductsAsync(int skip, int take, CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault();

            var items = await unitOfWork.Products.GetAllAsync(
                selector: p => p.Adapt<ProductDetailsDto>(),
                criteria: p => p.PharmacyId == pharmacyId && p.DeletedAt != null,
                orderBy: q => q.OrderByDescending(x => x.DeletedAt),
                skip: skip,
                take: take,
                asNoTracking: true,
                ct: ct);

            return AppResponse<List<ProductDetailsDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetDeletedProductsAsync failed");
            return AppResponse<List<ProductDetailsDto>>.InternalError("Failed to load deleted products");
        }
    }

    public async Task<AppResponse> UpdateProductAsync(int productId, ProductUpdateDto dto, CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault(); // استبدلها بالـ claims
            var userId = currentUser.UserId ?? "system";

            // 1) Validate DTO (FluentValidation)
            var validation = await _updateValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
                return AppResponse.ValidationErrors(validation.ToFieldErrorsDictionary());

            // 2) Load product (Tracking) + tenant + not deleted
            var product = await unitOfWork.Products.FirstOrDefaultAsync(
                criteria: p => p.Id == productId && p.PharmacyId == pharmacyId && p.DeletedAt == null,
                asNoTracking: false,
                ct: ct);

            if (product is null)
                return AppResponse.NotFound("Product not found");

            // 3) FK checks (Category)
            var categoryExists = await unitOfWork.Categories.AnyAsync(
                criteria: c => c.Id == dto.CategoryId && !c.IsDeleted,
                ct: ct);

            if (!categoryExists)
            {
                return AppResponse.ValidationErrors(new Dictionary<string, string[]>
                {
                    ["CategoryId"] = ["Category does not exist"]
                });
            }

            // 4) Build audit logs (before applying changes)
            var operationId = Guid.NewGuid();
            var reason = dto.AuditReason; // اختياري (لو ضفته في dto)
            var auditLogs = BuildProductUpdateAuditLogs(product, dto, pharmacyId, userId, operationId, reason);

            if (auditLogs.Count == 0)
                return AppResponse.Ok("No changes detected");

            // 5) Apply updates
            ApplyProductUpdate(product, dto);
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = userId;

            // 6) Transaction: Product update + Audit logs insert as one atomic op
            await using var tx = await unitOfWork.BeginTransactionAsync(ct);

            // 6.1 Concurrency: set original RowVersion (inside transaction, before save)
            // ✅ الأفضل أن يكون لديك helper في repo/uow. إن لم يوجد، استخدم DbContext داخل UoW
            // هنا استخدمنا method داخل UoW (أضفها لو غير موجودة) - انظر الملاحظة أسفل.
            unitOfWork.SetOriginalRowVersion(product, dto.RowVersion);

            unitOfWork.Products.Update(product);
            await unitOfWork.ProductAuditLogs.AddRangeAsync(auditLogs, ct);

            await unitOfWork.CompleteAsync(ct);
            await tx.CommitAsync(ct);

            return AppResponse.Ok("Product updated successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            return AppResponse.Fail(
                "Concurrency conflict: the product was updated by another user. Please reload and try again.",
                AppErrorCode.Conflict);
        }
        catch (DbUpdateException dbEx) when (IsUniqueCodeViolation(dbEx))
        {
            logger.LogWarning(dbEx, "UpdateProductAsync unique constraint violation for ProductId={ProductId}", productId);
            return AppResponse.ValidationErrors(MapUniqueViolationToErrors(dbEx));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateProductAsync failed for ProductId={ProductId}", productId);
            return AppResponse.InternalError("Failed to update product");
        }
    }

    public async Task<AppResponse> SetActiveAsync(int productId,bool isActive,ProductStateChangeDto dto,CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault();
            var userId = currentUser.UserId ?? "system";
            var operationId = Guid.NewGuid();

            // 1) Load product (Tracking) + tenant + not deleted
            var product = await unitOfWork.Products.FirstOrDefaultAsync(
                criteria: p => p.Id == productId && p.PharmacyId == pharmacyId && p.DeletedAt == null,
                asNoTracking: false,
                ct: ct);

            if (product is null)
                return AppResponse.NotFound("Product not found");

            // 2) No changes
            if (product.IsActive == isActive)
                return AppResponse.Ok("No changes detected");

            // 3) Build audit logs (field-level)
            var logs = new List<ProductAuditLog>
        {
            new()
            {
                PharmacyId = pharmacyId,
                ProductId = product.Id,
                OperationId = operationId,
                ChangeType = isActive ? "Activate" : "Deactivate", 
                FieldName = nameof(Product.IsActive),
                OldValue = product.IsActive ? "true" : "false",
                NewValue = isActive ? "true" : "false",
                ChangedBy = userId,
                ChangeDate = DateTime.UtcNow,
                Reason = string.IsNullOrWhiteSpace(dto.AuditReason) ? null : dto.AuditReason.Trim()
            }
        };

            // 4) Apply change
            product.IsActive = isActive;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = userId;

            // 5) Transaction: concurrency + update + audit
            await using var tx = await unitOfWork.BeginTransactionAsync(ct);

            unitOfWork.SetOriginalRowVersion(product, dto.RowVersion);

            unitOfWork.Products.Update(product);
            await unitOfWork.ProductAuditLogs.AddRangeAsync(logs, ct);

            await unitOfWork.CompleteAsync(ct);
            await tx.CommitAsync(ct);

            return AppResponse.Ok(isActive ? "Product activated successfully" : "Product deactivated successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            return AppResponse.Fail(
                "Concurrency conflict: the product was updated by another user. Please reload and try again.",
                AppErrorCode.Conflict);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SetActiveAsync failed for ProductId={ProductId}", productId);
            return AppResponse.InternalError("Failed to update product status");
        }
    }

    public async Task<AppResponse> SetDeletedAsync(int productId,bool isDeleted,ProductStateChangeDto dto,CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault();
            var userId = currentUser.UserId ?? "system";
            var operationId = Guid.NewGuid();
            var reason = string.IsNullOrWhiteSpace(dto.AuditReason) ? null : dto.AuditReason.Trim();

            // 1) Load product (Tracking) + tenant
            var product = await unitOfWork.Products.FirstOrDefaultAsync(
                criteria: p => p.Id == productId && p.PharmacyId == pharmacyId,
                asNoTracking: false,
                ct: ct);

            if (product is null)
                return AppResponse.NotFound("Product not found");

            var currentlyDeleted = product.DeletedAt is not null;

            // 2) No changes
            if (currentlyDeleted == isDeleted)
                return AppResponse.Ok("No changes detected");

            var now = DateTime.UtcNow;

            // 3) Build audit logs (field-level)
            var changeType = isDeleted ? "SoftDelete" : "Restore";

            var logs = new List<ProductAuditLog>();

            // DeletedAt
            logs.Add(new ProductAuditLog
            {
                PharmacyId = pharmacyId,
                ProductId = product.Id,
                OperationId = operationId,
                ChangeType = changeType,
                FieldName = nameof(Product.DeletedAt),
                OldValue = product.DeletedAt?.ToString("O"),
                NewValue = isDeleted ? now.ToString("O") : null,
                ChangedBy = userId,
                ChangeDate = now,
                Reason = reason
            });

            // DeletedBy
            logs.Add(new ProductAuditLog
            {
                PharmacyId = pharmacyId,
                ProductId = product.Id,
                OperationId = operationId,
                ChangeType = changeType,
                FieldName = nameof(Product.DeletedBy),
                OldValue = product.DeletedBy,
                NewValue = isDeleted ? userId : null,
                ChangedBy = userId,
                ChangeDate = now,
                Reason = reason
            });

            // (اختياري) إدارة IsActive:
            // - عند SoftDelete: نخليه false
            // - عند Restore: نخليه true (أو تتركه كما هو حسب سياستك)
            if (isDeleted)
            {
                if (product.IsActive)
                {
                    logs.Add(new ProductAuditLog
                    {
                        PharmacyId = pharmacyId,
                        ProductId = product.Id,
                        OperationId = operationId,
                        ChangeType = changeType,
                        FieldName = nameof(Product.IsActive),
                        OldValue = "true",
                        NewValue = "false",
                        ChangedBy = userId,
                        ChangeDate = now,
                        Reason = reason
                    });
                }
            }
            else
            {
                // Restore policy:
                // اختيار 1 (افتراضي): نرجعه نشط
                // اختيار 2: نترك IsActive كما هو
                // هنا نفترض اختيار 1 لأنه منطقي في أغلب الحالات
                if (!product.IsActive)
                {
                    logs.Add(new ProductAuditLog
                    {
                        PharmacyId = pharmacyId,
                        ProductId = product.Id,
                        OperationId = operationId,
                        ChangeType = changeType,
                        FieldName = nameof(Product.IsActive),
                        OldValue = "false",
                        NewValue = "true",
                        ChangedBy = userId,
                        ChangeDate = now,
                        Reason = reason
                    });
                }
            }

            // 4) Apply state change
            if (isDeleted)
            {
                product.DeletedAt = now;
                product.DeletedBy = userId;
                product.IsActive = false; // policy
            }
            else
            {
                product.DeletedAt = null;
                product.DeletedBy = null;
                product.IsActive = true; // policy
            }

            product.UpdatedAt = now;
            product.UpdatedBy = userId;

            // 5) Transaction + Concurrency
            await using var tx = await unitOfWork.BeginTransactionAsync(ct);

            unitOfWork.SetOriginalRowVersion(product, dto.RowVersion);

            unitOfWork.Products.Update(product);
            await unitOfWork.ProductAuditLogs.AddRangeAsync(logs, ct);

            await unitOfWork.CompleteAsync(ct);
            await tx.CommitAsync(ct);

            return AppResponse.Ok(isDeleted ? "Product deleted successfully" : "Product restored successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            return AppResponse.Fail(
                "Concurrency conflict: the product was updated by another user. Please reload and try again.",
                AppErrorCode.Conflict);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SetDeletedAsync failed for ProductId={ProductId}, isDeleted={IsDeleted}", productId, isDeleted);
            return AppResponse.InternalError("Failed to update product delete status");
        }
    }

    public async Task<AppResponse<List<ProductListItemDto>>> GetProductsAsync(ProductListQueryDto query, CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault();

            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 20 : (query.PageSize > 200 ? 200 : query.PageSize);

            var result = await unitOfWork.Products.SearchAsync(pharmacyId, query, ct);

            var response = AppResponse<List<ProductListItemDto>>.Ok(result.Items);
            response.Pagination = result.TotalCount == 0
                ? PaginationInfo.Empty(page, pageSize)
                : PaginationInfo.Create(page, pageSize, result.TotalCount);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetProductsAsync failed");
            return AppResponse<List<ProductListItemDto>>.InternalError("Failed to load products");
        }
    }

    //----------------------------------------------
    // Units
    //----------------------------------------------
    public async Task<AppResponse<ProductUnitCreatedDto>> AddProductUnitAsync(
    int productId,
    ProductUnitCreateDto dto,
    CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault();

            // 1) validate (FluentValidation)
            var validation = await productUnitCreateValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid)
            {
                var fieldErrors = validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

                return AppResponse<ProductUnitCreatedDto>.ValidationErrors(fieldErrors);
            }

            // 2) ensure product exists and belongs to pharmacy (and not deleted)
            var productExists = await unitOfWork.Products.AnyAsync(
                p => p.Id == productId && p.PharmacyId == pharmacyId && p.DeletedAt == null,
                ct);

            if (!productExists)
                return AppResponse<ProductUnitCreatedDto>.NotFound("Product not found");

            // 3) ensure Unit exists
            var unitExists = await unitOfWork.Units.AnyAsync(u => u.Id == dto.UnitId, ct);
            if (!unitExists)
                return AppResponse<ProductUnitCreatedDto>.ValidationErrors(new Dictionary<string, string[]>
                {
                    ["UnitId"] = new[] { "Unit does not exist" }
                });

            // 4) optional BaseUnit exists
            if (dto.BaseUnitId.HasValue)
            {
                var baseUnitExists = await unitOfWork.Units.AnyAsync(u => u.Id == dto.BaseUnitId.Value, ct);
                if (!baseUnitExists)
                    return AppResponse<ProductUnitCreatedDto>.ValidationErrors(new Dictionary<string, string[]>
                    {
                        ["BaseUnitId"] = new[] { "BaseUnit does not exist" }
                    });
            }

            // 5) optional ParentProductUnit validation (same product + same pharmacy + not deleted)
            if (dto.ParentProductUnitId.HasValue)
            {
                var parentOk = await unitOfWork.ProductUnits.AnyAsync(
                    pu => pu.Id == dto.ParentProductUnitId.Value
                          && pu.ProductId == productId
                          && pu.PharmacyId == pharmacyId
                          && pu.DeletedAt == null,
                    ct);

                if (!parentOk)
                    return AppResponse<ProductUnitCreatedDto>.ValidationErrors(new Dictionary<string, string[]>
                    {
                        ["ParentProductUnitId"] = new[] { "Parent product unit not found for this product" }
                    });
            }

            // 6) prevent duplicate (Product, Unit) in same pharmacy (non-deleted)
            var duplicate = await unitOfWork.ProductUnits.AnyAsync(
                pu => pu.PharmacyId == pharmacyId
                      && pu.ProductId == productId
                      && pu.UnitId == dto.UnitId
                      && pu.DeletedAt == null,
                ct);

            if (duplicate)
                return AppResponse<ProductUnitCreatedDto>.ValidationErrors(new Dictionary<string, string[]>
                {
                    ["UnitId"] = new[] { "This unit already exists for this product" }
                });

            // 7) optional uniqueness for SKU/UnitCode inside pharmacy
            if (!string.IsNullOrWhiteSpace(dto.SKU))
            {
                var skuUsed = await unitOfWork.ProductUnits.AnyAsync(
                    pu => pu.PharmacyId == pharmacyId && pu.SKU == dto.SKU && pu.DeletedAt == null,
                    ct);

                if (skuUsed)
                    return AppResponse<ProductUnitCreatedDto>.ValidationErrors(new Dictionary<string, string[]>
                    {
                        ["SKU"] = new[] { "SKU already used in this pharmacy" }
                    });
            }

            if (!string.IsNullOrWhiteSpace(dto.UnitCode))
            {
                var codeUsed = await unitOfWork.ProductUnits.AnyAsync(
                    pu => pu.PharmacyId == pharmacyId && pu.UnitCode == dto.UnitCode && pu.DeletedAt == null,
                    ct);

                if (codeUsed)
                    return AppResponse<ProductUnitCreatedDto>.ValidationErrors(new Dictionary<string, string[]>
                    {
                        ["UnitCode"] = new[] { "UnitCode already used in this pharmacy" }
                    });
            }

            // 8) Transaction: if IsPrimary => unset old primary first
            await using var tx = await unitOfWork.BeginTransactionAsync(ct);

            if (dto.IsPrimary)
            {
                // find current primary (if any)
                var currentPrimary = await unitOfWork.ProductUnits.SingleOrDefaultAsync(
                    pu => pu.ProductId == productId
                          && pu.PharmacyId == pharmacyId
                          && pu.IsPrimary
                          && pu.DeletedAt == null,
                    asNoTracking: false,
                    ct: ct);

                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                    currentPrimary.UpdatedAt = DateTime.UtcNow;
                    await unitOfWork.ProductUnits.UpdateAsync(currentPrimary, ct);
                }
            }

            // 9) create new unit
            var entity = new ProductUnit
            {
                PharmacyId = pharmacyId,
                ProductId = productId,

                UnitId = dto.UnitId,
                SortOrder = dto.SortOrder,

                UnitCode = string.IsNullOrWhiteSpace(dto.UnitCode) ? null : dto.UnitCode.Trim(),
                SKU = string.IsNullOrWhiteSpace(dto.SKU) ? null : dto.SKU.Trim(),

                ParentProductUnitId = dto.ParentProductUnitId,
                UnitsPerParent = dto.UnitsPerParent,

                BaseUnitId = dto.BaseUnitId,
                BaseQuantity = dto.BaseQuantity,

                CurrencyCode = dto.CurrencyCode.Trim().ToUpperInvariant(),
                CostPrice = dto.CostPrice,
                ListPrice = dto.ListPrice,
                PriceUpdatedAt = DateTime.UtcNow,

                IsPrimary = dto.IsPrimary,
                IsActive = dto.IsActive,

                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.ProductUnits.AddAsync(entity, ct);
            await unitOfWork.CompleteAsync(ct);
            await tx.CommitAsync(ct);

            return AppResponse<ProductUnitCreatedDto>.Ok(new ProductUnitCreatedDto
            {
                ProductUnitId = entity.Id,
                ProductId = entity.ProductId,
                IsPrimary = entity.IsPrimary
            }, "Product unit added successfully");
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "AddProductUnitAsync failed (DbUpdateException) ProductId={ProductId}", productId);
            // Unique index violations could happen concurrently -> friendly message
            return AppResponse<ProductUnitCreatedDto>.Conflict("Failed to add product unit due to a duplicate constraint");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AddProductUnitAsync failed ProductId={ProductId}", productId);
            return AppResponse<ProductUnitCreatedDto>.InternalError("Failed to add product unit");
        }
    }


    //----------------------------------------------
    // ProductBatches 
    //----------------------------------------------

    public async Task<AppResponse<OpenBoxResultDto>> OpenBoxAsync(
        int productId,
        OpenBoxDto dto,
        CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault();

            // minimal validation here (keep it simple)
            if (dto is null)
                return AppResponse<OpenBoxResultDto>.ValidationError("Body is required");

            // product must exist & allow split sale
            var product = await unitOfWork.Products.FirstOrDefaultAsync(
                p => p.Id == productId
                     && p.PharmacyId == pharmacyId
                     && p.DeletedAt == null,
                asNoTracking: true,
                ct: ct);

            if (product is null)
                return AppResponse<OpenBoxResultDto>.NotFound("Product not found");

            if (!product.AllowSplitSale)
                return AppResponse<OpenBoxResultDto>.ValidationError("This product does not allow split sale / conversion.");

            await using var tx = await unitOfWork.BeginTransactionAsync(ct);

            var (result, fieldErrors, errorMessage) =
                await unitOfWork.Products.ConvertUnitsAsync(pharmacyId, productId, dto, ct);

            if (fieldErrors is not null)
                return AppResponse<OpenBoxResultDto>.ValidationErrors(fieldErrors);

            if (errorMessage is not null)
                return AppResponse<OpenBoxResultDto>.ValidationError(errorMessage);

            await unitOfWork.CompleteAsync(ct);
            await tx.CommitAsync(ct);

            return AppResponse<OpenBoxResultDto>.Ok(result!, "Conversion completed successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            return AppResponse<OpenBoxResultDto>.Conflict("Stock changed by another request. Please retry.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OpenBoxAsync failed ProductId={ProductId}", productId);
            return AppResponse<OpenBoxResultDto>.InternalError("Failed to open/convert units");
        }
    }

    public async Task<AppResponse<ReceiveStockResultDto>> ReceiveStockAsync(
       int productId,
       ReceiveStockDto dto,
       CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault();

            if (dto is null)
                return AppResponse<ReceiveStockResultDto>.ValidationError("Body is required");

            // 1) FluentValidation (shape validation)
            var vr = await receiveStockValidator.ValidateAsync(dto, ct);
            if (!vr.IsValid)
            {
                var fieldErrors = vr.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

                return AppResponse<ReceiveStockResultDto>.ValidationErrors(fieldErrors);
            }

            // 2) Transaction
            await using var tx = await unitOfWork.BeginTransactionAsync(ct);

            // 3) Business logic + DB checks happen in the repository
            var (result, fieldErrorsFromRepo, errorMessage) =
                await unitOfWork.Products.ReceiveStockAsync(pharmacyId, productId, dto, ct);

            if (fieldErrorsFromRepo is not null)
                return AppResponse<ReceiveStockResultDto>.ValidationErrors(fieldErrorsFromRepo);

            if (errorMessage is not null)
                return AppResponse<ReceiveStockResultDto>.ValidationError(errorMessage);

            await unitOfWork.CompleteAsync(ct);
            await tx.CommitAsync(ct);

            return AppResponse<ReceiveStockResultDto>.Ok(result!, "Stock received successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            return AppResponse<ReceiveStockResultDto>.Conflict("Stock changed by another request. Please retry.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ReceiveStockAsync failed ProductId={ProductId}", productId);
            return AppResponse<ReceiveStockResultDto>.InternalError("Failed to receive stock");
        }
    }

    private static List<ProductAuditLog> BuildProductUpdateAuditLogs(Product product,ProductUpdateDto dto,int pharmacyId,string userId,Guid operationId,string? reason)
    {
        var logs = new List<ProductAuditLog>();

        void Add(string field, object? oldVal, object? newVal)
        {
            logs.Add(new ProductAuditLog
            {
                PharmacyId = pharmacyId,
                ProductId = product.Id,
                OperationId = operationId,
                ChangeType = "Update",
                FieldName = field,
                OldValue = ToAuditString(oldVal),
                NewValue = ToAuditString(newVal),
                ChangedBy = userId,
                ChangeDate = DateTime.UtcNow,
                Reason = reason
            });
        }

        void Compare<T>(string field, T? oldVal, T? newVal)
        {
            if (!EqualityComparer<T?>.Default.Equals(oldVal, newVal))
                Add(field, oldVal, newVal);
        }

        // Normalize inputs for accurate diff
        var barcode = NormalizeCode(dto.Barcode);
        var internationalCode = NormalizeCode(dto.InternationalCode);
        var stockCode = NormalizeCode(dto.StockProductCode);

        var name = dto.Name?.Trim();
        var nameEn = dto.NameEn?.Trim();

        Compare(nameof(Product.CategoryId), product.CategoryId, dto.CategoryId);
        Compare(nameof(Product.BrandId), product.BrandId, dto.BrandId);

        Compare(nameof(Product.Barcode), product.Barcode, barcode);
        Compare(nameof(Product.InternationalCode), product.InternationalCode, internationalCode);
        Compare(nameof(Product.StockProductCode), product.StockProductCode, stockCode);

        Compare(nameof(Product.Name), product.Name, name);
        Compare(nameof(Product.NameEn), product.NameEn, nameEn);

        Compare(nameof(Product.Slug), product.Slug, NormalizeNullable(dto.Slug));
        Compare(nameof(Product.Description), product.Description, dto.Description);
        Compare(nameof(Product.DescriptionEn), product.DescriptionEn, dto.DescriptionEn);

        Compare(nameof(Product.SearchKeywords), product.SearchKeywords, NormalizeNullable(dto.SearchKeywords));

        Compare(nameof(Product.DosageForm), product.DosageForm, NormalizeNullable(dto.DosageForm));
        Compare(nameof(Product.Strength), product.Strength, NormalizeNullable(dto.Strength));
        Compare(nameof(Product.PackSize), product.PackSize, NormalizeNullable(dto.PackSize));
        Compare(nameof(Product.Unit), product.Unit, NormalizeNullable(dto.Unit));

        Compare(nameof(Product.RequiresPrescription), product.RequiresPrescription, dto.RequiresPrescription);
        Compare(nameof(Product.EarnPoints), product.EarnPoints, dto.EarnPoints);
        Compare(nameof(Product.HasExpiry), product.HasExpiry, dto.HasExpiry);

        Compare(nameof(Product.AgeRestricted), product.AgeRestricted, dto.AgeRestricted);
        Compare(nameof(Product.MinAge), product.MinAge, dto.MinAge);

        Compare(nameof(Product.RequiresColdChain), product.RequiresColdChain, dto.RequiresColdChain);
        Compare(nameof(Product.ControlledSubstance), product.ControlledSubstance, dto.ControlledSubstance);
        Compare(nameof(Product.StorageConditions), product.StorageConditions, NormalizeNullable(dto.StorageConditions));

        Compare(nameof(Product.IsTaxable), product.IsTaxable, dto.IsTaxable);
        Compare(nameof(Product.VatRate), product.VatRate, dto.VatRate);
        Compare(nameof(Product.TaxCategoryCode), product.TaxCategoryCode, NormalizeNullable(dto.TaxCategoryCode));

        Compare(nameof(Product.MinOrderQty), product.MinOrderQty, dto.MinOrderQty);
        Compare(nameof(Product.MaxOrderQty), product.MaxOrderQty, dto.MaxOrderQty);
        Compare(nameof(Product.MaxPerDayQty), product.MaxPerDayQty, dto.MaxPerDayQty);

        Compare(nameof(Product.IsReturnable), product.IsReturnable, dto.IsReturnable);
        Compare(nameof(Product.ReturnWindowDays), product.ReturnWindowDays, dto.ReturnWindowDays);

        Compare(nameof(Product.AllowSplitSale), product.AllowSplitSale, dto.AllowSplitSale);
        Compare(nameof(Product.SplitLevel), product.SplitLevel, dto.SplitLevel);

        Compare(nameof(Product.WeightGrams), product.WeightGrams, dto.WeightGrams);
        Compare(nameof(Product.LengthMm), product.LengthMm, dto.LengthMm);
        Compare(nameof(Product.WidthMm), product.WidthMm, dto.WidthMm);
        Compare(nameof(Product.HeightMm), product.HeightMm, dto.HeightMm);

        Compare(nameof(Product.TrackInventory), product.TrackInventory, dto.TrackInventory);
        Compare(nameof(Product.IsFeatured), product.IsFeatured, dto.IsFeatured);
        Compare(nameof(Product.IsActive), product.IsActive, dto.IsActive);

        return logs;
    }

    private static void ApplyProductUpdate(Product product, ProductUpdateDto dto)
    {
        product.CategoryId = dto.CategoryId;
        product.BrandId = dto.BrandId;

        product.Barcode = NormalizeCode(dto.Barcode);
        product.InternationalCode = NormalizeCode(dto.InternationalCode);
        product.StockProductCode = NormalizeCode(dto.StockProductCode);

        product.Name = dto.Name.Trim();
        product.NameEn = dto.NameEn.Trim();

        product.Slug = NormalizeNullable(dto.Slug);
        product.Description = dto.Description;
        product.DescriptionEn = dto.DescriptionEn;

        product.SearchKeywords = NormalizeNullable(dto.SearchKeywords);

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

        product.NormalizedName = NormalizeForSearch(product.Name);
        product.NormalizedNameEn = NormalizeForSearch(product.NameEn);
    }


    // Audit

    public async Task<AppResponse<List<ProductAuditEventDto>>> GetProductAuditAsync(
    int productId, int skip, int take, CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault();

            // نجيب Logs للمنتج (الأحدث أولاً)
            var logs = await unitOfWork.ProductAuditLogs.GetAllAsync(
                criteria: l => l.ProductId == productId && l.PharmacyId == pharmacyId,
                orderBy: q => q.OrderByDescending(x => x.ChangeDate).ThenByDescending(x => x.Id),
                asNoTracking: true,
                ct: ct);

            // نجمع حسب OperationId ونطبق paging على مستوى الـ events
            var events = logs
                .GroupBy(l => l.OperationId)
                .OrderByDescending(g => g.Max(x => x.ChangeDate))
                .Skip(skip)
                .Take(take)
                .Select(g =>
                {
                    var header = g.OrderByDescending(x => x.ChangeDate).First();
                    return new ProductAuditEventDto
                    {
                        OperationId = g.Key,
                        ChangeType = header.ChangeType,
                        ChangedBy = header.ChangedBy,
                        ChangeDate = header.ChangeDate,
                        Reason = header.Reason,
                        Changes = g
                            .Where(x => x.FieldName is not null)
                            .OrderBy(x => x.FieldName)
                            .Select(x => new ProductAuditChangeDto
                            {
                                FieldName = x.FieldName!,
                                OldValue = x.OldValue,
                                NewValue = x.NewValue
                            })
                            .ToList()
                    };
                })
                .ToList();

            return AppResponse<List<ProductAuditEventDto>>.Ok(events);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetProductAuditAsync failed for ProductId={ProductId}", productId);
            return AppResponse<List<ProductAuditEventDto>>.InternalError("Failed to load product audit");
        }
    }


    public async Task<AppResponse<List<ProductAuditEventDto>>> SearchProductAuditAsync(
    string? userId, DateTime? fromUtc, DateTime? toUtc, int skip, int take, CancellationToken ct)
    {
        try
        {
            var pharmacyId = GetPharmacyIdOrDefault();

            // criteria ديناميكي
            bool HasFrom = fromUtc.HasValue;
            bool HasTo = toUtc.HasValue;
            bool HasUser = !string.IsNullOrWhiteSpace(userId);

            var logs = await unitOfWork.ProductAuditLogs.GetAllAsync(
                criteria: l =>
                    l.PharmacyId == pharmacyId
                    && (!HasUser || l.ChangedBy == userId)
                    && (!HasFrom || l.ChangeDate >= fromUtc!.Value)
                    && (!HasTo || l.ChangeDate < toUtc!.Value),
                orderBy: q => q.OrderByDescending(x => x.ChangeDate).ThenByDescending(x => x.Id),
                asNoTracking: true,
                ct: ct);

            var events = logs
                .GroupBy(l => l.OperationId)
                .OrderByDescending(g => g.Max(x => x.ChangeDate))
                .Skip(skip)
                .Take(take)
                .Select(g =>
                {
                    var header = g.OrderByDescending(x => x.ChangeDate).First();
                    return new ProductAuditEventDto
                    {
                        OperationId = g.Key,
                        ChangeType = header.ChangeType,
                        ChangedBy = header.ChangedBy,
                        ChangeDate = header.ChangeDate,
                        Reason = header.Reason,
                        Changes = g
                            .Where(x => x.FieldName is not null)
                            .OrderBy(x => x.FieldName)
                            .Select(x => new ProductAuditChangeDto
                            {
                                FieldName = x.FieldName!,
                                OldValue = x.OldValue,
                                NewValue = x.NewValue
                            })
                            .ToList()
                    };
                })
                .ToList();

            return AppResponse<List<ProductAuditEventDto>>.Ok(events);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SearchProductAuditAsync failed");
            return AppResponse<List<ProductAuditEventDto>>.InternalError("Failed to search audit");
        }
    }

    // end Audit


    public async Task<AppResponse> UpdateProductAsync2(int productId, ProductUpdateDto dto, CancellationToken ct)
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

    private static string? ToAuditString(object? value)
    {
        if (value is null) return null;

        return value switch
        {
            DateTime dt => dt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            float f => f.ToString(CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            _ => value.ToString()
        };
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
    #endregion
}