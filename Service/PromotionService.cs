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

    public PromotionService(IUnitOfWork unitOfWork, IValidator<PromotionCreateDto> validator)
    {
        this.unitOfWork = unitOfWork;
        _validator = validator;
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
}