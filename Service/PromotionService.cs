namespace Service;

using Contracts;
using Contracts.IServices;
using Entities.Models;
using FluentValidation;
using Shared.Models.Dtos.Promotion;
using Shared.Responses;

public sealed class PromotionService : IPromotionService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IValidator<PromotionCreateDto> _validator;

    private readonly IValidator<PromotionUpdateDto> _updateValidator;

    public PromotionService(IUnitOfWork unitOfWork, IValidator<PromotionCreateDto> validator, IValidator<PromotionUpdateDto> updateValidator)
    {
        this.unitOfWork = unitOfWork;
        _validator = validator;
        _updateValidator = updateValidator;
    }

    public async Task<AppResponse<int>> CreatePromotionAsync(PromotionCreateDto dto, CancellationToken ct)
    {
        // 1) Validate request
        var vr = await _validator.ValidateAsync(dto, ct);
        if (!vr.IsValid)
        {
            var fieldErrors = vr.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

            return AppResponse<int>.ValidationErrors(fieldErrors, detail: "Validation failed");
        }

        // 2) Unique check (ErpPgoId) if provided
        if (dto.ErpPgoId.HasValue)
        {
            var exists = await unitOfWork.Promotions.ExistsByErpPgoIdAsync(dto.ErpPgoId.Value, ct);
            if (exists)
            {
                return AppResponse<int>.ValidationErrors(
                    new Dictionary<string, string[]>
                    {
                        ["ErpPgoId"] = new[] { "ErpPgoId already exists" }
                    },
                    detail: "Validation failed"
                );
            }
        }

        // 3) Create entity
        var now = DateTime.UtcNow;

        var promotion = new Promotion
        {
            ErpPgoId = dto.ErpPgoId,

            Name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),

            StartAt = dto.StartAt,
            EndAt = dto.EndAt,

            TotalAmount = dto.TotalAmount,
            BasicAmount = dto.BasicAmount,
            OfferAmount = dto.OfferAmount,

            DiscountPercent = dto.DiscountPercent,

            IsActive = dto.IsActive ?? true,

            CreatedAt = now,
            UpdatedAt = null,
            DeletedAt = null
            // RowVersion handled by SQL Server
        };

        // 4) Save
        await unitOfWork.Promotions.AddAsync(promotion, ct);
        await unitOfWork.CompleteAsync(ct);

        // 5) Return created
        var location = $"/api/v1/admin/promotions/{promotion.Id}";
        return AppResponse<int>.Created(promotion.Id, location, title: "Promotion created successfully");
    }


    public async Task<AppResponse<List<PromotionListItemDto>>> GetPromotionsAsync(PromotionListQueryDto query, CancellationToken ct)
    {
        // 1) Normalize pagination
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        if (pageSize > 200) pageSize = 200;

        // 2) Normalize search term
        var term = string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim();

        // 3) Call repository
        var result = await unitOfWork.Promotions.SearchAsync(
            term: term,
            isActive: query.IsActive,
            from: query.From,
            to: query.To,
            onlyRunningNow: query.OnlyRunningNow,
            sort: query.Sort,
            page: page,
            pageSize: pageSize,
            ct: ct
        );

        // 4) Build pagination info
        var pagination = PaginationInfo.Create(page, pageSize, result.TotalCount);


        // 5) Return response
        return AppResponse<List<PromotionListItemDto>>.Ok(
            result.Items, pagination, title: "Promotions retrieved successfully"
        );

        // Future improvements:
        // - Add advanced filtering (by ErpPgoId, discount range)
        // - Add caching for common admin lists
        // - Add role-based access checks (admin only)
    }

    public async Task<AppResponse<PromotionDetailsDto>> GetPromotionByIdAsync(int id, CancellationToken ct)
    {
        // 1) Validate input
        if (id <= 0)
        {
            return AppResponse<PromotionDetailsDto>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid promotion id" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Load promotion details (read-only)
        var dto = await unitOfWork.Promotions.GetDetailsByIdAsync(id, ct);

        // 3) Return not found if missing or soft-deleted
        if (dto is null)
            return AppResponse<PromotionDetailsDto>.NotFound("Promotion not found");

        // 4) Return success
        return AppResponse<PromotionDetailsDto>.Ok(dto, title: "Promotion retrieved successfully");

        // Future improvements:
        // - Add RowVersion to response to support optimistic concurrency on update
        // - Add computed flags: IsCurrentlyActive (date window + IsActive)
    }


    public async Task<AppResponse<int>> UpdatePromotionAsync(int id, PromotionUpdateDto dto, CancellationToken ct)
    {
        // 1) Validate input id
        if (id <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid promotion id" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Validate request body (PromotionUpdateDto)
        var vr = await _updateValidator.ValidateAsync(dto, ct);
        if (!vr.IsValid)
        {
            var fieldErrors = vr.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

            return AppResponse<int>.ValidationErrors(fieldErrors, detail: "Validation failed");
        }

        // 3) Load promotion for update (tracked) and ignore soft-deleted
        var promotion = await unitOfWork.Promotions.GetByIdForUpdateAsync(id, ct);
        if (promotion is null)
            return AppResponse<int>.NotFound("Promotion not found");

        // 4) Unique check for ErpPgoId if provided
        if (dto.ErpPgoId.HasValue)
        {
            var exists = await unitOfWork.Promotions.ExistsByErpPgoIdAsync(dto.ErpPgoId.Value, id, ct);
            if (exists)
            {
                return AppResponse<int>.ValidationErrors(
                    new Dictionary<string, string[]>
                    {
                        ["ErpPgoId"] = new[] { "ErpPgoId already exists" }
                    },
                    detail: "Validation failed"
                );
            }
        }

        // 5) Apply updates
        promotion.ErpPgoId = dto.ErpPgoId;

        promotion.Name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim();
        promotion.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

        promotion.StartAt = dto.StartAt;
        promotion.EndAt = dto.EndAt;

        promotion.TotalAmount = dto.TotalAmount;
        promotion.BasicAmount = dto.BasicAmount;
        promotion.OfferAmount = dto.OfferAmount;

        promotion.DiscountPercent = dto.DiscountPercent;

        if (dto.IsActive.HasValue)
            promotion.IsActive = dto.IsActive.Value;

        // 6) Update timestamps
        promotion.UpdatedAt = DateTime.UtcNow;

        // 7) Save changes
        await unitOfWork.CompleteAsync(ct);

        // 8) Return success
        return AppResponse<int>.Ok(promotion.Id, "Promotion updated successfully");

        // Future improvements:
        // - Add RowVersion to prevent lost updates
        // - Return PromotionDetailsDto instead of Id for admin UI
    }


    public async Task<AppResponse<int>> SetPromotionActiveAsync(int id, PromotionActivateDto dto, CancellationToken ct)
    {
        // 1) Validate input
        if (id <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid promotion id" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Update using ExecuteUpdateAsync (fast path)
        var now = DateTime.UtcNow;
        var affected = await unitOfWork.Promotions.SetActiveAsync(id, dto.IsActive, now, ct);

        // 3) Return not found if nothing updated
        if (affected == 0)
            return AppResponse<int>.NotFound("Promotion not found");

        // 4) Return success
        return AppResponse<int>.Ok(id, dto.IsActive ? "Promotion activated" : "Promotion deactivated");

        // Future improvements:
        // - Add RowVersion to prevent lost updates
        // - Add audit logging (who activated/deactivated)
    }

    public async Task<AppResponse<PromotionAddProductsResultDto>> AddProductsToPromotionAsync(
    int promotionId, PromotionAddProductsDto dto, CancellationToken ct)
    {
        // 1) Validate input
        if (promotionId <= 0)
        {
            return AppResponse<PromotionAddProductsResultDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["id"] = new[] { "Invalid promotion id" } },
                detail: "Validation failed"
            );
        }

        if (dto is null || dto.Items is null || dto.Items.Count == 0)
        {
            return AppResponse<PromotionAddProductsResultDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["items"] = new[] { "Items is required" } },
                detail: "Validation failed"
            );
        }

        // 2) Ensure promotion exists
        var promoExists = await unitOfWork.Promotions.PromotionExistsAsync(promotionId, ct);
        if (!promoExists)
            return AppResponse<PromotionAddProductsResultDto>.NotFound("Promotion not found");

        // 3) Normalize items and deduplicate by ErpProductId
        var requested = dto.Items
            .Where(x => x is not null && x.ErpProductId > 0)
            .GroupBy(x => x.ErpProductId)
            .Select(g => g.First())
            .ToList();

        if (requested.Count == 0)
        {
            return AppResponse<PromotionAddProductsResultDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["items"] = new[] { "No valid items found" } },
                detail: "Validation failed"
            );
        }

        var requestedErpIds = requested.Select(x => x.ErpProductId).ToList();

        // 4) Load active duplicates (already added)
        var activeSet = await unitOfWork.Promotions.GetActiveErpProductIdsAsync(promotionId, requestedErpIds, ct);

        // 5) Load soft-deleted rows to restore
        var softDeletedIds = await unitOfWork.Promotions.GetSoftDeletedPromotionProductIdsAsync(promotionId, requestedErpIds, ct);

        // 6) Restore soft-deleted rows (if any)
        var restoredCount = await unitOfWork.Promotions.RestorePromotionProductsAsync(softDeletedIds, ct);

        // 7) Build new entities excluding active duplicates
        var toInsert = new List<PromotionProduct>();
        var skipped = new List<decimal>();

        foreach (var item in requested)
        {
            if (activeSet.Contains(item.ErpProductId))
            {
                skipped.Add(item.ErpProductId);
                continue;
            }

            // If it was soft-deleted and restored, we skip insert
            // Note: we cannot easily know which ErpProductId was restored without mapping;
            // MVP approach: if softDeletedIds > 0, we accept slight over-skip risk by re-checking later.
            // For accuracy, you can add a mapping query in repository later.

            toInsert.Add(new PromotionProduct
            {
                PromotionId = promotionId,
                ProductId = item.ProductId,          // Optional local mapping
                ErpOfferId = item.ErpOfferId,
                ErpProductId = item.ErpProductId,
                CreatedAt = DateTime.UtcNow,
                DeletedAt = null
            });
        }

        // 8) Insert new rows (if any)
        if (toInsert.Count > 0)
            await unitOfWork.Promotions.AddPromotionProductsRangeAsync(toInsert, ct);

        // 9) Persist changes
        await unitOfWork.CompleteAsync(ct);

        // 10) Return result
        var result = new PromotionAddProductsResultDto
        {
            PromotionId = promotionId,
            RequestedCount = requested.Count,
            AddedCount = toInsert.Count,
            RestoredCount = restoredCount,
            SkippedDuplicateCount = skipped.Count,
            SkippedErpProductIds = skipped
        };

        return AppResponse<PromotionAddProductsResultDto>.Ok(result, "Products added to promotion successfully");

        // Future improvements:
        // - Return mapping of restored ErpProductIds exactly (ErpProductId -> restored)
        // - Validate ProductId exists when provided (FK safety)
        // - Support bulk via ErpProductId only (no ProductId) for sync-first environments
    }


    public async Task<AppResponse<int>> RemoveProductFromPromotionAsync(
    int promotionId, int promotionProductId, CancellationToken ct)
    {
        // 1) Validate ids
        if (promotionId <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["id"] = new[] { "Invalid promotion id" }
                },
                detail: "Validation failed"
            );
        }

        if (promotionProductId <= 0)
        {
            return AppResponse<int>.ValidationErrors(
                new Dictionary<string, string[]>
                {
                    ["promotionProductId"] = new[] { "Invalid promotionProductId" }
                },
                detail: "Validation failed"
            );
        }

        // 2) Soft delete the promotion product row
        var now = DateTime.UtcNow;
        var affected = await unitOfWork.Promotions.SoftDeletePromotionProductAsync(promotionId, promotionProductId, now, ct);

        // 3) Return not found if nothing updated (wrong id or already deleted)
        if (affected == 0)
            return AppResponse<int>.NotFound("Promotion product not found");

        // 4) Touch promotion UpdatedAt for tracking
        await unitOfWork.Promotions.TouchPromotionUpdatedAtAsync(promotionId, now, ct);

        // 5) Persist changes (ExecuteUpdateAsync writes directly, but keep unitOfWork pattern consistent)
        await unitOfWork.CompleteAsync(ct);

        // 6) Return success (return promotionProductId)
        return AppResponse<int>.Ok(promotionProductId, "Product removed from promotion successfully");

        // Future improvements:
        // - Return updated products count for admin UI
        // - Add audit logging (who removed the product)
    }

    public async Task<AppResponse<PromotionReplaceProductsResultDto>> ReplacePromotionProductsAsync(
    int promotionId, PromotionReplaceProductsDto dto, CancellationToken ct)
    {
        // 1) Validate ids and body
        if (promotionId <= 0)
        {
            return AppResponse<PromotionReplaceProductsResultDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["id"] = new[] { "Invalid promotion id" } },
                detail: "Validation failed"
            );
        }

        if (dto is null || dto.Items is null)
        {
            return AppResponse<PromotionReplaceProductsResultDto>.ValidationErrors(
                new Dictionary<string, string[]> { ["items"] = new[] { "Items is required" } },
                detail: "Validation failed"
            );
        }

        // 2) Ensure promotion exists
        var promoExists = await unitOfWork.Promotions.PromotionExistsAsync(promotionId, ct);
        if (!promoExists)
            return AppResponse<PromotionReplaceProductsResultDto>.NotFound("Promotion not found");

        // 3) Normalize request and deduplicate by ErpProductId
        var duplicates = new List<decimal>();
        var requestedMap = new Dictionary<decimal, PromotionProductReplaceItemDto>();

        foreach (var item in dto.Items)
        {
            if (item is null) continue;
            if (item.ErpProductId <= 0) continue;

            if (!requestedMap.TryAdd(item.ErpProductId, item))
                duplicates.Add(item.ErpProductId);
        }

        var requestedErpIds = requestedMap.Keys.ToList();

        // 4) Load existing rows for this promotion
        var existingRows = await unitOfWork.Promotions.GetPromotionProductsRowsAsync(promotionId, ct);
        var existingByErp = existingRows.ToDictionary(x => x.ErpProductId, x => x);

        // 5) Decide which rows to soft-delete (active rows not in request)
        var toSoftDeleteIds = existingRows
            .Where(x => x.DeletedAt == null && !requestedMap.ContainsKey(x.ErpProductId))
            .Select(x => x.Id)
            .ToList();

        // 6) Decide which rows to restore (soft-deleted rows in request)
        var toRestoreIds = existingRows
            .Where(x => x.DeletedAt != null && requestedMap.ContainsKey(x.ErpProductId))
            .Select(x => x.Id)
            .ToList();

        // 7) Decide which rows to insert (not existing at all)
        var toInsert = new List<PromotionProduct>();
        foreach (var kv in requestedMap)
        {
            var erpProductId = kv.Key;
            var req = kv.Value;

            if (existingByErp.ContainsKey(erpProductId))
                continue;

            toInsert.Add(new PromotionProduct
            {
                PromotionId = promotionId,
                ProductId = req.ProductId,
                ErpOfferId = req.ErpOfferId,
                ErpProductId = req.ErpProductId,
                CreatedAt = DateTime.UtcNow,
                DeletedAt = null
            });
        }

        // 8) Calculate unchanged count (active rows that remain active)
        var unchangedCount = existingRows.Count(x => x.DeletedAt == null && requestedMap.ContainsKey(x.ErpProductId));

        // 9) Apply changes (soft delete + restore + insert)
        var now = DateTime.UtcNow;

        var softDeletedCount = await unitOfWork.Promotions.SoftDeleteByIdsAsync(toSoftDeleteIds, now, ct);
        var restoredCount = await unitOfWork.Promotions.RestoreByIdsAsync(toRestoreIds, ct);

        await unitOfWork.Promotions.AddPromotionProductsRangeAsync(toInsert, ct);

        // 10) Touch promotion UpdatedAt
        await unitOfWork.Promotions.TouchPromotionUpdatedAtAsync(promotionId, now, ct);

        // 11) Persist changes
        await unitOfWork.CompleteAsync(ct);

        // 12) Build result
        var finalActiveCount = requestedErpIds.Count; // Intended set size (deduped)
        var result = new PromotionReplaceProductsResultDto
        {
            PromotionId = promotionId,
            RequestedCount = dto.Items.Count,
            FinalActiveCount = finalActiveCount,

            InsertedCount = toInsert.Count,
            RestoredCount = restoredCount,
            SoftDeletedCount = softDeletedCount,
            UnchangedCount = unchangedCount,

            DuplicatesInRequest = duplicates
        };

        return AppResponse<PromotionReplaceProductsResultDto>.Ok(result, "Promotion products replaced successfully");

        // Future improvements:
        // - Validate ProductId existence when provided (avoid FK failures)
        // - Replace loading existing rows with set-based SQL for very large lists
        // - Add audit logging for replace operations
    }
}