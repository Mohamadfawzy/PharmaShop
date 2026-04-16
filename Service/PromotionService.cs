namespace Service;

using Contracts;
using Contracts.IServices;
using Entities.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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


}