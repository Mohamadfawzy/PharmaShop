using Contracts;
using Contracts.IServices;
using Entities.Models;
using FluentValidation;
using Shared.Enums;
using Shared.Models.Dtos.Banners;
using Shared.Responses;

namespace Service;

public sealed class BannerService : IBannerService
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<BannerCreateDto> _createValidator;
    private readonly IValidator<BannerUpdateDto> _updateValidator;

    public BannerService(IUnitOfWork uow, IValidator<BannerCreateDto> createValidator, IValidator<BannerUpdateDto> updateValidator)
    {
        _uow = uow;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<AppResponse<List<BannerDto>>> GetPublicAsync(BannerPlacement? placement, string publicBaseUrl, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var rows = await _uow.Banners.GetAllAsync(
            criteria: x => x.IsActive && x.DeletedAt == null
                && (!x.StartsAt.HasValue || x.StartsAt <= now)
                && (!x.EndsAt.HasValue || x.EndsAt >= now)
                && (!placement.HasValue || x.Placement == (byte)placement.Value),
            orderBy: q => q.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id),
            ct: ct);
        return AppResponse<List<BannerDto>>.Ok(rows.Select(x => ToDto(x, publicBaseUrl)).ToList(), "Banners retrieved successfully");
    }

    public async Task<AppResponse<List<BannerDto>>> SearchAdminAsync(BannerListQueryDto query, string publicBaseUrl, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var name = string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim();
        var count = await _uow.Banners.CountAsync(x => x.DeletedAt == null
            && (!query.Placement.HasValue || x.Placement == (byte)query.Placement.Value)
            && (!query.IsActive.HasValue || x.IsActive == query.IsActive.Value)
            && (name == null || x.Name.Contains(name)), ct);
        var rows = await _uow.Banners.GetAllAsync(
            criteria: x => x.DeletedAt == null
                && (!query.Placement.HasValue || x.Placement == (byte)query.Placement.Value)
                && (!query.IsActive.HasValue || x.IsActive == query.IsActive.Value)
                && (name == null || x.Name.Contains(name)),
            orderBy: q => q.OrderBy(x => x.Placement).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Id),
            skip: (page - 1) * pageSize, take: pageSize, ct: ct);
        return AppResponse<List<BannerDto>>.Ok(rows.Select(x => ToDto(x, publicBaseUrl)).ToList(),
            PaginationInfo.Create(page, pageSize, count), "Banners retrieved successfully");
    }

    public async Task<AppResponse<BannerDto>> GetAdminByIdAsync(int id, string publicBaseUrl, CancellationToken ct)
    {
        if (id <= 0) return AppResponse<BannerDto>.ValidationError("A valid banner id is required.");
        var row = await _uow.Banners.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, asNoTracking: true, ct: ct);
        return row is null ? AppResponse<BannerDto>.NotFound("Banner not found") : AppResponse<BannerDto>.Ok(ToDto(row, publicBaseUrl));
    }

    public async Task<AppResponse<BannerDto>> CreateAsync(BannerCreateDto dto, string imageUrl, string thumbnailUrl, string publicBaseUrl, CancellationToken ct)
    {
        var validation = await ValidateAsync(dto, ct);
        if (validation is not null) return validation;
        var now = DateTime.UtcNow;
        var row = new PromotionalBanner
        {
            Name = dto.Name.Trim(), Placement = (byte)dto.Placement, ActionType = (byte)dto.ActionType,
            TargetId = dto.TargetId, ParentTargetId = dto.ParentTargetId,
            ActionUrl = Normalize(dto.ActionUrl), ImageUrl = imageUrl, ThumbnailUrl = thumbnailUrl,
            AltText = Normalize(dto.AltText), DisplayOrder = dto.DisplayOrder, IsActive = dto.IsActive,
            StartsAt = dto.StartsAt, EndsAt = dto.EndsAt, CreatedAt = now
        };
        await _uow.Banners.AddAsync(row, ct);
        await _uow.CompleteAsync(ct);
        return AppResponse<BannerDto>.Created(ToDto(row, publicBaseUrl), $"/api/v1/admin/banners/{row.Id}", "Banner created successfully");
    }

    public async Task<AppResponse<BannerDto>> UpdateAsync(int id, BannerUpdateDto dto, string? imageUrl, string? thumbnailUrl, string publicBaseUrl, CancellationToken ct)
    {
        if (id <= 0) return AppResponse<BannerDto>.ValidationError("A valid banner id is required.");
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) return ValidationFailure<BannerDto>(validation);
        var row = await _uow.Banners.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, asNoTracking: false, ct: ct);
        if (row is null) return AppResponse<BannerDto>.NotFound("Banner not found");
        var targetError = await ValidateTargetAsync(dto.ActionType, dto.TargetId, dto.ParentTargetId, ct);
        if (targetError is not null) return AppResponse<BannerDto>.ValidationErrors(targetError);

        row.Name = dto.Name.Trim(); row.Placement = (byte)dto.Placement; row.ActionType = (byte)dto.ActionType;
        row.TargetId = dto.TargetId; row.ParentTargetId = dto.ParentTargetId; row.ActionUrl = Normalize(dto.ActionUrl);
        row.AltText = Normalize(dto.AltText); row.DisplayOrder = dto.DisplayOrder; row.IsActive = dto.IsActive;
        row.StartsAt = dto.StartsAt; row.EndsAt = dto.EndsAt; row.UpdatedAt = DateTime.UtcNow;
        if (imageUrl is not null) { row.ImageUrl = imageUrl; row.ThumbnailUrl = thumbnailUrl!; }
        await _uow.Banners.UpdateAsync(row, ct);
        await _uow.CompleteAsync(ct);
        return AppResponse<BannerDto>.Ok(ToDto(row, publicBaseUrl), "Banner updated successfully");
    }

    public async Task<AppResponse<int>> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var row = await _uow.Banners.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, asNoTracking: false, ct: ct);
        if (row is null) return AppResponse<int>.NotFound("Banner not found");
        row.DeletedAt = DateTime.UtcNow; row.UpdatedAt = row.DeletedAt; row.IsActive = false;
        await _uow.Banners.UpdateAsync(row, ct); await _uow.CompleteAsync(ct);
        return AppResponse<int>.Ok(row.Id, "Banner deleted successfully");
    }

    public async Task<AppResponse<int>> SetActiveAsync(int id, bool isActive, CancellationToken ct)
    {
        var row = await _uow.Banners.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, asNoTracking: false, ct: ct);
        if (row is null) return AppResponse<int>.NotFound("Banner not found");
        row.IsActive = isActive; row.UpdatedAt = DateTime.UtcNow;
        await _uow.Banners.UpdateAsync(row, ct); await _uow.CompleteAsync(ct);
        return AppResponse<int>.Ok(row.Id, isActive ? "Banner activated" : "Banner deactivated");
    }

    public async Task<AppResponse<int>> ReorderAsync(BannerReorderDto dto, CancellationToken ct)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            return AppResponse<int>.ValidationErrors(new() { ["items"] = ["At least one order item is required."] });
        if (dto.Items.Any(x => x.Id <= 0 || x.DisplayOrder < 0) || dto.Items.Select(x => x.Id).Distinct().Count() != dto.Items.Count)
            return AppResponse<int>.ValidationErrors(new() { ["items"] = ["Ids must be unique and positive; DisplayOrder cannot be negative."] });
        var ids = dto.Items.Select(x => x.Id).ToHashSet();
        var rows = await _uow.Banners.GetAllAsync(criteria: x => ids.Contains(x.Id) && x.DeletedAt == null,
            asNoTracking: false, ct: ct);
        if (rows.Count != ids.Count) return AppResponse<int>.NotFound("One or more active banners were not found");
        await using var tx = await _uow.BeginTransactionAsync(ct);
        try
        {
            foreach (var row in rows)
            {
                row.DisplayOrder = dto.Items.Single(x => x.Id == row.Id).DisplayOrder;
                row.UpdatedAt = DateTime.UtcNow;
            }
            await _uow.CompleteAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch { await tx.RollbackAsync(ct); throw; }
        return AppResponse<int>.Ok(rows.Count, "Banner order updated successfully");
    }

    private async Task<AppResponse<BannerDto>?> ValidateAsync(BannerCreateDto dto, CancellationToken ct)
    {
        var result = await _createValidator.ValidateAsync(dto, ct);
        if (!result.IsValid) return ValidationFailure<BannerDto>(result);
        var targetError = await ValidateTargetAsync(dto.ActionType, dto.TargetId, dto.ParentTargetId, ct);
        return targetError is null ? null : AppResponse<BannerDto>.ValidationErrors(targetError);
    }

    private async Task<Dictionary<string, string[]>?> ValidateTargetAsync(BannerActionType type, int? targetId, int? parentTargetId, CancellationToken ct)
    {
        var valid = type switch
        {
            BannerActionType.None or BannerActionType.ExternalUrl => true,
            BannerActionType.Product => await _uow.Products.AnyAsync(x => x.Id == targetId && x.DeletedAt == null && x.IsActive, ct),
            BannerActionType.Offer => await _uow.Promotions.AnyAsync(x => x.Id == targetId && x.DeletedAt == null, ct),
            BannerActionType.Category => await CategoryTargetExistsAsync(targetId, parentTargetId, ct),
            BannerActionType.Brand => await _uow.Companies.AnyAsync(x => x.Id == targetId && x.DeletedAt == null && x.IsActive, ct),
            _ => false
        };
        return valid ? null : new Dictionary<string, string[]> { ["targetId"] = ["The selected action target does not exist or is unavailable."] };
    }

    private async Task<bool> CategoryTargetExistsAsync(int? targetId, int? parentTargetId, CancellationToken ct)
    {
        if (!targetId.HasValue) return false;
        if (parentTargetId.HasValue)
            return await _uow.Categories.AnyAsync(x => x.Id == targetId && x.ParentCategoryId == parentTargetId && !x.IsDeleted && x.IsActive, ct)
                && await _uow.Categories.AnyAsync(x => x.Id == parentTargetId && !x.IsDeleted && x.IsActive, ct);
        return await _uow.Categories.AnyAsync(x => x.Id == targetId && !x.IsDeleted && x.IsActive, ct);
    }

    private static AppResponse<T> ValidationFailure<T>(FluentValidation.Results.ValidationResult result)
        => AppResponse<T>.ValidationErrors(result.Errors.GroupBy(x => x.PropertyName)
            .ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).Distinct().ToArray()));

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static BannerDto ToDto(PromotionalBanner row, string publicBaseUrl) => new()
    {
        Id = row.Id, Name = row.Name, Placement = (BannerPlacement)row.Placement, ActionType = (BannerActionType)row.ActionType,
        TargetId = row.TargetId, ParentTargetId = row.ParentTargetId, ActionUrl = row.ActionUrl,
        ImageUrl = MakeAbsolute(publicBaseUrl, row.ImageUrl), ThumbnailUrl = MakeAbsolute(publicBaseUrl, row.ThumbnailUrl), AltText = row.AltText, DisplayOrder = row.DisplayOrder,
        IsActive = row.IsActive, StartsAt = row.StartsAt, EndsAt = row.EndsAt, CreatedAt = row.CreatedAt, UpdatedAt = row.UpdatedAt
    };

    private static string MakeAbsolute(string baseUrl, string relativeUrl)
        => new Uri(new Uri(baseUrl.TrimEnd('/') + "/"), relativeUrl.TrimStart('/')).ToString();
}
